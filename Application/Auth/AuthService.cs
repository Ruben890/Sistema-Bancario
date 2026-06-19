using System.Net;
using Application.Abstractions;
using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IAuthCookieService authCookieService,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<Result<object>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.EmailExistsAsync(email, null, cancellationToken))
            return Result<object>.Failure(
                "USER_EMAIL_ALREADY_EXISTS",
                "A user with this email already exists.",
                HttpStatusCode.BadRequest);

        var user = User.Create(
            request.Name,
            email,
            passwordHasher.Hash(request.Password),
            UserRole.Customer);

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        authCookieService.WriteAccessToken(jwtTokenService.CreateToken(user));

        return Result<object>.Response("User registered successfully.", HttpStatusCode.OK);
    }

    public async Task<Result<object>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash.Value))
            return Result<object>.Failure(
                "INVALID_CREDENTIALS",
                "Email or password is invalid.",
                HttpStatusCode.BadRequest);

        authCookieService.WriteAccessToken(jwtTokenService.CreateToken(user));

        return Result<object>.Response("Login completed successfully.", HttpStatusCode.OK);
    }

    public async Task<Result<AuthResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<AuthResponse>.Failure(
                "USER_NOT_FOUND",
                "Authenticated user was not found.",
                HttpStatusCode.Unauthorized);
        }

        return Result<AuthResponse>.Response(
            new AuthResponse(user.Id, user.Name.Value, user.Email.Value, user.Role.ToString(), string.Empty),
            "Authenticated user retrieved successfully.",
            HttpStatusCode.OK);
    }
}
