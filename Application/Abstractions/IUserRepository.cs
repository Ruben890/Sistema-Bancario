using Domain.Entities;

namespace Application.Abstractions;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, Guid? excludingUserId, CancellationToken cancellationToken);
    void Add(User user);
    void Remove(User user);
}
