using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IUserService : IGenericService<UserGetDto, UserCreateDto, UserUpdateDto>
{
    Task<UserGetDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserGetDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserGetDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserGetDto> SetRoleAsync(Guid id, string role, CancellationToken cancellationToken = default);
}
