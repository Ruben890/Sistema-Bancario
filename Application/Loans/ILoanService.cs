using Application.Common;
using Domain.Enums;

namespace Application.Loans;

public interface ILoanService
{
    Task<Result<IReadOnlyList<LoanResponse>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<LoanResponse>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<object>> GetStatusAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> UpdateAsync(Guid id, UpdateLoanRequest request, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> ApproveAsync(Guid id, Guid adminUserId, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> RejectAsync(Guid id, Guid adminUserId, RejectLoanRequest request, CancellationToken cancellationToken);
    Task<Result<object>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
