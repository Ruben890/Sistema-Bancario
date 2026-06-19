using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class User : EntityBase
{
    private User()
    {
        Name = null!;
        Email = null!;
        PasswordHash = null!;
    }

    private User(Guid id, string name, string email, string passwordHash, UserRole role)
    {
        Id = id == Guid.Empty ? Guid.CreateVersion7() : id;
        CreatedAt = DateTime.UtcNow;
        Name = UserName.Create(name);
        Email = Email.Create(email);
        PasswordHash = PasswordHash.Create(passwordHash);
        Role = role;
        IsActive = true;
    }

    public UserName Name { get; private set; }
    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public static User Create(string name, string email, string passwordHash, UserRole role = UserRole.Customer)
        => new(Guid.Empty, name, email, passwordHash, role);

    public void UpdateProfile(string name, string email, UserRole role)
    {
        Name = UserName.Create(name);
        Email = Email.Create(email);
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = PasswordHash.Create(passwordHash);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("USER_ALREADY_INACTIVE", "The user is already inactive.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
