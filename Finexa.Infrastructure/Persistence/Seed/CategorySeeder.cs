using Finexa.Domain.Entities;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Infrastructure.Persistence.Seed
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(FinexaDbContext context)
        {
            var existingNames = await context.Categories
                .Select(c => c.Name)
                .ToListAsync();

            var categories = new List<Category>
            {
                //  Income
                new Category { Name = "Salary", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Freelance", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Business", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Investments", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Bonus", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Gift", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Other Income", Type = TransactionType.Income, IsDefault = true },

                //  Expense
                new Category { Name = "Food", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Drinks", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Groceries", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Transport", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Shopping", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Bills", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Rent", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Entertainment", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Electronics", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Health", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Education", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Travel", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Subscriptions", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Gym", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Other Expense", Type = TransactionType.Expense, IsDefault = true },

                //  System
                new Category { Name = "Goals", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Balance Adjustment", Type = TransactionType.Income, IsDefault = true }
            };

            var newCategories = categories
                .Where(c => !existingNames.Contains(c.Name))
                .ToList();

            if (newCategories.Any())
            {
                await context.Categories.AddRangeAsync(newCategories);
                await context.SaveChangesAsync();
            }
        }
    }
}