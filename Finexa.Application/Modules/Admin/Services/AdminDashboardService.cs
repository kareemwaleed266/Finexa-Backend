using Finexa.Application.Common.Helpers;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Domain.Entities.Admin;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Entities.Identity;
using Finexa.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Admin.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAdminAuditLogService _auditLogService;

        public AdminDashboardService(
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAdminAuditLogService auditLogService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
        }

        public async Task<AdminDashboardSummaryDto> GetSummaryAsync()
        {
            var (monthStartUtc, monthEndUtc) = GetCurrentMonthRangeUtc();
            var nowUtc = DateTime.UtcNow;
            var todayUtc = DateTimeHelper.ConvertClientLocalToUtc(GetEgyptToday());
            var weekEndUtc = DateTimeHelper.ConvertClientLocalToUtc(GetEgyptToday().AddDays(7));

            var summary = new AdminDashboardSummaryDto
            {
                Users = await GetUserStatsAsync(monthStartUtc, monthEndUtc),
                Financial = await GetFinancialStatsAsync(monthStartUtc, monthEndUtc),
                Bills = await GetBillsStatsAsync(monthStartUtc, monthEndUtc, nowUtc, todayUtc, weekEndUtc),
                AiUsage = await GetAiUsageStatsAsync(monthStartUtc, monthEndUtc),
                JobHealth = await GetJobHealthAsync(),
                RecentAuditLogs = await GetRecentAuditLogsAsync()
            };

            await AddAuditLogAsync();

            return summary;
        }

        private async Task<AdminUserStatsDto> GetUserStatsAsync(
            DateTime monthStartUtc,
            DateTime monthEndUtc)
        {
            var now = DateTimeOffset.UtcNow;

            return new AdminUserStatsDto
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                ActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive),
                DeactivatedUsers = await _userManager.Users.CountAsync(u => !u.IsActive),
                LockedUsers = await _userManager.Users.CountAsync(u =>
                    u.LockoutEnd != null &&
                    u.LockoutEnd > now),
                NewUsersThisMonth = await _userManager.Users.CountAsync(u =>
                    u.CreatedAt >= monthStartUtc &&
                    u.CreatedAt < monthEndUtc)
            };
        }

        private async Task<AdminFinancialStatsDto> GetFinancialStatsAsync(
            DateTime monthStartUtc,
            DateTime monthEndUtc)
        {
            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
            var balanceRepo = _unitOfWork.Repository<UserBalance, Guid>();
            var goalRepo = _unitOfWork.Repository<Goal, Guid>();

            return new AdminFinancialStatsDto
            {
                TotalTransactions = await transactionRepo.Query().CountAsync(),
                TransactionsThisMonth = await transactionRepo.Query().CountAsync(t =>
                    t.OccurredAt >= monthStartUtc &&
                    t.OccurredAt < monthEndUtc),
                TotalIncome = await balanceRepo.Query().SumAsync(b => b.TotalIncome),
                TotalExpense = await balanceRepo.Query().SumAsync(b => b.TotalExpense),
                TotalBalance = await balanceRepo.Query().SumAsync(b => b.TotalBalance),
                TotalGoals = await goalRepo.Query().CountAsync(),
                GoalsCreatedThisMonth = await goalRepo.Query().CountAsync(g =>
                    g.CreatedAt >= monthStartUtc &&
                    g.CreatedAt < monthEndUtc)
            };
        }

        private async Task<AdminBillsStatsDto> GetBillsStatsAsync(
            DateTime monthStartUtc,
            DateTime monthEndUtc,
            DateTime nowUtc,
            DateTime todayUtc,
            DateTime weekEndUtc)
        {
            var billSeriesRepo = _unitOfWork.Repository<BillSeries, Guid>();
            var occurrenceRepo = _unitOfWork.Repository<BillOccurrence, Guid>();

            var currentMonthOccurrences = occurrenceRepo.Query()
                .Where(o =>
                    o.DueDate >= monthStartUtc &&
                    o.DueDate < monthEndUtc &&
                    o.Status != BillOccurrenceStatus.Cancelled &&
                    o.Status != BillOccurrenceStatus.Skipped);

            return new AdminBillsStatsDto
            {
                TotalBillSeries = await billSeriesRepo.Query().CountAsync(),
                ActiveBillSeries = await billSeriesRepo.Query().CountAsync(b => b.IsActive),
                TotalOccurrences = await occurrenceRepo.Query().CountAsync(),
                ScheduledOccurrences = await occurrenceRepo.Query().CountAsync(o =>
                    o.Status == BillOccurrenceStatus.Scheduled),
                PaidOccurrences = await occurrenceRepo.Query().CountAsync(o =>
                    o.Status == BillOccurrenceStatus.Paid),
                OverdueOccurrences = await occurrenceRepo.Query().CountAsync(o =>
                    o.Status == BillOccurrenceStatus.Scheduled &&
                    o.DueDate < nowUtc),
                DueThisWeek = await occurrenceRepo.Query().CountAsync(o =>
                    o.Status == BillOccurrenceStatus.Scheduled &&
                    o.DueDate >= todayUtc &&
                    o.DueDate <= weekEndUtc),
                PaidThisMonth = await occurrenceRepo.Query().CountAsync(o =>
                    o.Status == BillOccurrenceStatus.Paid &&
                    o.DueDate >= monthStartUtc &&
                    o.DueDate < monthEndUtc),
                ExpectedThisMonth = await currentMonthOccurrences.SumAsync(o => o.Amount) ?? 0
            };
        }

        private async Task<AdminAiUsageStatsDto> GetAiUsageStatsAsync(
            DateTime monthStartUtc,
            DateTime monthEndUtc)
        {
            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            var aiSources = new[]
            {
                TransactionSource.Chat,
                TransactionSource.OCR,
                TransactionSource.Speech
            };

            var sourceStats = await transactionRepo.Query()
                .Where(t => aiSources.Contains(t.Source))
                .GroupBy(t => t.Source)
                .Select(g => new AdminTransactionSourceStatsDto
                {
                    Source = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            return new AdminAiUsageStatsDto
            {
                TotalAiTransactions = await transactionRepo.Query().CountAsync(t =>
                    aiSources.Contains(t.Source)),
                AiTransactionsThisMonth = await transactionRepo.Query().CountAsync(t =>
                    aiSources.Contains(t.Source) &&
                    t.OccurredAt >= monthStartUtc &&
                    t.OccurredAt < monthEndUtc),
                SourceStats = sourceStats
            };
        }

        private async Task<AdminJobHealthDto> GetJobHealthAsync()
        {
            var jobRepo = _unitOfWork.Repository<SystemJobLog, Guid>();

            var latestJob = await jobRepo.Query()
                .OrderByDescending(j => j.StartedAt)
                .FirstOrDefaultAsync();

            var last24Hours = DateTime.UtcNow.AddHours(-24);

            var failedJobsLast24Hours = await jobRepo.Query()
                .CountAsync(j =>
                    j.Status == SystemJobStatus.Failed &&
                    j.StartedAt >= last24Hours);

            if (latestJob == null)
            {
                return new AdminJobHealthDto
                {
                    FailedJobsLast24Hours = failedJobsLast24Hours
                };
            }

            return new AdminJobHealthDto
            {
                LatestJobId = latestJob.Id,
                LatestJobName = latestJob.JobName,
                LatestJobStatus = latestJob.Status,
                LatestJobStartedAt = DateTimeHelper.EnsureUtcKind(latestJob.StartedAt),
                LatestJobFinishedAt = DateTimeHelper.EnsureUtcKind(latestJob.FinishedAt),
                FailedJobsLast24Hours = failedJobsLast24Hours
            };
        }

        private async Task<List<AdminRecentAuditLogDto>> GetRecentAuditLogsAsync()
        {
            return await _unitOfWork.Repository<AdminAuditLog, Guid>()
                .Query()
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new AdminRecentAuditLogDto
                {
                    Id = x.Id,
                    AdminEmail = x.AdminEmail,
                    Action = x.Action,
                    TargetType = x.TargetType,
                    TargetDisplayName = x.TargetDisplayName,
                    Description = x.Description,
                    CreatedAt = DateTimeHelper.EnsureUtcKind(x.CreatedAt)
                })
                .ToListAsync();
        }

        private async Task AddAuditLogAsync()
        {
            await _auditLogService.AddAsync(new CreateAdminAuditLogDto
            {
                AdminUserId = _currentUserService.UserId == Guid.Empty
                    ? null
                    : _currentUserService.UserId,
                AdminEmail = _currentUserService.Email,
                Action = AdminAuditAction.DashboardViewed,
                TargetType = AdminTargetType.System,
                Description = "Admin viewed dashboard summary"
            });
        }

        private static (DateTime MonthStartUtc, DateTime MonthEndUtc) GetCurrentMonthRangeUtc()
        {
            var egyptToday = GetEgyptToday();

            var monthStart = new DateTime(
                egyptToday.Year,
                egyptToday.Month,
                1);

            var monthEnd = monthStart.AddMonths(1);

            return (
                DateTimeHelper.ConvertClientLocalToUtc(monthStart),
                DateTimeHelper.ConvertClientLocalToUtc(monthEnd)
            );
        }

        private static DateTime GetEgyptToday()
        {
            var egyptTimeZone = DateTimeHelper.GetEgyptTimeZone();

            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                egyptTimeZone).Date;
        }
    }
}