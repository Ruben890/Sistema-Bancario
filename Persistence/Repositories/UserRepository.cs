using Application.Abstractions;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories;

public sealed class UserRepository(BankingDbContext context) : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
        => await context.Users
            .OrderBy(user => user.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var value = Email.Create(email);
        return context.Users.FirstOrDefaultAsync(user => user.Email == value, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, Guid? excludingUserId, CancellationToken cancellationToken)
    {
        var value = Email.Create(email);
        return context.Users.AnyAsync(
            user => user.Email == value && (!excludingUserId.HasValue || user.Id != excludingUserId.Value),
            cancellationToken);
    }

    public void Add(User user)
        => context.Users.Add(user);

    public void Remove(User user)
        => context.Users.Remove(user);
}
