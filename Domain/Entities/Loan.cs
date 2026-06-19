using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Loan : EntityBase
{
    private Loan()
    {
        Amount = null!;
        Term = null!;
        Purpose = null!;
    }

    private Loan(Guid userId, decimal amount, int termInMonths, string purpose)
    {
        Id = Guid.CreateVersion7();
        CreatedAt = DateTime.UtcNow;
        UserId = userId == Guid.Empty
            ? throw new DomainException("LOAN_USER_REQUIRED", "A loan must be linked to a user.")
            : userId;
        Amount = Money.Create(amount);
        Term = LoanTerm.Create(termInMonths);
        Purpose = LoanPurpose.Create(purpose);
        Status = LoanStatus.Pending;
    }

    public Guid UserId { get; private set; }
    public Money Amount { get; private set; }
    public LoanTerm Term { get; private set; }
    public LoanPurpose Purpose { get; private set; }
    public LoanStatus Status { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public RejectionReason? RejectionReason { get; private set; }

    public static Loan Request(Guid userId, decimal amount, int termInMonths, string purpose)
        => new(userId, amount, termInMonths, purpose);

    public void UpdateRequest(decimal amount, int termInMonths, string purpose)
    {
        EnsurePending();
        Amount = Money.Create(amount);
        Term = LoanTerm.Create(termInMonths);
        Purpose = LoanPurpose.Create(purpose);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid adminUserId)
    {
        EnsurePending();
        EnsureReviewer(adminUserId);

        Status = LoanStatus.Approved;
        ReviewedByUserId = adminUserId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(Guid adminUserId, string reason)
    {
        EnsurePending();
        EnsureReviewer(adminUserId);

        Status = LoanStatus.Rejected;
        ReviewedByUserId = adminUserId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = RejectionReason.Create(reason);
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsurePending()
    {
        if (Status != LoanStatus.Pending)
            throw new DomainException("LOAN_ALREADY_REVIEWED", "Only pending loans can be changed.");
    }

    private static void EnsureReviewer(Guid adminUserId)
    {
        if (adminUserId == Guid.Empty)
            throw new DomainException("LOAN_REVIEWER_REQUIRED", "A loan review requires an administrator.");
    }
}
