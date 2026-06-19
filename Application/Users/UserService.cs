using System.Net;
using Application.Abstractions;
using Application.Common;
using Domain.Entities;

namespace Application.Users;

public sealed class UserService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IUserService
{
    public async Task<Result<IReadOnlyList<UserResponse>>> GetAllAsync(CancellationToken cancellationToken)
        => Result<IReadOnlyList<UserResponse>>.Response(
            (await users.GetAllAsync(cancellationToken)).Select(ToResponse).ToList(),
            "Users retrieved successfully.",
            HttpStatusCode.OK);

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        return user is null
            ? Result<UserResponse>.Failure("USER_NOT_FOUND", "User was not found.", HttpStatusCode.BadRequest)
            : Result<UserResponse>.Response(ToResponse(user), "User retrieved successfully.", HttpStatusCode.OK);
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.EmailExistsAsync(email, null, cancellationToken))
            return Result<UserResponse>.Failure(
                "USER_EMAIL_ALREADY_EXISTS",
                "A user with this email already exists.",
                HttpStatusCode.BadRequest);

        var user = User.Create(
            request.Name,
            email,
            passwordHasher.Hash(request.Password),
            request.Role);

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserResponse>.Response(ToResponse(user), "User created successfully.", HttpStatusCode.Created);
    }

    public async Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result<UserResponse>.Failure("USER_NOT_FOUND", "User was not found.", HttpStatusCode.BadRequest);

        var email = request.Email.Trim().ToLowerInvariant();

        if (await users.EmailExistsAsync(email, id, cancellationToken))
            return Result<UserResponse>.Failure(
                "USER_EMAIL_ALREADY_EXISTS",
                "A user with this email already exists.",
                HttpStatusCode.BadRequest);

        user.UpdateProfile(request.Name, email, request.Role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserResponse>.Response(ToResponse(user), "User updated successfully.", HttpStatusCode.OK);
    }

    public async Task<Result<object>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result<object>.Failure("USER_NOT_FOUND", "User was not found.", HttpStatusCode.BadRequest);

        user.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<object>.Response("User deactivated successfully.", HttpStatusCode.OK);
    }

    private static UserResponse ToResponse(User user)
        => new(user.Id, user.Name.Value, user.Email.Value, user.Role, user.IsActive);
}
