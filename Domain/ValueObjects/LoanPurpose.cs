using Domain.Common;

namespace Domain.ValueObjects;

public sealed class LoanPurpose : ValueObject
{
    private LoanPurpose(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LoanPurpose Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("LOAN_PURPOSE_REQUIRED", "Loan purpose is required.");

        var normalized = value.Trim();
        if (normalized.Length > 500)
            throw new DomainException("LOAN_PURPOSE_TOO_LONG", "Loan purpose cannot exceed 500 characters.");

        return new LoanPurpose(normalized);
    }

    public static LoanPurpose FromPersistence(string value)
        => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}
