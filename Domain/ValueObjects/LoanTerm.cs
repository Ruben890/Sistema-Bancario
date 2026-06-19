using Domain.Common;

namespace Domain.ValueObjects;

public sealed class LoanTerm : ValueObject
{
    private LoanTerm(int months)
    {
        Months = months;
    }

    public int Months { get; }

    public static LoanTerm Create(int months)
    {
        if (months is < 1 or > 120)
            throw new DomainException("LOAN_TERM_INVALID", "Loan term must be between 1 and 120 months.");

        return new LoanTerm(months);
    }

    public static LoanTerm FromPersistence(int months)
        => new(months);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Months;
    }
}
