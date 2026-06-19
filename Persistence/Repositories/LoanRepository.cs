using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories;

public sealed class LoanRepository(BankingDbContext context) : ILoanRepository
{
    public async Task<IReadOnlyList<Loan>> GetAllAsync(CancellationToken cancellationToken)
        => await context.Loans
            .OrderByDescending(loan => loan.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Loan>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => await context.Loans
            .Where(loan => loan.UserId == userId)
            .OrderByDescending(loan => loan.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => context.Loans.FirstOrDefaultAsync(loan => loan.Id == id, cancellationToken);

    public Task<LoanStatus?> GetStatusAsync(Guid id, CancellationToken cancellationToken)
        => context.Loans
            .Where(loan => loan.Id == id)
            .Select(loan => (LoanStatus?)loan.Status)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(Loan loan)
        => context.Loans.Add(loan);

    public void Remove(Loan loan)
        => context.Loans.Remove(loan);
}
