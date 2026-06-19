using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Contexts;

public sealed class BankingDbContext(DbContextOptions<BankingDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("banking");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankingDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyPersistenceConventions();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyPersistenceConventions();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyPersistenceConventions()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.Id == Guid.Empty)
                    entry.Property(nameof(EntityBase.Id)).CurrentValue = Guid.CreateVersion7();

                entry.Property(nameof(EntityBase.CreatedAt)).CurrentValue = now;
            }

            if (entry.State == EntityState.Modified)
                entry.Property(nameof(EntityBase.UpdatedAt)).CurrentValue = now;
        }
    }
}
