using Domain.Enums;

namespace Application.Abstractions;

public interface ILoanStatusCache
{
    Task<LoanStatus?> GetOrCreateAsync(Guid loanId, Func<Task<LoanStatus?>> factory);
    void Set(Guid loanId, LoanStatus status);
    void Remove(Guid loanId);
}
