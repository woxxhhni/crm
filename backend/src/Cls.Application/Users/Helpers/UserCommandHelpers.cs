using AutoMapper;
using Cls.Application.Abstractions;
using Cls.Application.Users.Commands;
using Cls.Application.Users.Services;
using Cls.Domain.Entities;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Cls.Application.Users.Helpers;

internal static class UserCommandHelpers
{
    internal static async Task EnsureEmailAvailableAsync(
        IUnitOfWork uow,
        string email,
        int? excludeUserId,
        CancellationToken ct)
    {
        var normalized = email.ToLower();
        var exists = await uow.Users.Query()
            .AnyAsync(x => x.Email.ToLower() == normalized && (!excludeUserId.HasValue || x.Id != excludeUserId.Value), ct);

        if (exists)
            throw new BadRequestException($"User exist with email {email}.");
    }

    internal static async Task<User> CreateAsync(
        IUnitOfWork uow,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        string name,
        string email,
        string passwordPlain,
        string? phone,
        string? address,
        string? description,
        UserRole role,
        bool isActive,
        int? fileId,
        CancellationToken ct)
    {
        UserRoleGuards.RejectAdminCreation(role);
        await EnsureEmailAvailableAsync(uow, email, excludeUserId: null, ct);

        var user = mapper.Map<User>(new CreateUserCommand(name, email, passwordPlain, phone, address, description, role, isActive, fileId));
        user.PasswordHash = passwordHasher.Hash(passwordPlain);
        return await uow.Users.AddAsync(user, ct);
    }

    internal static async Task<User?> UpdateAsync(
        IUnitOfWork uow,
        IMapper mapper,
        int id,
        string name,
        string email,
        string? phone,
        string? address,
        string? description,
        UserRole role,
        bool isActive,
        int? fileId,
        CancellationToken ct)
    {
        UserRoleGuards.RejectAdminCreation(role);
        await EnsureEmailAvailableAsync(uow, email, excludeUserId: id, ct);

        var user = await uow.Users.GetByIdAsync(id, ct);
        if (user is null)
            return null;

        var existingFileId = user.FileId;
        mapper.Map(new UpdateUserCommand(id, name, email, phone, address, description, role, isActive, fileId), user);
        if (fileId is null)
            user.FileId = existingFileId;

        user.UpdatedAt = DateTime.UtcNow;
        await uow.Users.UpdateAsync(user, ct);
        return user;
    }
}
