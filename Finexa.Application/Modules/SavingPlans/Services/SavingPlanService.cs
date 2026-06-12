using System.Text.Json;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.SavingPlans.DTOs;
using Finexa.Application.Modules.SavingPlans.Interfaces;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.SavingPlans.Services
{
    public class SavingPlanService : ISavingPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ISavingPlanAiService _savingPlanAiService;

        public SavingPlanService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ISavingPlanAiService savingPlanAiService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _savingPlanAiService = savingPlanAiService;
        }

        public async Task<SavingPlanResponseDto> GeneratePreviewAsync(GenerateSavingPlanRequestDto request)
        {
            var userId = GetCurrentUserId();

            ValidateGenerateRequest(request);

            var aiRequest = await BuildAiRequestAsync(userId, request);

            var aiResponse = await _savingPlanAiService.GenerateAsync(aiRequest);

            if (aiResponse == null)
                throw new InvalidOperationException("AI saving plan response is empty");

            NormalizePreviewResponse(aiResponse, request);

            var oldDrafts = await _unitOfWork.Repository<SavingPlan, Guid>()
                .Query(withTracking: true)
                .Where(x => x.AppUserId == userId &&
                            x.Status == SavingPlanStatus.Draft)
                .ToListAsync();

            foreach (var oldDraft in oldDrafts)
            {
                oldDraft.Deactivate();
                oldDraft.LastModifiedBy = GetAuditUser();
            }

            var draftPlan = CreatePlanFromPreview(
                userId,
                request,
                aiResponse);

            await _unitOfWork.Repository<SavingPlan, Guid>()
                .AddAsync(draftPlan);

            await _unitOfWork.SaveChangesAsync();

            var result = MapPlanToDto(draftPlan);

            result.Insights = aiResponse.Insights ?? new List<string>();
            result.Warnings = aiResponse.Warnings ?? new List<string>();

            return result;
        }
        public async Task<SavingPlanResponseDto> ApplyAsync(Guid draftPlanId)
        {
            var userId = GetCurrentUserId();

            var draftPlan = await _unitOfWork.Repository<SavingPlan, Guid>()
                .Query(withTracking: true)
                .Where(x =>
                    x.Id == draftPlanId &&
                    x.AppUserId == userId &&
                    x.Status == SavingPlanStatus.Draft)
                .Include(x => x.Items)
                .FirstOrDefaultAsync();

            if (draftPlan == null)
                throw new KeyNotFoundException("Draft saving plan not found");

            var activePlans = await _unitOfWork.Repository<SavingPlan, Guid>()
                .Query(withTracking: true)
                .Where(x =>
                    x.AppUserId == userId &&
                    x.Status == SavingPlanStatus.Active)
                .ToListAsync();

            foreach (var activePlan in activePlans)
            {
                activePlan.Deactivate();
                activePlan.LastModifiedBy = GetAuditUser();
            }

            if (activePlans.Any())
                await _unitOfWork.SaveChangesAsync();

            draftPlan.Activate();
            draftPlan.LastModifiedBy = GetAuditUser();

            await _unitOfWork.SaveChangesAsync();

            return MapPlanToDto(draftPlan);
        }

        public async Task<SavingPlanResponseDto?> GetActivePlanAsync()
        {
            var userId = GetCurrentUserId();

            await SyncActiveMonthlyProgressAsync();

            var plan = await _unitOfWork.Repository<SavingPlan, Guid>()
                .Query()
                .Where(x => x.AppUserId == userId &&
                            x.Status == SavingPlanStatus.Active)
                .Include(x => x.Items)
                .FirstOrDefaultAsync();

            return plan == null ? null : MapPlanToDto(plan);
        }

        public async Task<SavingPlanCurrentProgressDto> GetActiveProgressAsync()
        {
            var userId = GetCurrentUserId();

            var plan = await GetActivePlanEntityAsync(userId, withTracking: false);

            var now = DateTime.UtcNow;

            return await CalculateProgressForMonthAsync(
                plan,
                now.Year,
                now.Month);
        }

        public async Task<List<SavingPlanMonthlyProgressDto>> GetActiveMonthlyProgressAsync()
        {
            await SyncActiveMonthlyProgressAsync();

            var userId = GetCurrentUserId();

            var activePlan = await _unitOfWork.Repository<SavingPlan, Guid>()
                .Query(withTracking: false)
                .Where(x =>
                    x.AppUserId == userId &&
                    x.Status == SavingPlanStatus.Active)
                .Include(x => x.MonthlyProgress)
                .FirstOrDefaultAsync();

            if (activePlan == null)
                throw new KeyNotFoundException("No active saving plan found");

            return activePlan.MonthlyProgress
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Select(x => new SavingPlanMonthlyProgressDto
                {
                    Year = x.Year,
                    Month = x.Month,
                    RecommendedMonthlySaving = x.RecommendedMonthlySaving,
                    ActualIncome = x.ActualIncome,
                    ActualExpenses = x.ActualExpenses,
                    ActualSaving = x.ActualSaving,
                    Difference = x.Difference,
                    ProgressPercentage = x.ProgressPercentage,
                    Status = x.Status,
                    Summary = x.Summary,
                    CalculatedAt = x.CalculatedAt
                })
                .ToList();
        }
        public async Task SyncActiveMonthlyProgressAsync()
        {
            var userId = GetCurrentUserId();

            var plan = await _unitOfWork.Repository<SavingPlan, Guid>()
                .Query(withTracking: true)
                .Where(x => x.AppUserId == userId &&
                            x.Status == SavingPlanStatus.Active)
                .Include(x => x.Items)
                .FirstOrDefaultAsync();

            if (plan == null)
                return;

            var currentMonthStart = GetMonthStart(DateTime.UtcNow);
            var cursor = GetMonthStart(plan.StartDate);

            while (cursor < currentMonthStart)
            {
                var alreadyExists = await _unitOfWork
                    .Repository<SavingPlanMonthlyProgress, Guid>()
                    .ExistsAsync(x =>
                        x.SavingPlanId == plan.Id &&
                        x.Year == cursor.Year &&
                        x.Month == cursor.Month);

                if (!alreadyExists)
                {
                    var progress = await CalculateProgressForMonthAsync(
                        plan,
                        cursor.Year,
                        cursor.Month);

                    var snapshot = new SavingPlanMonthlyProgress
                    {
                        Id = Guid.NewGuid(),
                        SavingPlanId = plan.Id,
                        Year = progress.Year,
                        Month = progress.Month,
                        RecommendedMonthlySaving = progress.RecommendedMonthlySaving,
                        ActualIncome = progress.ActualIncome,
                        ActualExpenses = progress.ActualExpenses,
                        ActualSaving = progress.ActualSaving,
                        Difference = progress.Difference,
                        ProgressPercentage = progress.ProgressPercentage,
                        Status = progress.Status,
                        Summary = progress.Summary,
                        CalculatedAt = DateTime.UtcNow,
                        CreatedBy = GetAuditUser()
                    };

                    await _unitOfWork.Repository<SavingPlanMonthlyProgress, Guid>()
                        .AddAsync(snapshot);
                }

                cursor = cursor.AddMonths(1);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Guid savingPlanId)
        {
            var userId = GetCurrentUserId();

            var plan = await _unitOfWork.Repository<SavingPlan, Guid>()
                .Query(withTracking: true)
                .FirstOrDefaultAsync(x =>
                    x.Id == savingPlanId &&
                    x.AppUserId == userId);

            if (plan == null)
                throw new KeyNotFoundException("Saving plan not found");

            plan.Deactivate();
            plan.LastModifiedBy = GetAuditUser();

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<SavingPlanAiRequestDto> BuildAiRequestAsync(
            Guid userId,
            GenerateSavingPlanRequestDto request)
        {
            var currentMonthStart = GetMonthStart(DateTime.UtcNow);
            var from = currentMonthStart.AddMonths(-request.Months);
            var to = currentMonthStart;

            var transactions = await _unitOfWork.Repository<Transaction, Guid>()
                .Query()
                .Where(x =>
                    x.AppUserId == userId &&
                    x.OccurredAt >= from &&
                    x.OccurredAt < to)
                .Include(x => x.Category)
                .ToListAsync();

            var monthlySummary = BuildMonthlySummary(
                transactions,
                from,
                request.Months);

            var categorySummary = BuildCategorySummary(
                transactions,
                request.Months,
                from);

            return new SavingPlanAiRequestDto
            {
                Months = request.Months,
                PlanType = request.PlanType,
                TargetMonthlySaving = request.TargetMonthlySaving,
                MonthlySummary = monthlySummary,
                CategorySummary = categorySummary
            };
        }

        private static List<MonthlySummaryDto> BuildMonthlySummary(
            List<Transaction> transactions,
            DateTime from,
            int months)
        {
            var result = new List<MonthlySummaryDto>();

            for (var i = 0; i < months; i++)
            {
                var monthStart = from.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);

                var monthTransactions = transactions
                    .Where(x => x.OccurredAt >= monthStart &&
                                x.OccurredAt < monthEnd)
                    .ToList();

                var income = monthTransactions
                    .Where(x => x.Type == TransactionType.Income)
                    .Sum(x => x.Amount);

                var expenses = monthTransactions
                    .Where(x => x.Type == TransactionType.Expense)
                    .Sum(x => x.Amount);

                result.Add(new MonthlySummaryDto
                {
                    Month = monthStart.ToString("yyyy-MM"),
                    Income = income,
                    Expenses = expenses,
                    Saving = income - expenses
                });
            }

            return result;
        }

        private static List<CategorySummaryDto> BuildCategorySummary(List<Transaction> transactions,int months,DateTime from)
        {
            const decimal minimumAverageMonthlyAmount = 50m;

            var expenseTransactions = transactions
                .Where(x =>
                    x.Type == TransactionType.Expense &&
                    x.Category != null &&
                    !ShouldExcludeCategoryFromSavingPlan(x.Category.Name))
                .ToList();

            var totalExpenses = expenseTransactions.Sum(x => x.Amount);

            return expenseTransactions
                .GroupBy(x => new
                {
                    x.CategoryId,
                    CategoryName = x.Category!.Name
                })
                .Select(group =>
                {
                    var total = group.Sum(x => x.Amount);
                    var averageMonthlyAmount = Math.Round(total / months, 2);

                    return new CategorySummaryDto
                    {
                        CategoryId = group.Key.CategoryId,
                        CategoryName = group.Key.CategoryName,
                        CategoryType = GetCategoryType(group.Key.CategoryName),
                        AverageMonthlyAmount = averageMonthlyAmount,
                        TotalAmount = total,
                        PercentageOfExpenses = totalExpenses == 0
                            ? 0
                            : Math.Round((total / totalExpenses) * 100, 2),
                        Trend = CalculateTrend(group.ToList(), from, months)
                    };
                })
                .Where(x => x.AverageMonthlyAmount >= minimumAverageMonthlyAmount)
                .OrderByDescending(x => x.TotalAmount)
                .ToList();
        }
        private async Task<SavingPlanCurrentProgressDto> CalculateProgressForMonthAsync(
            SavingPlan plan,
            int year,
            int month)
        {
            var userId = plan.AppUserId;

            var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1);

            var transactions = await _unitOfWork.Repository<Transaction, Guid>()
                .Query()
                .Where(x =>
                    x.AppUserId == userId &&
                    x.OccurredAt >= from &&
                    x.OccurredAt < to)
                .Include(x => x.Category)
                .ToListAsync();

            var actualIncome = transactions
                .Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);

            var actualExpenses = transactions
                .Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);

            var actualSaving = actualIncome - actualExpenses;

            var difference = actualSaving - plan.RecommendedMonthlySaving;

            var progressPercentage = plan.RecommendedMonthlySaving <= 0
                ? 0
                : Math.Round((actualSaving / plan.RecommendedMonthlySaving) * 100, 2);

            if (progressPercentage < 0)
                progressPercentage = 0;

            var status = GetMonthlyStatus(
                actualIncome,
                actualExpenses,
                progressPercentage);

            var categoryProgress = plan.Items
                .Select(item =>
                {
                    var actualSpent = transactions
                        .Where(x =>
                            x.Type == TransactionType.Expense &&
                            item.CategoryId.HasValue &&
                            x.CategoryId == item.CategoryId.Value)
                        .Sum(x => x.Amount);

                    var categoryDifference = item.RecommendedBudget - actualSpent;

                    return new SavingPlanCategoryProgressDto
                    {
                        CategoryId = item.CategoryId,
                        CategoryName = item.CategoryName,
                        RecommendedBudget = item.RecommendedBudget,
                        ActualSpent = actualSpent,
                        Difference = categoryDifference,
                        Status = actualSpent <= item.RecommendedBudget
                            ? "UnderBudget"
                            : "OverBudget"
                    };
                })
                .ToList();

            return new SavingPlanCurrentProgressDto
            {
                Year = year,
                Month = month,
                RecommendedMonthlySaving = plan.RecommendedMonthlySaving,
                ActualIncome = actualIncome,
                ActualExpenses = actualExpenses,
                ActualSaving = actualSaving,
                Difference = difference,
                ProgressPercentage = progressPercentage,
                Status = status,
                Summary = BuildProgressSummary(difference, status),
                CategoryProgress = categoryProgress
            };
        }

        private async Task<SavingPlan> GetActivePlanEntityAsync(
            Guid userId,
            bool withTracking)
        {
            var plan = await _unitOfWork.Repository<SavingPlan, Guid>()
                .Query(withTracking)
                .Where(x => x.AppUserId == userId &&
                            x.Status == SavingPlanStatus.Active)
                .Include(x => x.Items)
                .FirstOrDefaultAsync();

            if (plan == null)
                throw new KeyNotFoundException("No active saving plan found");

            return plan;
        }

        private static SavingPlanResponseDto MapPlanToDto(SavingPlan plan)
        {
            return new SavingPlanResponseDto
            {
                Id = plan.Id,
                AnalysisPeriodMonths = plan.AnalysisPeriodMonths,
                PlanType = plan.PlanType,
                TargetMonthlySaving = plan.TargetMonthlySaving,
                StartDate = plan.Status == SavingPlanStatus.Draft
                    ? null
                    : plan.StartDate.ToString("yyyy-MM-dd"),

                                EndDate = plan.EndDate.HasValue
                    ? plan.EndDate.Value.ToString("yyyy-MM-dd")
                    : null,
                AverageIncome = plan.AverageIncome,
                AverageExpenses = plan.AverageExpenses,
                CurrentAverageSaving = plan.CurrentAverageSaving,
                ForecastedIncome = plan.ForecastedIncome,
                ForecastedExpenses = plan.ForecastedExpenses,
                ForecastedSaving = plan.ForecastedSaving,
                RecommendedMonthlySaving = plan.RecommendedMonthlySaving,
                ExtraSavingOpportunity = plan.ExtraSavingOpportunity,
                Difficulty = plan.Difficulty,
                PlanStatusLabel = plan.PlanStatusLabel,
                Status = plan.Status,
                SummaryMessage = plan.SummaryMessage,
                AppliedAt = plan.AppliedAt,
                Insights = DeserializeStringList(plan.InsightsJson),
                Warnings = DeserializeStringList(plan.WarningsJson),
                Items = plan.Items.Select(x => new SavingPlanItemDto
                {
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    CategoryType = x.CategoryType,
                    CurrentAverage = x.CurrentAverage,
                    RecommendedBudget = x.RecommendedBudget,
                    ReductionPercentage = x.ReductionPercentage,
                    ExpectedSaving = x.ExpectedSaving,
                    Reason = x.Reason
                }).ToList()
            };
        }
        private static List<string> DeserializeStringList(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
        private static void NormalizePreviewResponse(
            SavingPlanResponseDto response,
            GenerateSavingPlanRequestDto request)
        {
            response.Id = null;
            response.AnalysisPeriodMonths = request.Months;
            response.PlanType = request.PlanType;
            response.TargetMonthlySaving = request.TargetMonthlySaving;
            response.Status = null;
            response.StartDate = null;
            response.EndDate = null;
            response.AppliedAt = null;

            response.Items ??= new List<SavingPlanItemDto>();
            response.Insights ??= new List<string>();
            response.Warnings ??= new List<string>();

            response.RecommendedMonthlySaving = Math.Max(0, response.RecommendedMonthlySaving);
            response.ExtraSavingOpportunity = Math.Max(0, response.ExtraSavingOpportunity);

            foreach (var item in response.Items)
            {
                item.CurrentAverage = Math.Max(0, item.CurrentAverage);
                item.RecommendedBudget = Math.Max(0, item.RecommendedBudget);
                item.ReductionPercentage = Math.Max(0, item.ReductionPercentage);
                item.ExpectedSaving = Math.Max(0, item.ExpectedSaving);
            }
        }

        private static SavingPlanCategoryType GetCategoryType(string categoryName)
        {
            var normalized = categoryName.Trim().ToLowerInvariant();

            var essentialKeywords = new[]
            {
                "rent",
                "bill",
                "bills",
                "electricity",
                "water",
                "gas",
                "internet",
                "health",
                "medicine",
                "medical",
                "education",
                "transport",
                "transportation",
                "groceries",
                "grocery",
                "إيجار",
                "فاتورة",
                "فواتير",
                "كهرباء",
                "مياه",
                "غاز",
                "انترنت",
                "صحة",
                "علاج",
                "تعليم",
                "مواصلات",
                "بقالة"
            };

            return essentialKeywords.Any(keyword => normalized.Contains(keyword))
                ? SavingPlanCategoryType.Essential
                : SavingPlanCategoryType.Flexible;
        }

        private static string CalculateTrend(
            List<Transaction> categoryTransactions,
            DateTime from,
            int months)
        {
            if (categoryTransactions.Count == 0 || months < 2)
                return "Stable";

            var splitMonth = Math.Max(1, months / 2);

            var firstPeriodStart = from;
            var secondPeriodStart = from.AddMonths(splitMonth);
            var end = from.AddMonths(months);

            var firstPeriodTotal = categoryTransactions
                .Where(x => x.OccurredAt >= firstPeriodStart &&
                            x.OccurredAt < secondPeriodStart)
                .Sum(x => x.Amount);

            var secondPeriodTotal = categoryTransactions
                .Where(x => x.OccurredAt >= secondPeriodStart &&
                            x.OccurredAt < end)
                .Sum(x => x.Amount);

            if (firstPeriodTotal == 0 && secondPeriodTotal == 0)
                return "Stable";

            if (firstPeriodTotal == 0 && secondPeriodTotal > 0)
                return "Increasing";

            var changePercentage = ((secondPeriodTotal - firstPeriodTotal) / firstPeriodTotal) * 100;

            if (changePercentage >= 10)
                return "Increasing";

            if (changePercentage <= -10)
                return "Decreasing";

            return "Stable";
        }

        private static SavingPlanMonthlyStatus GetMonthlyStatus(
            decimal actualIncome,
            decimal actualExpenses,
            decimal progressPercentage)
        {
            if (actualIncome == 0 && actualExpenses == 0)
                return SavingPlanMonthlyStatus.NoData;

            if (progressPercentage >= 100)
                return SavingPlanMonthlyStatus.Achieved;

            if (progressPercentage >= 90)
                return SavingPlanMonthlyStatus.OnTrack;

            if (progressPercentage >= 70)
                return SavingPlanMonthlyStatus.SlightlyBehind;

            return SavingPlanMonthlyStatus.Failed;
        }

        private static string BuildProgressSummary(
            decimal difference,
            SavingPlanMonthlyStatus status)
        {
            return status switch
            {
                SavingPlanMonthlyStatus.Achieved =>
                    "You achieved your monthly saving target.",

                SavingPlanMonthlyStatus.OnTrack =>
                    "You are close to achieving your monthly saving target.",

                SavingPlanMonthlyStatus.SlightlyBehind =>
                    $"You are {Math.Abs(difference):0.##} below your monthly saving target.",

                SavingPlanMonthlyStatus.Failed =>
                    $"You missed your monthly saving target by {Math.Abs(difference):0.##}.",

                SavingPlanMonthlyStatus.NoData =>
                    "No financial data was recorded for this month.",

                _ => "Monthly progress calculated."
            };
        }

        private static DateTime GetMonthStart(DateTime date)
        {
            return new DateTime(
                date.Year,
                date.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);
        }

        private static void ValidateGenerateRequest(
            GenerateSavingPlanRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Months != 3 &&
                request.Months != 6 &&
                request.Months != 12)
                throw new ArgumentException("Months must be 3, 6, or 12");

            if (request.TargetMonthlySaving.HasValue &&
                request.TargetMonthlySaving.Value < 0)
                throw new ArgumentException("Target monthly saving cannot be negative");
        }

        private SavingPlan CreatePlanFromPreview(Guid userId,GenerateSavingPlanRequestDto request,SavingPlanResponseDto preview)
        {
            var plan = new SavingPlan
            {
                Id = Guid.NewGuid(),
                AppUserId = userId,
                AnalysisPeriodMonths = request.Months,
                PlanType = request.PlanType,
                TargetMonthlySaving = request.TargetMonthlySaving,
                StartDate = GetMonthStart(DateTime.UtcNow),
                EndDate = null,

                AverageIncome = preview.AverageIncome,
                AverageExpenses = preview.AverageExpenses,
                CurrentAverageSaving = preview.CurrentAverageSaving,
                ForecastedIncome = preview.ForecastedIncome,
                ForecastedExpenses = preview.ForecastedExpenses,
                ForecastedSaving = preview.ForecastedSaving,
                RecommendedMonthlySaving = preview.RecommendedMonthlySaving,
                ExtraSavingOpportunity = preview.ExtraSavingOpportunity,
                Difficulty = preview.Difficulty,
                PlanStatusLabel = preview.PlanStatusLabel,
                SummaryMessage = preview.SummaryMessage,
                InsightsJson = JsonSerializer.Serialize(preview.Insights ?? new List<string>()),
                WarningsJson = JsonSerializer.Serialize(preview.Warnings ?? new List<string>()),
                CreatedBy = GetAuditUser()
            };

            foreach (var item in preview.Items)
            {
                plan.Items.Add(new SavingPlanItem
                {
                    Id = Guid.NewGuid(),
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    CategoryType = item.CategoryType,
                    CurrentAverage = item.CurrentAverage,
                    RecommendedBudget = item.RecommendedBudget,
                    ReductionPercentage = item.ReductionPercentage,
                    ExpectedSaving = item.ExpectedSaving,
                    Reason = item.Reason,
                    CreatedBy = GetAuditUser()
                });
            }

            return plan;
        }

        private static bool ShouldExcludeCategoryFromSavingPlan(string categoryName)
        {
            var normalized = categoryName.Trim().ToLowerInvariant();

            var excludedNames = new[]
            {
                "goal",
                "goals",
                "saving goal",
                "balance adjustment",
                "adjustment",
                "refund",
                "reversal",
                "transfer",
                "تحويل",
                "هدف",
                "أهداف",
                "ايصال",
                "إيصال",
                "استرداد",
                "تسوية"
            };

            return excludedNames.Any(x => normalized.Contains(x));
        }

        private Guid GetCurrentUserId()
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            return userId;
        }

        private string? GetAuditUser()
        {
            return _currentUser.Email ?? _currentUser.UserName;
        }
    }
}