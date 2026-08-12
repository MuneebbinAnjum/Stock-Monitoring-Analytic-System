using SMAS.API.DTOs;

namespace SMAS.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(string? token = null);
        Task<AuthResponseDto> GetUserAsync(string email, string role);
        Task RevokeTokenAsync(string? token = null);
        Task ChangePasswordAsync(string email, string role, ChangePasswordDto dto);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}