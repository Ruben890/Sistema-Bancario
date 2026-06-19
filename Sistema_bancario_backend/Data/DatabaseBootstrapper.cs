using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Sistema_bancario_backend.Data;

internal static class DatabaseBootstrapper
{
    public static async Task EnsureDatabaseCreatedAndSeedAdminAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await context.Database.MigrateAsync();

        var users = await SeedUsersAsync(context, passwordHasher, configuration);
        await SeedLoansAsync(context, users);
    }

    private static async Task<IReadOnlyDictionary<string, User>> SeedUsersAsync(
        BankingDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        var adminEmail = NormalizeEmail(configuration["InitialAdmin:Email"] ?? "admin@bank.local");
        var adminPassword = configuration["InitialAdmin:Password"] ?? "admin123";
        var adminName = configuration["InitialAdmin:Name"] ?? "Administrator";

        var seedUsers = new[]
        {
            new SeedUser(adminName, adminEmail, adminPassword, UserRole.Admin),
            new SeedUser("Admin Demo", "admin@test.com", "123456", UserRole.Admin),
            new SeedUser("Usuario Demo", "usuario@test.com", "123456", UserRole.Customer),
            new SeedUser("Maria Rodriguez", "maria@test.com", "123456", UserRole.Customer),
            new SeedUser("Carlos Perez", "carlos@test.com", "123456", UserRole.Customer)
        };

        var emails = seedUsers.Select(user => user.Email).Distinct().ToArray();
        var existingUsers = (await context.Users.ToListAsync())
            .Where(user => emails.Contains(user.Email.Value))
            .ToDictionary(user => user.Email.Value);

        foreach (var seedUser in seedUsers)
        {
            if (existingUsers.ContainsKey(seedUser.Email))
                continue;

            var user = User.Create(
                seedUser.Name,
                seedUser.Email,
                passwordHasher.Hash(seedUser.Password),
                seedUser.Role);

            context.Users.Add(user);
            existingUsers[seedUser.Email] = user;
        }

        await context.SaveChangesAsync();

        return existingUsers;
    }

    private static async Task SeedLoansAsync(
        BankingDbContext context,
        IReadOnlyDictionary<string, User> users)
    {
        var demoUserEmails = new[]
        {
            "usuario@test.com",
            "maria@test.com",
            "carlos@test.com"
        };

        var demoUserIds = users
            .Where(pair => demoUserEmails.Contains(pair.Key))
            .Select(pair => pair.Value.Id)
            .ToArray();

        var alreadySeeded = await context.Loans.AnyAsync(loan => demoUserIds.Contains(loan.UserId));
        if (alreadySeeded)
            return;

        var admin = users.TryGetValue("admin@test.com", out var demoAdmin)
            ? demoAdmin
            : users.Values.First(user => user.Role == UserRole.Admin);

        var usuario = users["usuario@test.com"];
        var maria = users["maria@test.com"];
        var carlos = users["carlos@test.com"];

        var pendingLoan = Loan.Request(
            usuario.Id,
            125_000m,
            24,
            "Capital de trabajo para inventario");

        var approvedLoan = Loan.Request(
            maria.Id,
            340_000m,
            48,
            "Compra de vehiculo familiar");
        approvedLoan.Approve(admin.Id);

        var rejectedLoan = Loan.Request(
            carlos.Id,
            85_000m,
            18,
            "Consolidacion de deudas");
        rejectedLoan.Reject(admin.Id, "Historial crediticio insuficiente.");

        var secondPendingLoan = Loan.Request(
            maria.Id,
            55_000m,
            12,
            "Mejoras del hogar");

        context.Loans.AddRange(
            pendingLoan,
            approvedLoan,
            rejectedLoan,
            secondPendingLoan);

        await context.SaveChangesAsync();
    }

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    private sealed record SeedUser(
        string Name,
        string Email,
        string Password,
        UserRole Role);
}
