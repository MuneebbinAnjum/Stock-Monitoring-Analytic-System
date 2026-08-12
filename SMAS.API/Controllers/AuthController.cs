using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SMAS.API.DTOs;
using SMAS.API.Services;
using System.Security.Claims;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private const int RefreshTokenExpiryDays = 30;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto?.Email))
                    return BadRequest(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "Email is required" 
                    });

                if (string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "Password is required" 
                    });

                dto.ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var response = await _authService.LoginAsync(dto);

                // set refresh token as HttpOnly secure cookie
                if (!string.IsNullOrEmpty(response.RefreshToken))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
                    };
                    Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);
                }

                return Ok(new ApiResponse<AuthResponseDto> 
                { 
                    Success = true, 
                    Data = response,
                    Message = "Login successful"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred during login. Please try again." 
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "Registration data is required" 
                    });

                dto.ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var response = await _authService.RegisterAsync(dto);

                if (!string.IsNullOrEmpty(response.RefreshToken))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
                    };
                    Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);
                }

                return StatusCode(201, new ApiResponse<AuthResponseDto> 
                { 
                    Success = true, 
                    Data = response,
                    Message = response.ApprovalStatus == "Pending" 
                        ? "Registration successful! Your account is pending admin approval." 
                        : "Registration and login successful!"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred during registration. Please try again." 
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string? token = null)
        {
            try
            {
                // prefer cookie
                var cookie = Request.Cookies["refreshToken"];
                var tokenToUse = !string.IsNullOrEmpty(cookie) ? cookie : token;
                
                if (string.IsNullOrEmpty(tokenToUse))
                    return Unauthorized(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "Refresh token is missing. Please login again." 
                    });

                var response = await _authService.RefreshTokenAsync(tokenToUse);

                // set rotated refresh token in cookie
                if (!string.IsNullOrEmpty(response.RefreshToken))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
                    };
                    Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);
                }

                return Ok(new ApiResponse<AuthResponseDto> 
                { 
                    Success = true, 
                    Data = response,
                    Message = "Token refreshed successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred while refreshing the token." 
                });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string? token = null)
        {
            try
            {
                var cookie = Request.Cookies["refreshToken"];
                var tokenToRevoke = !string.IsNullOrEmpty(cookie) ? cookie : token;
                if (!string.IsNullOrEmpty(tokenToRevoke))
                {
                    await _authService.RevokeTokenAsync(tokenToRevoke);
                }
                // remove cookie
                Response.Cookies.Delete("refreshToken", new CookieOptions { SameSite = SameSiteMode.None, Secure = true });
                return Ok(new ApiResponse<string> 
                { 
                    Success = true, 
                    Message = "Logged out successfully" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred during logout." 
                });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
                    return Unauthorized(new ApiResponse<object> 
                    { 
                        Success = false, 
                        Message = "User information is missing from token." 
                    });

                await _authService.ChangePasswordAsync(email, role, dto);
                return Ok(new ApiResponse<string> 
                { 
                    Success = true, 
                    Message = "Password changed successfully. Please login again." 
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "An error occurred while changing password." 
                });
            }
        }
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role))
            {
                return Unauthorized(new ApiResponse<string> { Success = false, Message = "Invalid token" });
            }

            var response = await _authService.GetUserAsync(email, role);
            return Ok(new ApiResponse<AuthResponseDto> { Success = true, Data = response });
        }


    }
}