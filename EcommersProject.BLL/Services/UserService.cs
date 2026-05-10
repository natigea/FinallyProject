using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class UserService
    : GenericService<User, UserGetDto, UserCreateDto, UserUpdateDto>, IUserService
{
    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Users) { }

    public async Task<UserGetDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var users = await Repository.FindAsync(
            u => u.Email == email.Trim().ToLowerInvariant(), cancellationToken);
        var user = users.FirstOrDefault();
        return user is null ? null : Mapper.Map<UserGetDto>(user);
    }

    public async Task<UserGetDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await Repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);
        user.IsActive = true;
        await Repository.UpdateAsync(user, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Mapper.Map<UserGetDto>(user);
    }

    public async Task<UserGetDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await Repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);
        user.IsActive = false;
        await Repository.UpdateAsync(user, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Mapper.Map<UserGetDto>(user);
    }
}
