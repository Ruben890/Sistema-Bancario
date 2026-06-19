using Application.Abstractions;
using Application.Loans;
using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Tests.Application;

public sealed class LoanServiceTests
{
    [Fact]
    public async Task GetStatus_uses_cache_after_first_lookup()
    {
        var user = User.Create("Test User", "test@bank.local", "hash");
        var loan = Loan.Request(user.Id, 50_000, 12, "Education");
        var loanRepository = new InMemoryLoanRepository(loan);
        var userRepository = new InMemoryUserRepository(user);
        var cache = new InMemoryLoanStatusCache();
        var service = new LoanService(loanRepository, userRepository, cache, new NoOpUnitOfWork());

        var first = await service.GetStatusAsync(loan.Id, user.Id, UserRole.Customer.ToString(), CancellationToken.None);
        var second = await service.GetStatusAsync(loan.Id, user.Id, UserRole.Customer.ToString(), CancellationToken.None);

        Assert.NotNull(first.Entity);
        Assert.NotNull(second.Entity);
        Assert.Equal(1, loanRepository.StatusLookups);
    }

    private sealed class InMemoryLoanStatusCache : ILoanStatusCache
    {
        private readonly Dictionary<Guid, LoanStatus> _cache = new();

        public async Task<LoanStatus?> GetOrCreateAsync(Guid loanId, Func<Task<LoanStatus?>> factory)
        {
            if (_cache.TryGetValue(loanId, out var status))
                return status;

            var created = await factory();
            if (created.HasValue)
                _cache[loanId] = created.Value;

            return created;
        }

        public void Set(Guid loanId, LoanStatus status)
            => _cache[loanId] = status;

        public void Remove(Guid loanId)
            => _cache.Remove(loanId);
    }

    private sealed class InMemoryLoanRepository(Loan loan) : ILoanRepository
    {
        public int StatusLookups { get; private set; }

        public Task<IReadOnlyList<Loan>> GetAllAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Loan>>([loan]);

        public Task<IReadOnlyList<Loan>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Loan>>(loan.UserId == userId ? [loan] : []);

        public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(loan.Id == id ? loan : null);

        public Task<LoanStatus?> GetStatusAsync(Guid id, CancellationToken cancellationToken)
        {
            StatusLookups++;
            return Task.FromResult(loan.Id == id ? loan.Status : (LoanStatus?)null);
        }

        public void Add(Loan newLoan)
        {
        }

        public void Remove(Loan removedLoan)
        {
        }
    }

    private sealed class InMemoryUserRepository(User user) : IUserRepository
    {
        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<User>>([user]);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(user.Id == id ? user : null);

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
            => Task.FromResult(user.Email.Value == email ? user : null);

        public Task<bool> EmailExistsAsync(string email, Guid? excludingUserId, CancellationToken cancellationToken)
            => Task.FromResult(user.Email.Value == email && user.Id != excludingUserId);

        public void Add(User newUser)
        {
        }

        public void Remove(User removedUser)
        {
        }
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
            => Task.FromResult(0);

        public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
            => action(cancellationToken);
    }
}
