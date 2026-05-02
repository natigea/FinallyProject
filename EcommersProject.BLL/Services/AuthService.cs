using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class AuthService(IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var normalized = dto.Email.Trim().ToLowerInvariant();
        var users = await unitOfWork.Users.FindAsync(
            u => u.Email == normalized && u.IsActive, cancellationToken);

        var user = users.FirstOrDefault();
        if (user is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

        return MapToResponse(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var normalized = dto.Email.Trim().ToLowerInvariant();
        var existing = await unitOfWork.Users.FindAsync(u => u.Email == normalized, cancellationToken);
        if (existing.Any())
            throw new InvalidOperationException($"Пользователь с email {dto.Email} уже существует.");

        var user = new User
        {
            Email = normalized,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Customer,
            IsActive = true
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(user);
    }

    public async Task<bool> CreateAdminAsync(
        string email, string password, string firstName, string lastName,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var existing = await unitOfWork.Users.FindAsync(u => u.Email == normalized, cancellationToken);
        if (existing.Any()) return false;

        var admin = new User
        {
            Email = normalized,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Admin,
            IsActive = true
        };

        await unitOfWork.Users.AddAsync(admin, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AuthResponseDto MapToResponse(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString());
}
