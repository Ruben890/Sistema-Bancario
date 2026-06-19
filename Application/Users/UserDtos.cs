using Domain.Enums;

namespace Application.Users;

public sealed record UserResponse(Guid Id, string Name, string Email, UserRole Role, bool IsActive);
public sealed record CreateUserRequest(string Name, string Email, string Password, UserRole Role);
public sealed record UpdateUserRequest(string Name, string Email, UserRole Role);
