using Domain.Enums;

namespace Application.Loans;

public sealed record LoanUserResponse(Guid Id, string Name, string Email, UserRole Role);

public sealed record LoanResponse(
    Guid Id,
    Guid UserId,
    LoanUserResponse? User,
    decimal Amount,
    int TermInMonths,
    string Purpose,
    LoanStatus Status,
    Guid? ReviewedByUserId,
    DateTime? ReviewedAt,
    string? RejectionReason);

public sealed record CreateLoanRequest(Guid UserId, decimal Amount, int TermInMonths, string Purpose);
public sealed record UpdateLoanRequest(decimal Amount, int TermInMonths, string Purpose);
public sealed record RejectLoanRequest(string Reason);
