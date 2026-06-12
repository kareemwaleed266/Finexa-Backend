using Finexa.Application.Common.Helpers;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Admin.Services
{
    public class AdminBillsMonitoringService : IAdminBillsMonitoringService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAdminAuditLogService _auditLogService;

        public AdminBillsMonitoringService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAdminAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
        }

        public async Task<AdminBillsOverviewDto> GetOverviewAsync()
        {
            var (monthStartUtc, monthEndUtc) = GetCurrentMonthRangeUtc();
            var nowUtc = DateTime.UtcNow;
            var todayUtc = DateTimeHelper.ConvertClientLocalToUtc(GetEgyptToday());
            var weekEndUtc = DateTimeHelper.ConvertClientLocalToUtc(GetEgyptToday().AddDays(7));

            var overview = new AdminBillsOverviewDto
            {
                Summary = await GetSummaryAsync(
                    monthStartUtc,
                    monthEndUtc,
                    nowUtc,
                    todayUtc,
                    weekEndUtc),
                ByStatus = await GetByStatusAsync(),
                TopCategories = await GetTopCategoriesAsync()
            };

            await AddAuditLogAsync();

            return overview;
        }

        private async Task<AdminBillsStatsDto> GetSummaryAsync(
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

        private async Task<List<AdminBillsByStatusDto>> GetByStatusAsync()
        {
            return await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query()
                .GroupBy(o => o.Status)
                .Select(g => new AdminBillsByStatusDto
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(x => x.Amount) ?? 0
                })
                .ToListAsync();
        }

        private async Task<List<AdminTopBillCategoryDto>> GetTopCategoriesAsync()
        {
            var billSeries = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query()
                .Include(b => b.Category)
                .Where(b => b.IsActive)
                .ToListAsync();

            return billSeries
                .GroupBy(b => new
                {
                    b.CategoryId,
                    CategoryName = b.Category != null
                        ? b.Category.Name
                        : "Unknown"
                })
                .Select(g => new AdminTopBillCategoryDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    BillSeriesCount = g.Count(),
                    ExpectedAmount = g.Sum(x => x.DefaultAmount ?? 0)
                })
                .OrderByDescending(x => x.BillSeriesCount)
                .ThenByDescending(x => x.ExpectedAmount)
                .Take(10)
                .ToList();
        }

        private async Task AddAuditLogAsync()
        {
            await _auditLogService.AddAsync(new CreateAdminAuditLogDto
            {
                AdminUserId = _currentUserService.UserId == Guid.Empty
                    ? null
                    : _currentUserService.UserId,
                AdminEmail = _currentUserService.Email,
                Action = AdminAuditAction.BillsOverviewViewed,
                TargetType = AdminTargetType.Bill,
                Description = "Admin viewed bills monitoring overview"
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