using Domain.Common;

namespace Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public const decimal MaxLoanAmount = 5_000_000m;

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public decimal Amount { get; }

    public static Money Create(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("LOAN_AMOUNT_INVALID", "Loan amount must be greater than zero.");

        if (amount > MaxLoanAmount)
            throw new DomainException("LOAN_AMOUNT_TOO_HIGH", "Loan amount exceeds the allowed limit.");

        return new Money(amount);
    }

    public static Money FromPersistence(decimal amount)
        => new(amount);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString()
        => Amount.ToString("0.00");
}
