using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

public interface ILoanRepository
{
    Task<IReadOnlyList<Loan>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Loan>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LoanStatus?> GetStatusAsync(Guid id, CancellationToken cancellationToken);
    void Add(Loan loan);
    void Remove(Loan loan);
}
