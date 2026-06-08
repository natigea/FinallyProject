using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcommersProject.BLL.Services;

public class AuthService(IUnitOfWork unitOfWork, ILogger<AuthService> logger) : IAuthService
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
            throw new InvalidOperationException($"User with email {dto.Email} already exists.");

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

        // Use IgnoreQueryFilters to find even soft-deleted admins
        var existing = await unitOfWork.Context.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        if (existing != null)
        {
            if (!existing.IsDeleted) return false;

            // Restore the soft-deleted super admin
            existing.IsDeleted = false;
            existing.IsActive = true;
            existing.Role = UserRole.Admin;
            existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

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

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var users = await unitOfWork.Users.FindAsync(u => u.Email == normalized && u.IsActive, cancellationToken);
        var user = users.FirstOrDefault();
        if (user is null) return null;

        var token = Guid.NewGuid().ToString("N")[..8].ToUpper();
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTimeOffset.UtcNow.AddMinutes(30);
        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var users = await unitOfWork.Users.FindAsync(u => u.Email == normalized && u.IsActive, cancellationToken);
        var user = users.FirstOrDefault();
        if (user is null) return false;
        if (user.ResetToken != token.Trim().ToUpper() || user.ResetTokenExpiry < DateTimeOffset.UtcNow)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null) return false;
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash)) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<string> GenerateTwoFactorCodeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new InvalidOperationException("User not found.");

        if (!string.IsNullOrEmpty(user.EmailVerificationCode)
            && user.EmailCodeExpiry.HasValue
            && user.EmailCodeExpiry.Value > DateTimeOffset.UtcNow)
        {
            return user.EmailVerificationCode;
        }

        var code = Random.Shared.Next(0, 1_000_000).ToString("D6");
        user.EmailVerificationCode = code;
        user.EmailCodeExpiry = DateTimeOffset.UtcNow.AddMinutes(10);
        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return code;
    }

    public async Task<AuthResponseDto?> VerifyTwoFactorCodeAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[2FA-Verify] userId={UserId}, inputCode='{Code}'", userId, code?.Trim());

        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("[2FA-Verify] User NOT FOUND for id={UserId}", userId);
            return null;
        }

        var trimmedCode = code.Trim();
        var storedCode = user.EmailVerificationCode?.Trim();

        logger.LogInformation("[2FA-Verify] storedCode='{StoredCode}', inputCode='{InputCode}', match={Match}",
            storedCode, trimmedCode, storedCode == trimmedCode);
        logger.LogInformation("[2FA-Verify] ExpiresAt={ExpiresAt}, UtcNow={UtcNow}, expired={Expired}",
            user.EmailCodeExpiry, DateTimeOffset.UtcNow,
            !user.EmailCodeExpiry.HasValue || user.EmailCodeExpiry.Value < DateTimeOffset.UtcNow);

        if (string.IsNullOrEmpty(user.EmailVerificationCode)) return null;
        if (storedCode != trimmedCode) return null;
        if (!user.EmailCodeExpiry.HasValue || user.EmailCodeExpiry.Value < DateTimeOffset.UtcNow) return null;

        user.EmailVerificationCode = null;
        user.EmailCodeExpiry = null;
        await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[2FA-Verify] SUCCESS — user {Email} verified", user.Email);
        return MapToResponse(user);
    }

    private static AuthResponseDto MapToResponse(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString());
}
