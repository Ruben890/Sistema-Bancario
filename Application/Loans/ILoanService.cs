using Application.Common;
using Domain.Enums;

namespace Application.Loans;

public interface ILoanService
{
    Task<Result<IReadOnlyList<LoanResponse>>> GetVisibleAsync(Guid currentUserId, string? currentRole, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> GetByIdAsync(Guid id, Guid currentUserId, string? currentRole, CancellationToken cancellationToken);
    Task<Result<object>> GetStatusAsync(Guid id, Guid currentUserId, string? currentRole, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> CreateAsync(CreateLoanRequest request, Guid currentUserId, string? currentRole, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> UpdateAsync(Guid id, UpdateLoanRequest request, Guid currentUserId, string? currentRole, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> ApproveAsync(Guid id, Guid adminUserId, string? currentRole, CancellationToken cancellationToken);
    Task<Result<LoanResponse>> RejectAsync(Guid id, Guid adminUserId, string? currentRole, RejectLoanRequest request, CancellationToken cancellationToken);
    Task<Result<object>> DeleteAsync(Guid id, Guid currentUserId, string? currentRole, CancellationToken cancellationToken);
}
