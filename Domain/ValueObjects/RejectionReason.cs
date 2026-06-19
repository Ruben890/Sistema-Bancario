using Domain.Common;

namespace Domain.ValueObjects;

public sealed class RejectionReason : ValueObject
{
    private RejectionReason(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RejectionReason Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("LOAN_REJECTION_REASON_REQUIRED", "A rejection reason is required.");

        var normalized = value.Trim();
        if (normalized.Length > 500)
            throw new DomainException("LOAN_REJECTION_REASON_TOO_LONG", "Rejection reason cannot exceed 500 characters.");

        return new RejectionReason(normalized);
    }

    public static RejectionReason FromPersistence(string value)
        => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}
