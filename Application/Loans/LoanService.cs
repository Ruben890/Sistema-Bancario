using System.Net;
using Application.Abstractions;
using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Loans;

public sealed class LoanService(
    ILoanRepository loans,
    IUserRepository users,
    ILoanStatusCache loanStatusCache,
    IUnitOfWork unitOfWork) : ILoanService
{
    public async Task<Result<IReadOnlyList<LoanResponse>>> GetAllAsync(CancellationToken cancellationToken)
        => Result<IReadOnlyList<LoanResponse>>.Response(
            (await loans.GetAllAsync(cancellationToken)).Select(ToResponse).ToList(),
            "Loans retrieved successfully.",
            HttpStatusCode.OK);

    public async Task<Result<IReadOnlyList<LoanResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => Result<IReadOnlyList<LoanResponse>>.Response(
            (await loans.GetByUserIdAsync(userId, cancellationToken)).Select(ToResponse).ToList(),
            "Loans retrieved successfully.",
            HttpStatusCode.OK);

    public async Task<Result<LoanResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var loan = await loans.GetByIdAsync(id, cancellationToken);
        return loan is null
            ? Result<LoanResponse>.Failure("LOAN_NOT_FOUND", "Loan was not found.", HttpStatusCode.BadRequest)
            : Result<LoanResponse>.Response(ToResponse(loan), "Loan retrieved successfully.", HttpStatusCode.OK);
    }

    public async Task<Result<object>> GetStatusAsync(Guid id, CancellationToken cancellationToken)
    {
        var status = await loanStatusCache.GetOrCreateAsync(
            id,
            () => loans.GetStatusAsync(id, cancellationToken));

        if (!status.HasValue)
            return Result<object>.Failure("LOAN_NOT_FOUND", "Loan was not found.", HttpStatusCode.BadRequest);

        return Result<object>.Response(
            new { LoanId = id, Status = status.Value },
            "Loan status retrieved successfully.",
            HttpStatusCode.OK);
    }

    public async Task<Result<LoanResponse>> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            return Result<LoanResponse>.Failure(
                "LOAN_USER_NOT_FOUND",
                "Loan user was not found or is inactive.",
                HttpStatusCode.BadRequest);

        var loan = Loan.Request(request.UserId, request.Amount, request.TermInMonths, request.Purpose);
        loans.Add(loan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        loanStatusCache.Set(loan.Id, loan.Status);

        return Result<LoanResponse>.Response(ToResponse(loan), "Loan requested successfully.", HttpStatusCode.Created);
    }

    public async Task<Result<LoanResponse>> UpdateAsync(Guid id, UpdateLoanRequest request, CancellationToken cancellationToken)
    {
        var loan = await loans.GetByIdAsync(id, cancellationToken);
        if (loan is null)
            return Result<LoanResponse>.Failure("LOAN_NOT_FOUND", "Loan was not found.", HttpStatusCode.BadRequest);

        loan.UpdateRequest(request.Amount, request.TermInMonths, request.Purpose);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        loanStatusCache.Set(loan.Id, loan.Status);

        return Result<LoanResponse>.Response(ToResponse(loan), "Loan updated successfully.", HttpStatusCode.OK);
    }

    public async Task<Result<LoanResponse>> ApproveAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken)
    {
        Loan? reviewedLoan = null;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var loan = await loans.GetByIdAsync(id, ct);
            if (loan is null)
                return;

            loan.Approve(adminUserId);
            await unitOfWork.SaveChangesAsync(ct);
            reviewedLoan = loan;
        }, cancellationToken);

        if (reviewedLoan is null)
            return Result<LoanResponse>.Failure("LOAN_NOT_FOUND", "Loan was not found.", HttpStatusCode.BadRequest);

        loanStatusCache.Set(id, LoanStatus.Approved);
        return Result<LoanResponse>.Response(ToResponse(reviewedLoan!), "Loan approved successfully.", HttpStatusCode.OK);
    }

    public async Task<Result<LoanResponse>> RejectAsync(
        Guid id,
        Guid adminUserId,
        RejectLoanRequest request,
        CancellationToken cancellationToken)
    {
        Loan? reviewedLoan = null;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var loan = await loans.GetByIdAsync(id, ct);
            if (loan is null)
                return;

            loan.Reject(adminUserId, request.Reason);
            await unitOfWork.SaveChangesAsync(ct);
            reviewedLoan = loan;
        }, cancellationToken);

        if (reviewedLoan is null)
            return Result<LoanResponse>.Failure("LOAN_NOT_FOUND", "Loan was not found.", HttpStatusCode.BadRequest);

        loanStatusCache.Set(id, LoanStatus.Rejected);
        return Result<LoanResponse>.Response(ToResponse(reviewedLoan!), "Loan rejected successfully.", HttpStatusCode.OK);
    }

    public async Task<Result<object>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var loan = await loans.GetByIdAsync(id, cancellationToken);
        if (loan is null)
            return Result<object>.Failure("LOAN_NOT_FOUND", "Loan was not found.", HttpStatusCode.BadRequest);

        if (loan.Status != LoanStatus.Pending)
            return Result<object>.Failure(
                "LOAN_DELETE_INVALID_STATUS",
                "Only pending loans can be deleted.",
                HttpStatusCode.BadRequest);

        loans.Remove(loan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        loanStatusCache.Remove(id);

        return Result<object>.Response("Loan deleted successfully.", HttpStatusCode.OK);
    }

    private static LoanResponse ToResponse(Loan loan)
        => new(
            loan.Id,
            loan.UserId,
            loan.Amount.Amount,
            loan.Term.Months,
            loan.Purpose.Value,
            loan.Status,
            loan.ReviewedByUserId,
            loan.ReviewedAt,
            loan.RejectionReason?.Value);
}
