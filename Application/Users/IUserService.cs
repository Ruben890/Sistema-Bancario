using Application.Common;

namespace Application.Users;

public interface IUserService
{
    Task<Result<IReadOnlyList<UserResponse>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken);
    Task<Result<object>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
