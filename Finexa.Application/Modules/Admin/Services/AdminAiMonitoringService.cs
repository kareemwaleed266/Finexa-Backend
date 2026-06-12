using Finexa.Application.Common.Helpers;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Admin.Services
{
    public class AdminAiMonitoringService : IAdminAiMonitoringService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAdminAuditLogService _auditLogService;

        public AdminAiMonitoringService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAdminAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
        }

        public async Task<AdminAiUsageSummaryDto> GetUsageSummaryAsync()
        {
            var (monthStartUtc, monthEndUtc) = GetCurrentMonthRangeUtc();

            var aiSources = new[]
            {
                TransactionSource.Chat,
                TransactionSource.OCR,
                TransactionSource.Speech
            };

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

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

            var totalAiTransactions = await transactionRepo.Query()
                .CountAsync(t => aiSources.Contains(t.Source));

            var aiTransactionsThisMonth = await transactionRepo.Query()
                .CountAsync(t =>
                    aiSources.Contains(t.Source) &&
                    t.OccurredAt >= monthStartUtc &&
                    t.OccurredAt < monthEndUtc);

            var aiTransactionsAmount = await transactionRepo.Query()
                .Where(t => aiSources.Contains(t.Source))
                .SumAsync(t => t.Amount);

            var totalTransactions = await transactionRepo.Query()
                .CountAsync();

            await AddAuditLogAsync();

            return new AdminAiUsageSummaryDto
            {
                TotalTransactions = totalTransactions,
                TotalAiTransactions = totalAiTransactions,
                AiTransactionsThisMonth = aiTransactionsThisMonth,
                AiTransactionsAmount = aiTransactionsAmount,
                SourceStats = sourceStats
            };
        }

        private async Task AddAuditLogAsync()
        {
            await _auditLogService.AddAsync(new CreateAdminAuditLogDto
            {
                AdminUserId = _currentUserService.UserId == Guid.Empty
                    ? null
                    : _currentUserService.UserId,
                AdminEmail = _currentUserService.Email,
                Action = AdminAuditAction.AiUsageViewed,
                TargetType = AdminTargetType.System,
                Description = "Admin viewed AI usage monitoring summary"
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