using Finexa.Domain.Entities;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Infrastructure.Persistence.Seed
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(FinexaDbContext context)
        {
            var defaultCategories = new List<Category>
            {
                // Income
                new Category { Name = "Salary", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Freelance", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Business", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Investments", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Bonus", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Gift", Type = TransactionType.Income, IsDefault = true },
                new Category { Name = "Other Income", Type = TransactionType.Income, IsDefault = true },

                // Expense
                new Category { Name = "Food", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Drinks", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Groceries", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Transport", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Shopping", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Bills", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Entertainment", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Electronics", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Health", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Education", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Travel", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Gym", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Receipt", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Other Expense", Type = TransactionType.Expense, IsDefault = true },

                // Bill Categories
                new Category { Name = "Internet", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Mobile", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Electricity", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Water", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Gas", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Rent", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Subscriptions", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Insurance", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Loan Payment", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Installments", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Education Fees", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Medical Bills", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Maintenance", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Government Fees", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },
                new Category { Name = "Other Bills", Type = TransactionType.Expense, IsDefault = true, IsBillCategory = true },

                // System
                new Category { Name = "Goals", Type = TransactionType.Expense, IsDefault = true },
                new Category { Name = "Balance Adjustment", Type = TransactionType.Income, IsDefault = true }
            };

            var existingCategories = await context.Categories.ToListAsync();

            foreach (var defaultCategory in defaultCategories)
            {
                var existing = existingCategories.FirstOrDefault(c =>
                    c.Name.ToLower() == defaultCategory.Name.ToLower() &&
                    c.Type == defaultCategory.Type &&
                    c.AppUserId == null);

                if (existing == null)
                {
                    await context.Categories.AddAsync(defaultCategory);
                }
                else
                {
                    existing.IsDefault = true;
                    existing.IsActive = true;

                    if (defaultCategory.IsBillCategory)
                        existing.IsBillCategory = true;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}