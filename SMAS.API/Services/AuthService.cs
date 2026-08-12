using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SMAS.API.Data;
using SMAS.API.DTOs;
using SMAS.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SMAS.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly SmasDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(SmasDbContext context, IConfiguration config, ILogger<AuthService>? logger = null)
        {
            _context = context;
            _config = config;
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthService>.Instance;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Email))
                throw new UnauthorizedAccessException("Email is required");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new UnauthorizedAccessException("Password is required");

            var email = dto.Email.Trim().ToLower();
            var password = dto.Password.Trim();

            _logger.LogInformation("[Auth] Login attempt for email: {Email}", email);

            // Try employee login first
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (employee != null)
            {
                bool isPasswordValid = VerifyPassword(password, employee.PasswordHash);
                
                if (!isPasswordValid)
                {
                    _logger.LogWarning("[Auth] Failed login attempt for employee: {Email}", email);
                    throw new UnauthorizedAccessException("Email or password is incorrect");
                }

                // Check approval status for employees
                if (employee.ApprovalStatus == "Pending")
                    throw new UnauthorizedAccessException("Your account is pending admin approval. You will receive an email once approved.");
                
                if (employee.ApprovalStatus == "Rejected")
                    throw new UnauthorizedAccessException("Your account registration was rejected. Please contact support for more information.");

                _logger.LogInformation("[Auth] Successful login for employee: {Email}", email);
                return await GenerateAndPersistTokensAsync(employee.Email, employee.Role, dto.ClientIp, employee.Id, employee.FullName, employee.Phone, employee.ApprovalStatus);
            }

            // Try customer login
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer != null)
            {
                bool isPasswordValid = VerifyPassword(password, customer.PasswordHash);
                
                if (!isPasswordValid)
                {
                    _logger.LogWarning("[Auth] Failed login attempt for customer: {Email}", email);
                    throw new UnauthorizedAccessException("Email or password is incorrect");
                }

                _logger.LogInformation("[Auth] Successful login for customer: {Email}", email);
                return await GenerateAndPersistTokensAsync(customer.Email, "Buyer", dto.ClientIp, customer.Id, customer.FullName, customer.Phone, null);
            }

            _logger.LogWarning("[Auth] No user found with email: {Email}", email);
            throw new UnauthorizedAccessException("Email or password is incorrect");
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingCustomer = await _context.Customers.AnyAsync(c => c.Email == dto.Email);
            var existingEmployee = await _context.Employees.AnyAsync(e => e.Email == dto.Email);
            if (existingCustomer || existingEmployee)
                throw new InvalidOperationException("Email already exists");

            if (dto.Role == "Salesman")
            {
                var employee = new Employee
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    PasswordHash = HashPassword(dto.Password),
                    ApprovalStatus = "Pending",
                    Role = "Salesman"
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // Notify admin of new salesman signup (broadcast notification - employeeId null = admin sees it)
                var notification = new Notification
                {
                    Title = "New Salesman Registration",
                    Message = $"{employee.FullName} ({employee.Email}) has registered as a salesman and is awaiting approval.",
                    NotificationType = "SalesmanRegistered",
                    RelatedId = employee.Id
                };
                _context.Notifications.Add(notification);

                // Audit log
                _context.AuditLogs.Add(new AuditLog
                {
                    EntityName = "Employee",
                    EntityId = employee.Id,
                    Action = "Register",
                    PerformedBy = employee.Email,
                    PerformedAt = DateTime.UtcNow,
                    Details = $"Salesman '{employee.FullName}' registered, awaiting admin approval"
                });
                await _context.SaveChangesAsync();

                // IMPORTANT: do NOT auto-generate authentication tokens for salesmen until admin approves.
                // Return an AuthResponseDto indicating pending approval but without tokens.
                return new AuthResponseDto
                {
                    Token = string.Empty,
                    RefreshToken = string.Empty,
                    Role = employee.Role,
                    Email = employee.Email,
                    UserId = employee.Id,
                    FullName = employee.FullName,
                    Phone = employee.Phone,
                    ApprovalStatus = employee.ApprovalStatus
                };
            }

            var customer = new Customer
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                City = dto.City,
                Province = dto.Province,
                PasswordHash = HashPassword(dto.Password)
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return await GenerateAndPersistTokensAsync(customer.Email, "Buyer", dto.ClientIp, customer.Id, customer.FullName, customer.Phone, null);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string? token = null)
        {
            if (string.IsNullOrEmpty(token)) throw new UnauthorizedAccessException("Refresh token missing");

            var existing = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (existing == null || !existing.IsActive) throw new UnauthorizedAccessException("Invalid refresh token");

            existing.RevokedAt = DateTime.UtcNow;
            existing.RevokedByIp = "system";
            var newRefreshToken = Guid.NewGuid().ToString();
            existing.ReplacedByToken = newRefreshToken;

            var refresh = new RefreshToken
            {
                Token = newRefreshToken,
                Email = existing.Email,
                Role = existing.Role,
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshExpiryDays()),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "system"
            };

            _context.RefreshTokens.Add(refresh);
            await _context.SaveChangesAsync();

            return await GenerateTokenResponseAsync(existing.Email, existing.Role, newRefreshToken);
        }

        public async Task RevokeTokenAsync(string? token = null)
        {
            if (string.IsNullOrEmpty(token)) return;
            var existing = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (existing == null) return;
            existing.RevokedAt = DateTime.UtcNow;
            existing.RevokedByIp = "system";
            await _context.SaveChangesAsync();
        }

        public async Task<AuthResponseDto> GetUserAsync(string email, string role)
        {
            var userInfo = await GetUserInfoByEmailAndRoleAsync(email, role);
            return new AuthResponseDto
            {
                Token = string.Empty,
                RefreshToken = string.Empty,
                Role = role,
                Email = email,
                UserId = userInfo.UserId,
                FullName = userInfo.FullName,
                Phone = userInfo.Phone,
                ApprovalStatus = userInfo.ApprovalStatus
            };
        }

        private int GetRefreshExpiryDays()
        {
            var configValue = _config["Jwt:RefreshExpiryDays"];
            if (int.TryParse(configValue, out var days) && days > 0)
                return days;
            return 7;  // Default to 7 days
        }

        private int GetTokenExpiryMinutes()
        {
            var configValue = _config["Jwt:ExpiryMinutes"];
            if (int.TryParse(configValue, out var minutes) && minutes > 0)
                return minutes;
            return 60;  // Default to 60 minutes
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private AuthResponseDto GenerateTokens(string email, string role)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                _logger.LogError("JWT key is not configured. Check environment variables and appsettings.json");
                throw new InvalidOperationException("Jwt:Key must be configured and cannot be empty. Please check environment variables.");
            }

            var key = Encoding.UTF8.GetBytes(jwtKey);
            if (key.Length <= 32)
            {
                _logger.LogError($"JWT key length is {key.Length} bytes, but must be > 32 bytes (256 bits). Current key: {jwtKey.Substring(0, Math.Min(20, jwtKey.Length))}...");
                throw new InvalidOperationException($"Jwt:Key must be longer than 256 bits (32 bytes) for HS256 signing. Current length: {key.Length} bytes. Please configure a longer key in environment or appsettings.json.");
            }

            var expiry = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes());

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponseDto
            {
                Token = tokenString,
                Role = role,
                Email = email
            };
        }

        private async Task<AuthResponseDto> GenerateAndPersistTokensAsync(string email, string role, string? clientIp, Guid userId, string fullName, string? phone, string? approvalStatus)
        {
            var jwt = GenerateTokens(email, role);
            var refreshToken = Guid.NewGuid().ToString();

            var refresh = new RefreshToken
            {
                Token = refreshToken,
                Email = email,
                Role = role,
                ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:RefreshExpiryDays"] ?? "30")),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = clientIp ?? "unknown"
            };

            _context.RefreshTokens.Add(refresh);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = jwt.Token,
                RefreshToken = refreshToken,
                Role = role,
                Email = email,
                UserId = userId,
                FullName = fullName,
                Phone = phone,
                ApprovalStatus = approvalStatus
            };
        }

        private async Task<AuthResponseDto> GenerateTokenResponseAsync(string email, string role, string refreshToken)
        {
            var jwt = GenerateTokens(email, role);
            var userInfo = await GetUserInfoByEmailAndRoleAsync(email, role);
            return new AuthResponseDto
            {
                Token = jwt.Token,
                RefreshToken = refreshToken,
                Role = role,
                Email = email,
                UserId = userInfo.UserId,
                FullName = userInfo.FullName,
                Phone = userInfo.Phone,
                ApprovalStatus = userInfo.ApprovalStatus
            };
        }

        private async Task<(Guid UserId, string FullName, string? Phone, string? ApprovalStatus)> GetUserInfoByEmailAndRoleAsync(string email, string role)
        {
            if (role == "Buyer")
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (customer == null) throw new KeyNotFoundException("Customer not found");
                return (customer.Id, customer.FullName, customer.Phone, null);
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (employee == null) throw new KeyNotFoundException("Employee not found");
            return (employee.Id, employee.FullName, employee.Phone, employee.ApprovalStatus);
        }

        public async Task ChangePasswordAsync(string email, string role, ChangePasswordDto dto)
        {
            if (role == "Buyer")
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (customer == null) throw new KeyNotFoundException("Customer not found");
                
                if (!VerifyPassword(dto.CurrentPassword, customer.PasswordHash))
                    throw new UnauthorizedAccessException("Current password is incorrect");

                customer.PasswordHash = HashPassword(dto.NewPassword);
                customer.UpdatedAt = DateTime.UtcNow;
                _context.Customers.Update(customer);

                // Audit log
                _context.AuditLogs.Add(new AuditLog
                {
                    EntityName = "Customer",
                    EntityId = customer.Id,
                    Action = "ChangePassword",
                    PerformedBy = email,
                    PerformedAt = DateTime.UtcNow,
                    Details = "Customer changed their password"
                });
                await _context.SaveChangesAsync();
            }
            else
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
                if (employee == null) throw new KeyNotFoundException("Employee not found");

                if (!VerifyPassword(dto.CurrentPassword, employee.PasswordHash))
                    throw new UnauthorizedAccessException("Current password is incorrect");

                employee.PasswordHash = HashPassword(dto.NewPassword);
                employee.UpdatedAt = DateTime.UtcNow;
                _context.Employees.Update(employee);

                // Audit log
                _context.AuditLogs.Add(new AuditLog
                {
                    EntityName = "Employee",
                    EntityId = employee.Id,
                    Action = "ChangePassword",
                    PerformedBy = email,
                    PerformedAt = DateTime.UtcNow,
                    Details = $"{role} changed their password"
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}