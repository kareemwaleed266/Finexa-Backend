using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Dashboard.DTOs;
using Finexa.Application.Modules.Dashboard.Interfaces;
using Finexa.Domain.Enums;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummaryDto> GetDashboardAsync()
    {
        var userId = _currentUser.UserId;

        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException();

        var transactions = await GetUserTransactions(userId);
        var goals = await GetUserGoals(userId);
        var balance = await GetUserBalance(userId);

        var categories = await GetCategoriesMap(transactions);

        var (income, expense, totalBalance) = CalculateTotals(transactions, balance);

        var recentTransactions = MapRecentTransactions(transactions, categories);

        var goalsDto = MapGoals(goals);

        return new DashboardSummaryDto
        {
            TotalBalance = totalBalance,
            TotalIncome = income,
            TotalExpense = expense,
            RecentTransactions = recentTransactions,
            Goals = goalsDto
        };
    }
    public async Task RebuildBalanceAsync()
    {
        var userId = _currentUser.UserId;

        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("User not authenticated");

        var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
        var balanceRepo = _unitOfWork.Repository<UserBalance, Guid>();

        var transactions = await transactionRepo.WhereAsync(x => x.AppUserId == userId);

        var income = transactions
            .Where(x => x.Type == TransactionType.Income)
            .Sum(x => x.Amount);

        var expense = transactions
            .Where(x => x.Type == TransactionType.Expense)
            .Sum(x => x.Amount);

        var balance = await balanceRepo.FirstOrDefaultAsync(x => x.AppUserId == userId);

        if (balance == null)
        {
            balance = new UserBalance
            {
                Id = Guid.NewGuid(),
                AppUserId = userId,
                TotalIncome = income,
                TotalExpense = expense,
                TotalBalance = income - expense
            };

            await balanceRepo.AddAsync(balance);
        }
        else
        {
            balance.TotalIncome = income;
            balance.TotalExpense = expense;
            balance.TotalBalance = income - expense;
        }

        await _unitOfWork.SaveChangesAsync();
    }


    private async Task<List<Transaction>> GetUserTransactions(Guid userId)
    {
        var transactions = _unitOfWork.Repository<Transaction, Guid>();

        return (await transactions.WhereAsync(x => x.AppUserId == userId)).ToList();
    }

    private async Task<List<Goal>> GetUserGoals(Guid userId)
    {
        var goals = _unitOfWork.Repository<Goal, Guid>();

        return (await goals.WhereAsync(x => x.AppUserId == userId)).ToList();
    }

    private async Task<UserBalance?> GetUserBalance(Guid userId)
    {
        var userBalance = _unitOfWork.Repository<UserBalance, Guid>();

        return await userBalance.FirstOrDefaultAsync(x => x.AppUserId == userId);
    }

    private async Task<Dictionary<Guid, string>> GetCategoriesMap(List<Transaction> transactions)
    {
        var categoryIds = transactions
            .Select(t => t.CategoryId)
            .Distinct()
            .ToList();

        var category = _unitOfWork.Repository<Category, Guid>();

        var categories = await category.WhereAsync(c => categoryIds.Contains(c.Id));

        return categories.ToDictionary(c => c.Id, c => c.Name);
    }



    private (decimal income, decimal expense, decimal balance) CalculateTotals(
     List<Transaction> transactions,
     UserBalance? balance)
    {
        decimal income;
        decimal expense;
        decimal totalBalance;

        if (balance != null && (balance.TotalIncome != 0 || balance.TotalExpense != 0))
        {
            income = balance.TotalIncome;
            expense = balance.TotalExpense;
            totalBalance = balance.TotalBalance;
        }
        else
        {
            income = transactions
                .Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);

            expense = transactions
                .Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);

            totalBalance = income - expense;
        }

        return (income, expense, totalBalance);
    }


    private List<RecentTransactionDto> MapRecentTransactions(
        List<Transaction> transactions,
        Dictionary<Guid, string> categories)
    {
        return transactions
            .OrderByDescending(x => x.OccurredAt)
            .Take(5)
            .Select(t => new RecentTransactionDto
            {
                TransactionId = t.Id,
                Amount = t.Amount,
                Type = t.Type,
                Notes = t.Notes,
                OccurredAt = t.OccurredAt,
                CategoryName = categories.TryGetValue(t.CategoryId, out var name)
                    ? name
                    : "Unknown"
            })
            .ToList();
    }

    private List<GoalProgressDto> MapGoals(List<Goal> goals)
    {
        return goals
            .OrderByDescending(g => g.CreatedAt)
            .Take(5)
            .Select(g => new GoalProgressDto
            {
                GoalId = g.Id,
                Title = g.Title,
                TargetAmount = g.TargetAmount,
                CurrentAmount = g.CurrentAmount,
                RemainingAmount = g.TargetAmount - g.CurrentAmount,
                ProgressPercentage = g.TargetAmount == 0
                    ? 0
                    : (double)(g.CurrentAmount / g.TargetAmount) * 100,
                Status = g.Status
            })
            .ToList();
    }
}