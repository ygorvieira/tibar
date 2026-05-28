using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tibar.Domain.Entities;
using Tibar.Domain.Enums;
using Tibar.Infrastructure.Identity;

namespace Tibar.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        AppDbContext context,
        UserManager<AppUser> userManager,
        ILogger logger,
        string adminEmail = "admin@tibar.com",
        string adminPassword = "Admin@123")
    {
        await context.Database.MigrateAsync();

        var userId = await EnsureAdminUserAsync(context, userManager, adminEmail, adminPassword);
        if (userId.HasValue)
            await EnsureCategoriesAsync(context, userId.Value, logger);
    }

    private static async Task<Guid?> EnsureAdminUserAsync(
        AppDbContext context,
        UserManager<AppUser> userManager,
        string adminEmail,
        string adminPassword)
    {
        var appUser = await userManager.FindByEmailAsync(adminEmail);
        if (appUser == null)
        {
            appUser = new AppUser
            {
                UserName = "admin",
                Email = adminEmail,
                Name = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(appUser, adminPassword);
            if (!result.Succeeded)
                return null;
        }

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (existingUser != null)
            return existingUser.Id;

        var userName = appUser.UserName ?? "admin";
        var user = new User(userName, adminEmail);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    private static async Task EnsureCategoriesAsync(AppDbContext context, Guid userId, ILogger logger)
    {
        if (await context.Categories.AnyAsync(c => c.UserId == userId))
        {
            logger.LogInformation("Categories already exist for user {UserId}", userId);
            return;
        }

        var categories = new List<Category>
        {
            new("Salário", TransactionType.Income, userId),
            new("Freelance", TransactionType.Income, userId),
            new("Investimentos", TransactionType.Income, userId),
            new("Outros", TransactionType.Income, userId),
            new("Alimentação", TransactionType.Expense, userId),
            new("Transporte", TransactionType.Expense, userId),
            new("Moradia", TransactionType.Expense, userId),
            new("Lazer", TransactionType.Expense, userId),
            new("Saúde", TransactionType.Expense, userId),
            new("Educação", TransactionType.Expense, userId),
            new("Assinaturas", TransactionType.Expense, userId),
            new("Compras", TransactionType.Expense, userId),
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} categories for user {UserId}", categories.Count, userId);
    }
}
