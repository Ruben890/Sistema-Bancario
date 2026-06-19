using Domain.Common;

namespace Domain.ValueObjects;

public sealed class UserName : ValueObject
{
    private UserName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static UserName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("USER_NAME_REQUIRED", "User name is required.");

        var normalized = value.Trim();
        if (normalized.Length > 150)
            throw new DomainException("USER_NAME_TOO_LONG", "User name cannot exceed 150 characters.");

        return new UserName(normalized);
    }

    public static UserName FromPersistence(string value)
        => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}
