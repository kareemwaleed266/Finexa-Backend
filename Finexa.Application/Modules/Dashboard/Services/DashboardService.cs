using Finexa.Application.Common.Helpers;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Dashboard.DTOs;
using Finexa.Application.Modules.Dashboard.Interfaces;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummaryDto> GetDashboardAsync(DashboardFilterDto filter)
    {
        var userId = _currentUser.UserId;

        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException();

        var (from, to) = DateRangeHelper.GetRange(
            filter.Period,
            filter.From,
            filter.To
        );

        var transactions = await GetUserTransactions(userId, from, to);

        //var goals = await GetUserGoals(userId);
        var balance = await GetUserBalance(userId);

        var categories = await GetCategoriesMap(transactions);

        var income = transactions
            .Where(x => x.Type == TransactionType.Income)
            .Sum(x => x.Amount);

        var expense = transactions
            .Where(x => x.Type == TransactionType.Expense)
            .Sum(x => x.Amount);

        var savings = income - expense;

        var (prevFrom, prevTo) = GetPreviousDateRange(from, to);

        var prevTransactions = await GetUserTransactions(userId, prevFrom, prevTo);

        var prevCategories = await GetCategoriesMap(prevTransactions);

        var prevBreakdown = prevTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Amount)
            );

        var prevIncome = prevTransactions
            .Where(x => x.Type == TransactionType.Income)
            .Sum(x => x.Amount);

        var prevExpense = prevTransactions
            .Where(x => x.Type == TransactionType.Expense)
            .Sum(x => x.Amount);

        var prevSavings = prevIncome - prevExpense;

        var incomeChange = FormatChange(income, prevIncome);
        var expenseChange = FormatChange(expense, prevExpense);
        var savingsChange = FormatChange(savings, prevSavings);

        var totalBalance = balance?.TotalBalance ?? 0;

        //var recentTransactions = MapRecentTransactions(transactions, categories);
        //var goalsDto = MapGoals(goals);

        var expenseBreakdown = GetExpenseBreakdown(transactions, categories, prevBreakdown);

        var moneyFlow = GetMoneyFlow(transactions, filter.Period);

        return new DashboardSummaryDto
        {
            TotalBalance = totalBalance,
            TotalIncome = income,
            TotalExpense = expense,
            TotalSavings = savings,

            IncomeChangePercentage = incomeChange,
            ExpenseChangePercentage = expenseChange,
            SavingsChangePercentage = savingsChange,


            From = DateTimeHelper.EnsureUtcKind(from),
            To = DateTimeHelper.EnsureUtcKind(to),


            ExpenseBreakdown = expenseBreakdown,
            MoneyFlow = moneyFlow
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

    private (DateTime from, DateTime to) GetPreviousDateRange(DateTime currentFrom, DateTime currentTo)
    {
        var duration = currentTo - currentFrom;
        var prevTo = currentFrom.AddTicks(-1); 
        var prevFrom = prevTo - duration;

        return (prevFrom, prevTo);
    }

    private decimal CalculatePercentageChange(decimal current, decimal previous)
    {
        if (previous == 0)
            return current > 0 ? 100 : 0;

        var change = ((current - previous) / previous) * 100;

        change = Math.Min(change, 100);
        change = Math.Max(change, -100);

        return Math.Round(change, 1);
    }

    private ChangeDto FormatChange(decimal current, decimal previous)
    {
        var change = CalculatePercentageChange(current, previous);

        string label;
        string trend;

        if (change > 0)
        {
            trend = "up";

            label = change switch
            {
                >= 80 => $"+{change}% (High increase)",
                >= 30 => $"+{change}% (Moderate increase)",
                _ => $"+{change}%"
            };
        }
        else if (change < 0)
        {
            trend = "down";

            label = change switch
            {
                <= -80 => $"{change}% (High decrease)",
                <= -30 => $"{change}% (Moderate decrease)",
                _ => $"{change}%"
            };
        }
        else
        {
            trend = "neutral";
            label = "0%";
        }

        return new ChangeDto
        {
            Value = change,
            Label = label,
            Trend = trend
        };
    }
    private List<CategoryBreakdownDto> GetExpenseBreakdown(
    List<Transaction> transactions,
    Dictionary<Guid, string> categories,
    Dictionary<Guid, decimal> prevBreakdown)
    {
        return transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g =>
            {
                var currentAmount = g.Sum(x => x.Amount);

                prevBreakdown.TryGetValue(g.Key, out var prevAmount);


                var change = FormatChange(currentAmount, prevAmount);

                return new CategoryBreakdownDto
                {
                    CategoryName = categories.TryGetValue(g.Key, out var name)
                        ? name
                        : "Unknown",

                    Amount = currentAmount,
                    Change = change
                };
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
    }


    private List<MoneyFlowDto> GetMoneyFlow(List<Transaction> transactions, PeriodType period)
    {
        var egyptTimeZone = DateTimeHelper.GetEgyptTimeZone();

        if (period == PeriodType.Year)
        {
            return transactions
                .GroupBy(t =>
                {
                    var localDate = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTimeHelper.EnsureUtcKind(t.OccurredAt),
                        egyptTimeZone);

                    return new DateTime(localDate.Year, localDate.Month, 1);
                })
                .OrderBy(g => g.Key)
                .Select(g => new MoneyFlowDto
                {
                    Label = $"{g.Key.Month}/{g.Key.Year}",
                    Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount),
                    Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
                })
                .ToList();
        }

        return transactions
            .GroupBy(t =>
            {
                var localDate = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTimeHelper.EnsureUtcKind(t.OccurredAt),
                    egyptTimeZone);

                return localDate.Date;
            })
            .OrderBy(g => g.Key)
            .Select(g => new MoneyFlowDto
            {
                Label = g.Key.ToString("dd MMM"),
                Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount),
                Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
            })
            .ToList();
    }
    private async Task<List<Transaction>> GetUserTransactions(Guid userId, DateTime from, DateTime to)
    {
        var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

        return await transactionRepo.Query()
            .Where(x =>
                x.AppUserId == userId &&
                x.OccurredAt >= from &&
                x.OccurredAt <= to)
            .ToListAsync();
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

        if (!categoryIds.Any())
            return new Dictionary<Guid, string>();

        var categoryRepo = _unitOfWork.Repository<Category, Guid>();

        var categories = await categoryRepo.Query()
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        return categories.ToDictionary(c => c.Id, c => c.Name);
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