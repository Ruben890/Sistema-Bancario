using Domain.Common;

namespace Domain.ValueObjects;

public sealed class PasswordHash : ValueObject
{
    private PasswordHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PasswordHash Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("USER_PASSWORD_REQUIRED", "User password is required.");

        if (value.Length > 500)
            throw new DomainException("USER_PASSWORD_HASH_TOO_LONG", "User password hash cannot exceed 500 characters.");

        return new PasswordHash(value);
    }

    public static PasswordHash FromPersistence(string value)
        => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}
