using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMAS.API.DTOs;
using SMAS.API.Data;
using SMAS.API.Models;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly SmasDbContext _context;

        public NotificationsController(SmasDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? unreadOnly = null, [FromQuery] int? limit = 50)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            // Determine current user id and type
            Guid? currentUserId = null;
            string currentUserType = "Employee";
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (emp != null)
            {
                currentUserId = emp.Id;
                currentUserType = "Employee";
            }
            else
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (customer != null)
                {
                    currentUserId = customer.Id;
                    currentUserType = "Buyer";
                }
            }

            var query = _context.Notifications.AsQueryable();

            if (role == "Admin")
            {
                // Admin sees all notifications
            }
            else if (role == "Salesman" || role == "Employee")
            {
                if (emp != null)
                {
                    // Salesmen/employees should see broadcast notifications (EmployeeId == null) and those targeted to them.
                    // Do NOT show admin-specific notifications like SalesmanRegistered to regular employees.
                    query = query.Where(n => n.EmployeeId == null || n.EmployeeId == emp.Id);
                }
                else
                {
                    query = query.Where(n => n.EmployeeId == null);
                }
            }
            else if (role == "Buyer")
            {
                // Buyers see only their own notifications
                query = query.Where(n => n.CustomerId == currentUserId);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit ?? 50)
                .ToListAsync();

            // Load read entries for this user
            var readIds = new HashSet<Guid>();
            if (currentUserId.HasValue)
            {
                readIds = (await _context.NotificationReads
                    .Where(nr => nr.UserId == currentUserId.Value && nr.UserType == currentUserType && !nr.IsDeleted)
                    .Select(nr => nr.NotificationId)
                    .ToListAsync()).ToHashSet();
            }

            var dtos = notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType,
                RelatedId = n.RelatedId,
                IsRead = currentUserId.HasValue ? readIds.Contains(n.Id) : n.IsRead,
                CreatedAt = n.CreatedAt
            });

            if (unreadOnly == true)
                dtos = dtos.Where(d => !d.IsRead);

            return Ok(new ApiResponse<IEnumerable<NotificationResponseDto>> { Success = true, Data = dtos });
        }

        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetAllNotificationHistory([FromQuery] int? limit = 100)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // Only admin can view full history
            if (role != "Admin")
                return Unauthorized(new ApiResponse<string> { Success = false, Message = "Only admins can view notification history" });

            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit ?? 100)
                .ToListAsync();

            // For admin, mark read status per admin user
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            Guid? adminId = null;
            if (!string.IsNullOrEmpty(email))
            {
                var admin = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
                if (admin != null) adminId = admin.Id;
            }

            var readIds = new HashSet<Guid>();
            if (adminId.HasValue)
            {
                readIds = (await _context.NotificationReads
                    .Where(nr => nr.UserId == adminId.Value && nr.UserType == "Employee" && !nr.IsDeleted)
                    .Select(nr => nr.NotificationId)
                    .ToListAsync()).ToHashSet();
            }

            var dtos = notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType,
                RelatedId = n.RelatedId,
                IsRead = adminId.HasValue ? readIds.Contains(n.Id) : n.IsRead,
                CreatedAt = n.CreatedAt
            });

            return Ok(new ApiResponse<IEnumerable<NotificationResponseDto>> { Success = true, Data = dtos });
        }

        [Authorize]
        [HttpGet("count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            // Determine current user id and type
            Guid? currentUserId = null;
            string currentUserType = "Employee";
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (emp != null)
            {
                currentUserId = emp.Id;
                currentUserType = "Employee";
            }
            else
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (customer != null)
                {
                    currentUserId = customer.Id;
                    currentUserType = "Buyer";
                }
            }

            var visibleQuery = _context.Notifications.AsQueryable();
            if (role == "Admin") { }
            else if (role == "Salesman" || role == "Employee")
            {
                if (emp != null) visibleQuery = visibleQuery.Where(n => n.EmployeeId == null || n.EmployeeId == emp.Id || n.NotificationType == "SalesmanRegistered");
                else visibleQuery = visibleQuery.Where(n => n.EmployeeId == null);
            }
            else if (role == "Buyer")
            {
                visibleQuery = visibleQuery.Where(n => n.CustomerId == currentUserId);
            }

            if (!currentUserId.HasValue)
            {
                // No user context -> return zero
                return Ok(new ApiResponse<int> { Success = true, Data = 0 });
            }

            var visibleIds = await visibleQuery.Select(n => n.Id).ToListAsync();
            var readIds = await _context.NotificationReads
                .Where(nr => nr.UserId == currentUserId.Value && nr.UserType == currentUserType && !nr.IsDeleted)
                .Select(nr => nr.NotificationId)
                .ToListAsync();

            var unreadCount = visibleIds.Except(readIds).Count();
            return Ok(new ApiResponse<int> { Success = true, Data = unreadCount });
        }

        [Authorize]
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Notification not found" });

            // Determine current user id and type
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            Guid? currentUserId = null;
            string currentUserType = "Employee";
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (emp != null)
            {
                currentUserId = emp.Id;
                currentUserType = "Employee";
            }
            else
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (customer != null)
                {
                    currentUserId = customer.Id;
                    currentUserType = "Buyer";
                }
            }

            if (!currentUserId.HasValue)
                return Unauthorized(new ApiResponse<string> { Success = false, Message = "User context not found" });

            // Create or update a NotificationRead entry for this user + notification
            var existing = await _context.NotificationReads.FirstOrDefaultAsync(nr => nr.NotificationId == id && nr.UserId == currentUserId.Value && nr.UserType == currentUserType && !nr.IsDeleted);
            if (existing == null)
            {
                var nr = new NotificationRead
                {
                    NotificationId = id,
                    UserId = currentUserId.Value,
                    UserType = currentUserType,
                    IsRead = true,
                    ReadAt = DateTime.UtcNow
                };
                _context.NotificationReads.Add(nr);
            }
            else
            {
                existing.IsRead = true;
                existing.ReadAt = DateTime.UtcNow;
                _context.NotificationReads.Update(existing);
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Marked as read" });
        }

        [Authorize]
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            // Determine current user id and type
            Guid? currentUserId = null;
            string currentUserType = "Employee";
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (emp != null)
            {
                currentUserId = emp.Id;
                currentUserType = "Employee";
            }
            else
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (customer != null)
                {
                    currentUserId = customer.Id;
                    currentUserType = "Buyer";
                }
            }

            if (!currentUserId.HasValue)
                return Unauthorized(new ApiResponse<string> { Success = false, Message = "User context not found" });

            // Build visible notifications query for this user
            var visibleQuery = _context.Notifications.AsQueryable();
            if (role == "Admin") { }
            else if (role == "Salesman" || role == "Employee")
            {
                if (emp != null) visibleQuery = visibleQuery.Where(n => n.EmployeeId == null || n.EmployeeId == emp.Id);
                else visibleQuery = visibleQuery.Where(n => n.EmployeeId == null);
            }
            else if (role == "Buyer")
            {
                visibleQuery = visibleQuery.Where(n => n.CustomerId == currentUserId);
            }

            var visibleIds = await visibleQuery.Select(n => n.Id).ToListAsync();

            // For all visible notifications where this user has no NotificationRead, create one marking read
            var existingReads = await _context.NotificationReads
                .Where(nr => nr.UserId == currentUserId.Value && nr.UserType == currentUserType && !nr.IsDeleted)
                .Select(nr => nr.NotificationId)
                .ToListAsync();

            var toMark = visibleIds.Except(existingReads).ToList();
            foreach (var nid in toMark)
            {
                _context.NotificationReads.Add(new NotificationRead
                {
                    NotificationId = nid,
                    UserId = currentUserId.Value,
                    UserType = currentUserType,
                    IsRead = true,
                    ReadAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = $"Marked {toMark.Count} notifications as read" });
        }
    }
}

