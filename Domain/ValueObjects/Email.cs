using Domain.Common;

namespace Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("USER_EMAIL_REQUIRED", "User email is required.");

        var normalized = value.Trim().ToLowerInvariant();
        if (!normalized.Contains('@', StringComparison.Ordinal) || normalized.Length > 256)
            throw new DomainException("USER_EMAIL_INVALID", "User email is invalid.");

        return new Email(normalized);
    }

    public static Email FromPersistence(string value)
        => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}
