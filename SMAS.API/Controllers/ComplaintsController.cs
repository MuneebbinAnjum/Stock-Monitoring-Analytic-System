using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMAS.API.DTOs;
using SMAS.API.Data;
using SMAS.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]


    public class ComplaintsController : ControllerBase
    {
        private readonly SmasDbContext _context;
        private readonly IHubContext<SMAS.API.Hubs.NotificationHub> _hubContext;

        public ComplaintsController(SmasDbContext context, IHubContext<SMAS.API.Hubs.NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var complaints = await _context.Complaints
                    .Include(c => c.Order)
                    .Include(c => c.Customer)
                    .Include(c => c.Messages)
                    .Where(c => !c.IsDeleted)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var dtos = complaints.Select(c => MapToDto(c)).ToList();
                return Ok(new ApiResponse<IEnumerable<ComplaintResponseDto>> { Success = true, Data = dtos });
            }
            catch (Exception ex)
            {
                return Ok(new { error = ex.Message, stackTrace = ex.StackTrace, inner = ex.InnerException?.Message });
            }
        }

        [Authorize(Roles = "Buyer")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyComplaints()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Customer not found" });

            var complaints = await _context.Complaints
                .Include(c => c.Order)
                .Where(c => c.CustomerId == customer.Id && !c.IsDeleted)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var dtos = complaints.Select(c => MapToDto(c));
            return Ok(new ApiResponse<IEnumerable<ComplaintResponseDto>> { Success = true, Data = dtos });
        }

        [Authorize(Roles = "Buyer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ComplaintCreateDto dto)
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
                return Unauthorized(new ApiResponse<string> { Success = false, Message = "Customer not found" });

            Order? order = null;
            if (!string.IsNullOrWhiteSpace(dto.OrderNumber))
            {
                order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == dto.OrderNumber && o.CustomerId == customer.Id);
                if (order == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Order not found or does not belong to you" });
            }
            else
            {
                order = await _context.Orders.OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync(o => o.CustomerId == customer.Id);
                if (order == null)
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "You must have at least one order to file a complaint" });
            }

            var complaint = new Complaint
            {
                OrderId = order.Id,
                CustomerId = customer.Id,
                ComplaintType = dto.ComplaintType,
                Title = dto.Title,
                Description = dto.Description,
                EvidenceImageUrl = dto.EvidenceImageUrl,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);

            // Create notification for admin
            var notification = new Notification
            {
                Title = "New Complaint",
                Message = $"Complaint filed: {dto.Title} on order {order.OrderNumber}",
                NotificationType = "NewComplaint",
                RelatedId = complaint.Id
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            // Broadcast notification to connected clients
            try
            {
                await _hubContext.Clients.All.SendAsync("NotificationCreated", new
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    NotificationType = notification.NotificationType,
                    RelatedId = notification.RelatedId,
                    CreatedAt = notification.CreatedAt
                });
            }
            catch { }

            return Ok(new ApiResponse<ComplaintResponseDto> { Success = true, Data = MapToDto(complaint), Message = "Complaint filed successfully" });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Order)
                .Include(c => c.Customer)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (complaint == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Complaint not found" });

            // Authorization: buyers can only view own complaints
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (role == "Buyer")
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (customer == null || complaint.CustomerId != customer.Id)
                    return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authorized to view this complaint" });
            }

            return Ok(new ApiResponse<ComplaintResponseDto> { Success = true, Data = MapToDto(complaint) });
        }

        [Authorize]
        [HttpPost("{id}/messages")]
        public async Task<IActionResult> PostMessage(Guid id, [FromBody] CreateComplaintMessageDto dto)
        {
            var complaint = await _context.Complaints.Include(c => c.Order).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (complaint == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Complaint not found" });

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            Guid? senderId = null;
            string senderType = "Buyer";

            if (role == "Buyer")
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (customer == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Customer not found" });
                if (complaint.CustomerId != customer.Id) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authorized" });
                senderId = customer.Id;
                senderType = "Buyer";
            }
            else
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
                if (employee == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Employee not found" });
                senderId = employee.Id;
                senderType = "Employee";
            }

            var msg = new ComplaintMessage
            {
                ComplaintId = complaint.Id,
                SenderId = senderId,
                SenderType = senderType,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow
            };
            _context.Add(msg);
            await _context.SaveChangesAsync();

            // Notify the other party
            try
            {
                if (senderType == "Employee")
                {
                    // notify buyer
                    var notification = new Notification
                    {
                        Title = "Complaint Update",
                        Message = $"Admin replied to your complaint: {complaint.Title}",
                        NotificationType = "ComplaintResponse",
                        RelatedId = complaint.Id,
                        CustomerId = complaint.CustomerId
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    try 
                    {
                        await _hubContext.Clients.Group(complaint.CustomerId.ToString()).SendAsync("NotificationCreated", new
                        {
                            Id = notification.Id,
                            Title = notification.Title,
                            Message = notification.Message,
                            NotificationType = notification.NotificationType,
                            RelatedId = notification.RelatedId,
                            CreatedAt = notification.CreatedAt
                        });
                    } catch { }
                }
                else
                {
                    // notify admins
                    var notification = new Notification
                    {
                        Title = "Complaint Message",
                        Message = $"Customer replied on complaint: {complaint.Title}",
                        NotificationType = "ComplaintReply",
                        RelatedId = complaint.Id
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                    
                    try 
                    {
                        await _hubContext.Clients.All.SendAsync("NotificationCreated", new
                        {
                            Id = notification.Id,
                            Title = notification.Title,
                            Message = notification.Message,
                            NotificationType = notification.NotificationType,
                            RelatedId = notification.RelatedId,
                            CreatedAt = notification.CreatedAt
                        });
                    } catch { }
                }
            }
            catch { }

            var result = new ComplaintMessageDto
            {
                Id = msg.Id,
                ComplaintId = msg.ComplaintId,
                SenderId = msg.SenderId,
                SenderType = msg.SenderType,
                Message = msg.Message,
                CreatedAt = msg.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = complaint.Id }, new ApiResponse<ComplaintMessageDto> { Success = true, Data = result });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ComplaintStatusUpdateDto dto)
        {
            var complaint = await _context.Complaints.Include(c => c.Order).FirstOrDefaultAsync(c => c.Id == id);
            if (complaint == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Complaint not found" });

            complaint.Status = dto.Status;
            complaint.AdminNotes = dto.AdminNotes ?? complaint.AdminNotes;

            if (dto.Status == "Resolved" || dto.Status == "Rejected")
                complaint.ResolvedAt = DateTime.UtcNow;

            if (dto.ComplaintType == "Return Request" && dto.ReturnApproved.HasValue)
            {
                complaint.ReturnApproved = dto.ReturnApproved.Value;
                if (dto.ReturnApproved.Value && complaint.Order != null)
                {
                    complaint.Order.Status = "Returned";
                    complaint.Order.UpdatedAt = DateTime.UtcNow;

                    // Restore inventory for each item in the returned order
                    var orderItems = await _context.OrderItems
                        .Where(oi => oi.OrderId == complaint.OrderId)
                        .ToListAsync();

                    foreach (var item in orderItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                            _context.InventoryTransactions.Add(new SMAS.API.Models.InventoryTransaction
                            {
                                ProductId = product.Id,
                                QuantityChange = item.Quantity,
                                Reason = $"Return approved for order {complaint.Order.OrderNumber}",
                                CreatedBy = "Admin"
                            });
                        }
                    }

                    // Audit log
                    _context.AuditLogs.Add(new SMAS.API.Models.AuditLog
                    {
                        EntityName = "Complaint",
                        EntityId = complaint.Id,
                        Action = "ReturnApproved",
                        PerformedBy = "Admin",
                        PerformedAt = DateTime.UtcNow,
                        Details = $"Return approved for order {complaint.Order.OrderNumber}, stock restored"
                    });
                }
            }

            complaint.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify the customer about the update/response
            try
            {
                if (complaint.CustomerId != Guid.Empty)
                {
                    var notification = new Notification
                    {
                        Title = "Complaint Update",
                        Message = !string.IsNullOrWhiteSpace(complaint.AdminNotes)
                            ? $"Your complaint '{complaint.Title}' has an update: {complaint.AdminNotes}"
                            : $"Your complaint '{complaint.Title}' status changed to {complaint.Status}",
                        NotificationType = "ComplaintResponse",
                        RelatedId = complaint.Id,
                        CustomerId = complaint.CustomerId
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    try 
                    {
                        await _hubContext.Clients.Group(complaint.CustomerId.ToString()).SendAsync("NotificationCreated", new
                        {
                            Id = notification.Id,
                            Title = notification.Title,
                            Message = notification.Message,
                            NotificationType = notification.NotificationType,
                            RelatedId = notification.RelatedId,
                            CreatedAt = notification.CreatedAt
                        });
                    } catch { }
                }
            }
            catch { }

            return Ok(new ApiResponse<ComplaintResponseDto> { Success = true, Data = MapToDto(complaint), Message = "Complaint updated" });
        }

        private static ComplaintResponseDto MapToDto(Complaint c)
        {
            return new ComplaintResponseDto
            {
                Id = c.Id,
                OrderId = c.OrderId,
                OrderNumber = c.Order?.OrderNumber ?? "",
                CustomerId = c.CustomerId,
                CustomerName = c.Customer?.FullName ?? "",
                ComplaintType = c.ComplaintType,
                Title = c.Title,
                Description = c.Description,
                Status = c.Status,
                AdminNotes = c.AdminNotes,
                ReturnApproved = c.ReturnApproved,
                EvidenceImageUrl = c.EvidenceImageUrl,
                CreatedAt = c.CreatedAt,
                ResolvedAt = c.ResolvedAt,
                UpdatedAt = c.UpdatedAt
            };
        }
    }
}
