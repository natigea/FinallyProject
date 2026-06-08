using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<bool> CreateAdminAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
