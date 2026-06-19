using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.Name)
            .HasConversion(name => name.Value, value => UserName.FromPersistence(value))
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasConversion(email => email.Value, value => Email.FromPersistence(value))
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasConversion(passwordHash => passwordHash.Value, value => PasswordHash.FromPersistence(value))
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.CreatedAt).IsRequired();
        builder.Property(user => user.UpdatedAt);

        builder.HasIndex(user => user.Email).IsUnique();
    }
}
