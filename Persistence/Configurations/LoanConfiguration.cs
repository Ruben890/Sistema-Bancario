using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans");

        builder.HasKey(loan => loan.Id);

        builder.Property(loan => loan.Id).ValueGeneratedNever();
        builder.Property(loan => loan.UserId).IsRequired();
        builder.Property(loan => loan.Amount)
            .HasConversion(amount => amount.Amount, value => Money.FromPersistence(value))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(loan => loan.Term)
            .HasColumnName("TermInMonths")
            .HasConversion(term => term.Months, value => LoanTerm.FromPersistence(value))
            .IsRequired();

        builder.Property(loan => loan.Purpose)
            .HasConversion(purpose => purpose.Value, value => LoanPurpose.FromPersistence(value))
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(loan => loan.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(loan => loan.ReviewedByUserId);
        builder.Property(loan => loan.ReviewedAt);
        builder.Property(loan => loan.RejectionReason)
            .HasConversion(
                reason => reason == null ? null : reason.Value,
                value => value == null ? null : RejectionReason.FromPersistence(value))
            .HasMaxLength(500);
        builder.Property(loan => loan.CreatedAt).IsRequired();
        builder.Property(loan => loan.UpdatedAt);

        builder.HasIndex(loan => loan.UserId);
        builder.HasIndex(loan => loan.Status);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(loan => loan.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(loan => loan.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
