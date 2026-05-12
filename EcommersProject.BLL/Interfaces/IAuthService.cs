using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<bool> CreateAdminAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default);
}
