using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Tests.Domain;

public sealed class LoanTests
{
    [Fact]
    public void Request_creates_pending_loan()
    {
        var userId = Guid.CreateVersion7();

        var loan = Loan.Request(userId, 100_000, 24, "Vehicle");

        Assert.Equal(userId, loan.UserId);
        Assert.Equal(LoanStatus.Pending, loan.Status);
        Assert.Equal(100_000, loan.Amount.Amount);
    }

    [Fact]
    public void Approve_marks_loan_as_approved()
    {
        var loan = Loan.Request(Guid.CreateVersion7(), 100_000, 24, "Vehicle");
        var adminId = Guid.CreateVersion7();

        loan.Approve(adminId);

        Assert.Equal(LoanStatus.Approved, loan.Status);
        Assert.Equal(adminId, loan.ReviewedByUserId);
        Assert.NotNull(loan.ReviewedAt);
    }

    [Fact]
    public void Reviewed_loan_cannot_be_reviewed_again()
    {
        var loan = Loan.Request(Guid.CreateVersion7(), 100_000, 24, "Vehicle");
        loan.Approve(Guid.CreateVersion7());

        var exception = Assert.Throws<DomainException>(() => loan.Reject(Guid.CreateVersion7(), "Too risky"));

        Assert.Equal("LOAN_ALREADY_REVIEWED", exception.Code);
    }

    [Fact]
    public void Reject_requires_reason()
    {
        var loan = Loan.Request(Guid.CreateVersion7(), 100_000, 24, "Vehicle");

        var exception = Assert.Throws<DomainException>(() => loan.Reject(Guid.CreateVersion7(), ""));

        Assert.Equal("LOAN_REJECTION_REASON_REQUIRED", exception.Code);
    }
}
