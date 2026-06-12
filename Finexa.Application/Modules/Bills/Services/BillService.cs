using Finexa.Application.Common.Helpers;
using Finexa.Application.Common.Models;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Bills.DTOs;
using Finexa.Application.Modules.Bills.Interfaces;
using Finexa.Domain.Entities;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Bills.Services
{
    public class BillService : IBillService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public BillService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> CreateBillSeriesAsync(CreateBillSeriesDto dto)
        {
            var userId = GetCurrentUserId();

            ValidateCreateBillSeriesDto(dto);

            var category = await GetValidExpenseCategoryAsync(dto.CategoryId, userId);

            var billSeries = new BillSeries
            {
                Id = Guid.NewGuid(),
                AppUserId = userId,
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),
                CategoryId = category.Id,
                DefaultAmount = dto.AmountType == BillAmountType.Fixed ? dto.DefaultAmount : null,
                AmountType = dto.AmountType,
                Frequency = dto.Frequency,
                DueDay = dto.Frequency == BillFrequency.Monthly ? dto.DueDay : null,
                DueDate = dto.Frequency == BillFrequency.OneTime ||
                          dto.Frequency == BillFrequency.Yearly
                    ? DateTimeHelper.ConvertClientLocalToUtc(dto.DueDate!.Value)
                    : null,
                StartDate = DateTimeHelper.ConvertClientLocalToUtc(dto.StartDate),
                EndDate = dto.EndDate.HasValue
                    ? DateTimeHelper.ConvertClientLocalToUtc(dto.EndDate.Value)
                    : null,
                IsActive = true,
                ReminderDaysBefore = dto.ReminderDaysBefore,
                AllowsEarlyRenewal = dto.AllowsEarlyRenewal,
                AllowsTopUp = dto.AllowsTopUp,
                CreatedBy = GetAuditUser()
            };

            var firstOccurrence = CreateFirstScheduledOccurrence(billSeries, userId);

            await _unitOfWork.Repository<BillSeries, Guid>().AddAsync(billSeries);
            await _unitOfWork.Repository<BillOccurrence, Guid>().AddAsync(firstOccurrence);

            await _unitOfWork.SaveChangesAsync();

            return billSeries.Id;
        }

        public async Task UpdateBillSeriesAsync(Guid billSeriesId, UpdateBillSeriesDto dto)
        {
            var userId = GetCurrentUserId();

            ValidateUpdateBillSeriesDto(dto);

            var billSeries = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query(withTracking: true)
                .FirstOrDefaultAsync(b => b.Id == billSeriesId && b.AppUserId == userId);

            if (billSeries == null)
                throw new KeyNotFoundException("Bill not found");

            var category = await GetValidExpenseCategoryAsync(dto.CategoryId, userId);

            billSeries.Name = dto.Name.Trim();
            billSeries.Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();
            billSeries.CategoryId = category.Id;
            billSeries.DefaultAmount = dto.AmountType == BillAmountType.Fixed ? dto.DefaultAmount : null;
            billSeries.AmountType = dto.AmountType;
            billSeries.Frequency = dto.Frequency;
            billSeries.DueDay = dto.Frequency == BillFrequency.Monthly ? dto.DueDay : null;
            billSeries.DueDate = dto.Frequency == BillFrequency.OneTime ||
                                 dto.Frequency == BillFrequency.Yearly
                ? DateTimeHelper.ConvertClientLocalToUtc(dto.DueDate!.Value)
                : null;
            billSeries.StartDate = DateTimeHelper.ConvertClientLocalToUtc(dto.StartDate);
            billSeries.EndDate = dto.EndDate.HasValue
                ? DateTimeHelper.ConvertClientLocalToUtc(dto.EndDate.Value)
                : null;
            billSeries.IsActive = dto.IsActive;
            billSeries.ReminderDaysBefore = dto.ReminderDaysBefore;
            billSeries.AllowsEarlyRenewal = dto.AllowsEarlyRenewal;
            billSeries.AllowsTopUp = dto.AllowsTopUp;
            billSeries.LastModifiedBy = GetAuditUser();

            await UpdateFutureUnpaidScheduledOccurrencesAsync(billSeries, userId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelBillSeriesAsync(Guid billSeriesId)
        {
            var userId = GetCurrentUserId();

            var billSeries = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query(withTracking: true)
                .FirstOrDefaultAsync(b => b.Id == billSeriesId && b.AppUserId == userId);

            if (billSeries == null)
                throw new KeyNotFoundException("Bill not found");

            billSeries.IsActive = false;
            billSeries.LastModifiedBy = GetAuditUser();

            var futureOccurrences = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query(withTracking: true)
                .Where(o =>
                    o.BillSeriesId == billSeriesId &&
                    o.AppUserId == userId &&
                    o.Status == BillOccurrenceStatus.Scheduled &&
                    o.DueDate >= DateTime.UtcNow)
                .ToListAsync();

            foreach (var occurrence in futureOccurrences)
            {
                occurrence.Status = BillOccurrenceStatus.Cancelled;
                occurrence.LastModifiedBy = GetAuditUser();
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<BillSeriesDto>> GetMyBillsAsync()
        {
            var userId = GetCurrentUserId();

            await GenerateUpcomingOccurrencesForUserAsync(userId);

            var bills = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query()
                .Where(b => b.AppUserId == userId)
                .Include(b => b.Category)
                .Include(b => b.Occurrences)
                .OrderByDescending(b => b.IsActive)
                .ThenBy(b => b.Name)
                .ToListAsync();

            return bills.Select(MapBillSeriesToDto).ToList();
        }

        public async Task<BillSeriesDetailsDto> GetBillDetailsAsync(Guid billSeriesId)
        {
            var userId = GetCurrentUserId();

            await GenerateUpcomingOccurrencesForUserAsync(userId);

            var bill = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query()
                .Where(b => b.Id == billSeriesId && b.AppUserId == userId)
                .Include(b => b.Category)
                .Include(b => b.Occurrences)
                .FirstOrDefaultAsync();

            if (bill == null)
                throw new KeyNotFoundException("Bill not found");

            var payments = await _unitOfWork.Repository<BillPayment, Guid>()
                .Query()
                .Where(p => p.BillSeriesId == billSeriesId && p.AppUserId == userId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();

            return new BillSeriesDetailsDto
            {
                Id = bill.Id,
                Name = bill.Name,
                Description = bill.Description,
                CategoryId = bill.CategoryId,
                CategoryName = bill.Category?.Name ?? "Unknown",
                DefaultAmount = bill.DefaultAmount,
                AmountType = bill.AmountType,
                Frequency = bill.Frequency,
                DueDay = bill.DueDay,
                DueDate = DateTimeHelper.EnsureUtcKind(bill.DueDate),
                StartDate = DateTimeHelper.EnsureUtcKind(bill.StartDate),
                EndDate = DateTimeHelper.EnsureUtcKind(bill.EndDate),
                IsActive = bill.IsActive,
                ReminderDaysBefore = bill.ReminderDaysBefore,
                AllowsEarlyRenewal = bill.AllowsEarlyRenewal,
                AllowsTopUp = bill.AllowsTopUp,
                CurrentOccurrence = GetCurrentOccurrenceDto(bill),
                Occurrences = bill.Occurrences
                    .OrderBy(o => o.DueDate)
                    .Select(o => MapOccurrenceToDto(o, bill))
                    .ToList(),
                Payments = payments.Select(MapPaymentToDto).ToList()
            };
        }

        public async Task<List<BillOccurrenceDto>> GetBillOccurrencesAsync(Guid billSeriesId)
        {
            var userId = GetCurrentUserId();

            await GenerateUpcomingOccurrencesForUserAsync(userId);

            var bill = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query()
                .Where(b => b.Id == billSeriesId && b.AppUserId == userId)
                .Include(b => b.Occurrences)
                .FirstOrDefaultAsync();

            if (bill == null)
                throw new KeyNotFoundException("Bill not found");

            return bill.Occurrences
                .OrderBy(o => o.DueDate)
                .Select(o => MapOccurrenceToDto(o, bill))
                .ToList();
        }

        public async Task<BillDashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var userId = GetCurrentUserId();

            await GenerateUpcomingOccurrencesForUserAsync(userId);

            var egyptTimeZone = DateTimeHelper.GetEgyptTimeZone();
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var today = localNow.Date;
            var weekEnd = today.AddDays(7);

            var monthStartLocal = new DateTime(localNow.Year, localNow.Month, 1);
            var nextMonthStartLocal = monthStartLocal.AddMonths(1);

            var monthStartUtc = DateTimeHelper.ConvertClientLocalToUtc(monthStartLocal);
            var nextMonthStartUtc = DateTimeHelper.ConvertClientLocalToUtc(nextMonthStartLocal);

            var occurrences = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query()
                .Where(o =>
                    o.AppUserId == userId &&
                    o.Status != BillOccurrenceStatus.Cancelled &&
                    o.Status != BillOccurrenceStatus.Skipped)
                    .Include(o => o.BillSeries)
                     .ThenInclude(b => b.Category).ToListAsync();

            var currentMonthOccurrences = occurrences
                .Where(o => o.DueDate >= monthStartUtc && o.DueDate < nextMonthStartUtc)
                .ToList();

            var paymentsThisMonth = await _unitOfWork.Repository<BillPayment, Guid>()
                .Query()
                .Where(p =>
                    p.AppUserId == userId &&
                    p.Status == BillPaymentStatus.Completed &&
                    p.PaidAt >= monthStartUtc &&
                    p.PaidAt < nextMonthStartUtc)
                .ToListAsync();

            var dueThisWeek = occurrences
                .Where(o =>
                    o.Status == BillOccurrenceStatus.Scheduled &&
                    GetLocalDate(o.DueDate) >= today &&
                    GetLocalDate(o.DueDate) <= weekEnd)
                .OrderBy(o => o.DueDate)
                .ToList();

            var overdue = occurrences
                .Where(o => GetDisplayStatus(o, o.BillSeries.ReminderDaysBefore) == "Overdue")
                .OrderBy(o => o.DueDate)
                .ToList();

            var upcoming = occurrences
                .Where(o =>
                    o.Status == BillOccurrenceStatus.Scheduled &&
                    GetLocalDate(o.DueDate) >= today)
                .OrderBy(o => o.DueDate)
                .ToList();

            var expectedThisMonth = currentMonthOccurrences
                .Where(o =>
                    o.Status == BillOccurrenceStatus.Scheduled ||
                    o.Status == BillOccurrenceStatus.Paid)
                .Sum(o => o.Amount ?? 0);

            var billsThisMonthCount = currentMonthOccurrences
                .Count(o =>
                    o.Status == BillOccurrenceStatus.Scheduled ||
                    o.Status == BillOccurrenceStatus.Paid);

            var paidThisMonthAmount = paymentsThisMonth.Sum(p => p.AmountPaid);
            var paidThisMonthCount = paymentsThisMonth.Count;

            return new BillDashboardSummaryDto
            {
                ExpectedThisMonth = expectedThisMonth,
                BillsThisMonthCount = billsThisMonthCount,

                DueThisWeekAmount = dueThisWeek.Sum(o => o.Amount ?? 0),
                DueThisWeekCount = dueThisWeek.Count,

                PaidThisMonthAmount = paidThisMonthAmount,
                PaidThisMonthCount = paidThisMonthCount,

                PaidCompletionPercentage = billsThisMonthCount == 0
                    ? 0
                    : Math.Round((decimal)paidThisMonthCount / billsThisMonthCount * 100, 2),

                OverdueAmount = overdue.Sum(o => o.Amount ?? 0),
                OverdueCount = overdue.Count,

                NextBillDue = upcoming.FirstOrDefault() == null
                    ? null
                    : MapOccurrenceToDto(upcoming.First(), upcoming.First().BillSeries),

                UpcomingBills = upcoming
                    .Take(5)
                    .Select(o => MapOccurrenceToDto(o, o.BillSeries))
                    .ToList(),

                OverdueBills = overdue
                    .Take(5)
                    .Select(o => MapOccurrenceToDto(o, o.BillSeries))
                    .ToList()
            };
        }

        public async Task<BillCalendarDto> GetCalendarAsync(int year, int month)
        {
            var userId = GetCurrentUserId();

            if (year < 2000 || year > 2100)
                throw new ArgumentException("Invalid year");

            if (month < 1 || month > 12)
                throw new ArgumentException("Invalid month");

            await GenerateUpcomingOccurrencesForUserAsync(userId);

            var localMonthStart = new DateTime(year, month, 1);
            var localNextMonthStart = localMonthStart.AddMonths(1);

            var monthStartUtc = DateTimeHelper.ConvertClientLocalToUtc(localMonthStart);
            var nextMonthStartUtc = DateTimeHelper.ConvertClientLocalToUtc(localNextMonthStart);

            var occurrences = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query()
                .Where(o =>
                    o.AppUserId == userId &&
                    o.DueDate >= monthStartUtc &&
                    o.DueDate < nextMonthStartUtc &&
                    o.Status != BillOccurrenceStatus.Cancelled &&
                    o.Status != BillOccurrenceStatus.Skipped)
                .Include(o => o.BillSeries)
                .ThenInclude(b => b.Category).OrderBy(o => o.DueDate)
                .ToListAsync();

            return new BillCalendarDto
            {
                Year = year,
                Month = month,
                Occurrences = occurrences.Select(o => new BillCalendarOccurrenceDto
                {
                    Id = o.Id,
                    BillSeriesId = o.BillSeriesId,
                    BillName = o.BillSeries.Name,
                    Title = o.Title,
                    Amount = o.Amount,
                    DueDate = DateTimeHelper.EnsureUtcKind(o.DueDate),
                    Status = o.Status,
                    DisplayStatus = GetDisplayStatus(o, o.BillSeries.ReminderDaysBefore),
                    OccurrenceType = o.OccurrenceType
                }).ToList()
            };
        }
        public async Task<PagedResult<BillOccurrenceDto>> GetAllOccurrencesAsync(BillOccurrenceFilterDto filter)
        {
            var userId = GetCurrentUserId();

            await GenerateUpcomingOccurrencesForUserAsync(userId);

            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 10;

            if (filter.PageSize > 100)
                filter.PageSize = 100;

            var query = _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query()
                .Where(o => o.AppUserId == userId)
                .Include(o => o.BillSeries)
                .ThenInclude(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();

                query = query.Where(o =>
                    o.Title.ToLower().Contains(search) ||
                    o.BillSeries.Name.ToLower().Contains(search));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(o => o.Status == filter.Status.Value);
            }

            if (filter.FromDate.HasValue)
            {
                var fromUtc = DateTimeHelper.ConvertClientLocalToUtc(
                    filter.FromDate.Value.Date);

                query = query.Where(o => o.DueDate >= fromUtc);
            }

            if (filter.ToDate.HasValue)
            {
                var toUtc = DateTimeHelper.ConvertClientLocalToUtc(
                    filter.ToDate.Value.Date.AddDays(1));

                query = query.Where(o => o.DueDate < toUtc);
            }

            var totalCount = await query.CountAsync();

            var isDescending =
                filter.SortDirection?.ToString().Equals("Descending", StringComparison.OrdinalIgnoreCase) == true ||
                filter.SortDirection?.ToString().Equals("Desc", StringComparison.OrdinalIgnoreCase) == true;

            query = isDescending
                ? query.OrderByDescending(o => o.DueDate)
                : query.OrderBy(o => o.DueDate);

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<BillOccurrenceDto>
            {
                Items = items
                    .Select(o => MapOccurrenceToDto(o, o.BillSeries))
                    .ToList(),

                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }
        public async Task<int> GenerateUpcomingOccurrencesForAllUsersAsync(int daysAhead = 90)
        {
            if (daysAhead <= 0)
                throw new ArgumentException("Days ahead must be greater than zero");

            if (daysAhead > 365)
                throw new ArgumentException("Days ahead cannot be greater than 365");

            var bills = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query(withTracking: true)
                .Where(b => b.IsActive)
                .Include(b => b.Occurrences)
                .ToListAsync();

            var createdCount = 0;

            foreach (var bill in bills)
            {
                createdCount += await GenerateUpcomingOccurrencesForBillAsync(
                    bill,
                    bill.AppUserId,
                    daysAhead);
            }

            if (createdCount > 0)
                await _unitOfWork.SaveChangesAsync();

            return createdCount;
        }

        public async Task<BillPaymentResultDto> RecordPaymentAsync(
            Guid occurrenceId,
            RecordBillPaymentDto dto)
        {
            var userId = GetCurrentUserId();

            var occurrence = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query(withTracking: true)
                .Include(o => o.BillSeries)
                .ThenInclude(b => b.Category)
                .FirstOrDefaultAsync(o => o.Id == occurrenceId && o.AppUserId == userId);

            if (occurrence == null)
                throw new KeyNotFoundException("Bill occurrence not found");

            var result = await RecordPaymentInternalAsync(
                occurrence,
                occurrence.BillSeries,
                dto,
                userId);

            await _unitOfWork.SaveChangesAsync();

            return result;
        }

        public async Task ReversePaymentAsync(Guid paymentId)
        {
            var userId = GetCurrentUserId();

            var payment = await _unitOfWork.Repository<BillPayment, Guid>()
                .Query(withTracking: true)
                .Include(p => p.BillOccurrence)
                .Include(p => p.BillSeries)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.AppUserId == userId);

            if (payment == null)
                throw new KeyNotFoundException("Bill payment not found");

            if (payment.Status == BillPaymentStatus.Reversed)
                throw new InvalidOperationException("Payment is already reversed");

            if (payment.ReversalTransactionId.HasValue)
                throw new InvalidOperationException("Payment already has a reversal transaction");

            var adjustmentCategory = await GetBalanceAdjustmentCategoryAsync(userId);

            var reversalTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = payment.AmountPaid,
                Type = TransactionType.Income,
                CategoryId = adjustmentCategory.Id,
                Notes = $"Reversal for bill payment: {payment.BillOccurrence.Title}",
                OccurredAt = DateTime.UtcNow,
                AppUserId = userId,
                Source = TransactionSource.Bill,
                Merchant = payment.BillSeries.Name,
                Item = "Bill Payment Reversal",
                CreatedBy = GetAuditUser()
            };

            await _unitOfWork.Repository<Transaction, Guid>().AddAsync(reversalTransaction);

            await ApplyTransactionEffectToBalanceAsync(
                userId,
                reversalTransaction.Amount,
                reversalTransaction.Type);

            payment.Status = BillPaymentStatus.Reversed;
            payment.ReversalTransactionId = reversalTransaction.Id;
            payment.ReversedAt = DateTime.UtcNow;
            payment.LastModifiedBy = GetAuditUser();

            payment.BillOccurrence.Status =
                payment.BillOccurrence.OccurrenceType == BillOccurrenceType.Scheduled
                    ? BillOccurrenceStatus.Scheduled
                    : BillOccurrenceStatus.Cancelled;

            payment.BillOccurrence.PaidAt = null;
            payment.BillOccurrence.LastModifiedBy = GetAuditUser();

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SkipOccurrenceAsync(Guid occurrenceId, string? notes = null)
        {
            var userId = GetCurrentUserId();

            var occurrence = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query(withTracking: true)
                .FirstOrDefaultAsync(o => o.Id == occurrenceId && o.AppUserId == userId);

            if (occurrence == null)
                throw new KeyNotFoundException("Bill occurrence not found");

            if (occurrence.Status == BillOccurrenceStatus.Paid)
                throw new InvalidOperationException("Paid occurrence cannot be skipped. Reverse payment first.");

            if (occurrence.Status == BillOccurrenceStatus.Skipped)
                throw new InvalidOperationException("Occurrence is already skipped");

            if (occurrence.Status == BillOccurrenceStatus.Cancelled)
                throw new InvalidOperationException("Occurrence is already cancelled");

            occurrence.Status = BillOccurrenceStatus.Skipped;

            if (!string.IsNullOrWhiteSpace(notes))
                occurrence.Notes = notes.Trim();

            occurrence.LastModifiedBy = GetAuditUser();

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelOccurrenceAsync(Guid occurrenceId, string? notes = null)
        {
            var userId = GetCurrentUserId();

            var occurrence = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query(withTracking: true)
                .FirstOrDefaultAsync(o => o.Id == occurrenceId && o.AppUserId == userId);

            if (occurrence == null)
                throw new KeyNotFoundException("Bill occurrence not found");

            if (occurrence.Status == BillOccurrenceStatus.Paid)
                throw new InvalidOperationException("Paid occurrence cannot be cancelled. Reverse payment first.");

            if (occurrence.Status == BillOccurrenceStatus.Skipped)
                throw new InvalidOperationException("Occurrence is already skipped");

            if (occurrence.Status == BillOccurrenceStatus.Cancelled)
                throw new InvalidOperationException("Occurrence is already cancelled");

            occurrence.Status = BillOccurrenceStatus.Cancelled;

            if (!string.IsNullOrWhiteSpace(notes))
                occurrence.Notes = notes.Trim();

            occurrence.LastModifiedBy = GetAuditUser();

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<BillPaymentResultDto> RenewEarlyAsync(
            Guid billSeriesId,
            RenewEarlyBillDto dto)
        {
            var userId = GetCurrentUserId();

            var bill = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query(withTracking: true)
                .FirstOrDefaultAsync(b => b.Id == billSeriesId && b.AppUserId == userId);

            if (bill == null)
                throw new KeyNotFoundException("Bill not found");

            if (!bill.IsActive)
                throw new InvalidOperationException("Bill series is not active");

            if (!bill.AllowsEarlyRenewal)
                throw new InvalidOperationException("Early renewal is not allowed for this bill");

            decimal amount;

            if (bill.AmountType == BillAmountType.Fixed)
            {
                amount = dto.Amount ?? bill.DefaultAmount
                    ?? throw new InvalidOperationException("Amount is required for early renewal");

                if (amount <= 0)
                    throw new InvalidOperationException("Amount must be greater than zero");
            }
            else
            {
                if (!dto.Amount.HasValue || dto.Amount.Value <= 0)
                    throw new ArgumentException("Amount is required for early renewal");

                amount = dto.Amount.Value;
            }

            var paidAtUtc = dto.PaidAt.HasValue
                ? DateTimeHelper.ConvertClientLocalToUtc(dto.PaidAt.Value)
                : DateTime.UtcNow;

            var paidAtLocal = GetLocalDate(paidAtUtc);

            var occurrence = CreateSpecialOccurrence(
                bill,
                userId,
                BillOccurrenceType.EarlyRenewal,
                amount,
                paidAtLocal,
                dto.Notes);

            await _unitOfWork.Repository<BillOccurrence, Guid>().AddAsync(occurrence);

            var result = await RecordPaymentInternalAsync(
                occurrence,
                bill,
                new RecordBillPaymentDto
                {
                    Amount = amount,
                    PaidAt = dto.PaidAt,
                    Notes = dto.Notes
                },
                userId);

            await _unitOfWork.SaveChangesAsync();

            return result;
        }

        public async Task<BillPaymentResultDto> AddTopUpAsync(
            Guid billSeriesId,
            AddBillTopUpDto dto)
        {
            var userId = GetCurrentUserId();

            var bill = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query(withTracking: true)
                .FirstOrDefaultAsync(b => b.Id == billSeriesId && b.AppUserId == userId);

            if (bill == null)
                throw new KeyNotFoundException("Bill not found");

            if (!bill.IsActive)
                throw new InvalidOperationException("Bill series is not active");

            if (!bill.AllowsTopUp)
                throw new InvalidOperationException("Top-up is not allowed for this bill");

            if (dto.Amount <= 0)
                throw new ArgumentException("Top-up amount must be greater than zero");

            var paidAtUtc = dto.PaidAt.HasValue
                ? DateTimeHelper.ConvertClientLocalToUtc(dto.PaidAt.Value)
                : DateTime.UtcNow;

            var paidAtLocal = GetLocalDate(paidAtUtc);

            var occurrence = CreateSpecialOccurrence(
                bill,
                userId,
                BillOccurrenceType.TopUp,
                dto.Amount,
                paidAtLocal,
                dto.Notes);

            await _unitOfWork.Repository<BillOccurrence, Guid>().AddAsync(occurrence);

            var result = await RecordPaymentInternalAsync(
                occurrence,
                bill,
                new RecordBillPaymentDto
                {
                    Amount = dto.Amount,
                    PaidAt = dto.PaidAt,
                    Notes = dto.Notes
                },
                userId);

            await _unitOfWork.SaveChangesAsync();

            return result;
        }

        private async Task<BillPaymentResultDto> RecordPaymentInternalAsync(
            BillOccurrence occurrence,
            BillSeries bill,
            RecordBillPaymentDto dto,
            Guid userId)
        {
            if (!bill.IsActive)
                throw new InvalidOperationException("Bill series is not active");

            if (occurrence.Status == BillOccurrenceStatus.Paid)
                throw new InvalidOperationException("This bill occurrence is already paid");

            if (occurrence.Status == BillOccurrenceStatus.Skipped)
                throw new InvalidOperationException("Skipped bill occurrence cannot be paid");

            if (occurrence.Status == BillOccurrenceStatus.Cancelled)
                throw new InvalidOperationException("Cancelled bill occurrence cannot be paid");

            var alreadyPaid = await _unitOfWork.Repository<BillPayment, Guid>()
                .ExistsAsync(p =>
                    p.BillOccurrenceId == occurrence.Id &&
                    p.Status == BillPaymentStatus.Completed);

            if (alreadyPaid)
                throw new InvalidOperationException("This bill occurrence already has a completed payment");

            decimal amount;

            if (bill.AmountType == BillAmountType.Fixed)
            {
                if (!occurrence.Amount.HasValue || occurrence.Amount.Value <= 0)
                    throw new InvalidOperationException("Fixed bill occurrence amount is missing");

                amount = dto.Amount ?? occurrence.Amount.Value;

                if (amount != occurrence.Amount.Value)
                    throw new InvalidOperationException("Fixed bill must be recorded with the full occurrence amount");
            }
            else
            {
                if (!dto.Amount.HasValue || dto.Amount.Value <= 0)
                    throw new ArgumentException("Amount is required for variable bills");

                if (occurrence.Amount.HasValue &&
                    occurrence.Amount.Value > 0 &&
                    dto.Amount.Value != occurrence.Amount.Value)
                {
                    throw new InvalidOperationException("Payment amount must match the occurrence amount");
                }

                amount = dto.Amount.Value;
            }

            var paidAtUtc = dto.PaidAt.HasValue
                ? DateTimeHelper.ConvertClientLocalToUtc(dto.PaidAt.Value)
                : DateTime.UtcNow;

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                Type = TransactionType.Expense,
                CategoryId = bill.CategoryId,
                Notes = string.IsNullOrWhiteSpace(dto.Notes)
                    ? $"Paid bill: {occurrence.Title}"
                    : dto.Notes.Trim(),
                OccurredAt = paidAtUtc,
                AppUserId = userId,
                Source = TransactionSource.Bill,
                Merchant = bill.Name,
                Item = occurrence.Title,
                CreatedBy = GetAuditUser()
            };

            await _unitOfWork.Repository<Transaction, Guid>().AddAsync(transaction);

            await ApplyTransactionEffectToBalanceAsync(userId, transaction.Amount, transaction.Type);

            var payment = new BillPayment
            {
                Id = Guid.NewGuid(),
                BillSeriesId = bill.Id,
                BillOccurrenceId = occurrence.Id,
                AppUserId = userId,
                TransactionId = transaction.Id,
                AmountPaid = amount,
                PaidAt = paidAtUtc,
                Status = BillPaymentStatus.Completed,
                Notes = string.IsNullOrWhiteSpace(dto.Notes)
                    ? null
                    : dto.Notes.Trim(),
                CreatedBy = GetAuditUser()
            };

            await _unitOfWork.Repository<BillPayment, Guid>().AddAsync(payment);

            occurrence.Amount = amount;
            occurrence.Status = BillOccurrenceStatus.Paid;
            occurrence.PaidAt = paidAtUtc;
            occurrence.LastModifiedBy = GetAuditUser();

            await CreateNextOccurrenceIfNeededAsync(bill, occurrence, userId);

            return new BillPaymentResultDto
            {
                OccurrenceId = occurrence.Id,
                PaymentId = payment.Id,
                TransactionId = transaction.Id,
                Amount = amount,
                PaidAt = paidAtUtc
            };
        }

        private async Task CreateNextOccurrenceIfNeededAsync(
            BillSeries bill,
            BillOccurrence paidOccurrence,
            Guid userId)
        {
            if (!bill.IsActive)
                return;

            if (bill.Frequency == BillFrequency.OneTime)
                return;

            if (paidOccurrence.OccurrenceType != BillOccurrenceType.Scheduled)
                return;

            var nextOccurrence = CreateNextScheduledOccurrence(bill, paidOccurrence, userId);

            if (bill.EndDate.HasValue && nextOccurrence.DueDate > bill.EndDate.Value)
                return;

            var exists = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .ExistsAsync(o =>
                    o.BillSeriesId == bill.Id &&
                    o.DueDate == nextOccurrence.DueDate &&
                    o.OccurrenceType == BillOccurrenceType.Scheduled);

            if (exists)
                return;

            await _unitOfWork.Repository<BillOccurrence, Guid>().AddAsync(nextOccurrence);
        }

        private async Task UpdateFutureUnpaidScheduledOccurrencesAsync(
            BillSeries bill,
            Guid userId)
        {
            var futureOccurrences = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query(withTracking: true)
                .Where(o =>
                    o.BillSeriesId == bill.Id &&
                    o.AppUserId == userId &&
                    o.OccurrenceType == BillOccurrenceType.Scheduled &&
                    o.Status == BillOccurrenceStatus.Scheduled &&
                    o.DueDate >= DateTime.UtcNow)
                .ToListAsync();

            foreach (var occurrence in futureOccurrences)
            {
                var localPeriodStart = GetLocalDate(occurrence.PeriodStart);

                DateTime localDueDate;
                DateTime localPeriodStartUpdated;
                DateTime localPeriodEndUpdated;

                if (bill.Frequency == BillFrequency.Monthly)
                {
                    var day = Math.Min(
                        bill.DueDay!.Value,
                        DateTime.DaysInMonth(localPeriodStart.Year, localPeriodStart.Month));

                    localDueDate = new DateTime(localPeriodStart.Year, localPeriodStart.Month, day);
                    localPeriodStartUpdated = new DateTime(localDueDate.Year, localDueDate.Month, 1);
                    localPeriodEndUpdated = localPeriodStartUpdated.AddMonths(1).AddTicks(-1);
                }
                else if (bill.Frequency == BillFrequency.Yearly)
                {
                    var yearlyDueDate = GetLocalDate(bill.DueDate!.Value);

                    var day = Math.Min(
                        yearlyDueDate.Day,
                        DateTime.DaysInMonth(localPeriodStart.Year, yearlyDueDate.Month));

                    localDueDate = new DateTime(localPeriodStart.Year, yearlyDueDate.Month, day);
                    localPeriodStartUpdated = new DateTime(localDueDate.Year, 1, 1);
                    localPeriodEndUpdated = new DateTime(localDueDate.Year, 12, 31, 23, 59, 59);
                }
                else
                {
                    localDueDate = GetLocalDate(bill.DueDate!.Value);
                    localPeriodStartUpdated = localDueDate;
                    localPeriodEndUpdated = localDueDate;
                }

                occurrence.Title = BuildOccurrenceTitle(
                    bill.Name,
                    localDueDate,
                    BillOccurrenceType.Scheduled);

                occurrence.Amount = bill.AmountType == BillAmountType.Fixed
                    ? bill.DefaultAmount
                    : null;

                occurrence.DueDate = DateTimeHelper.ConvertClientLocalToUtc(localDueDate);
                occurrence.PeriodStart = DateTimeHelper.ConvertClientLocalToUtc(localPeriodStartUpdated);
                occurrence.PeriodEnd = DateTimeHelper.ConvertClientLocalToUtc(localPeriodEndUpdated);
                occurrence.LastModifiedBy = GetAuditUser();
            }
        }

        private BillOccurrence CreateFirstScheduledOccurrence(BillSeries bill, Guid userId)
        {
            var localStartDate = GetLocalDate(bill.StartDate);

            DateTime localDueDate;
            DateTime localPeriodStart;
            DateTime localPeriodEnd;

            switch (bill.Frequency)
            {
                case BillFrequency.OneTime:
                    localDueDate = GetLocalDate(bill.DueDate!.Value);
                    localPeriodStart = localDueDate;
                    localPeriodEnd = localDueDate;
                    break;

                case BillFrequency.Monthly:
                    localDueDate = GetFirstMonthlyDueDate(localStartDate, bill.DueDay!.Value);
                    localPeriodStart = new DateTime(localDueDate.Year, localDueDate.Month, 1);
                    localPeriodEnd = localPeriodStart.AddMonths(1).AddTicks(-1);
                    break;

                case BillFrequency.Yearly:
                    localDueDate = GetFirstYearlyDueDate(localStartDate, GetLocalDate(bill.DueDate!.Value));
                    localPeriodStart = new DateTime(localDueDate.Year, 1, 1);
                    localPeriodEnd = new DateTime(localDueDate.Year, 12, 31, 23, 59, 59);
                    break;

                default:
                    throw new InvalidOperationException("Unsupported bill frequency");
            }

            if (bill.EndDate.HasValue && localDueDate > GetLocalDate(bill.EndDate.Value))
                throw new InvalidOperationException("Bill end date is before the first due date");

            return new BillOccurrence
            {
                Id = Guid.NewGuid(),
                BillSeriesId = bill.Id,
                AppUserId = userId,
                Title = BuildOccurrenceTitle(
                    bill.Name,
                    localDueDate,
                    BillOccurrenceType.Scheduled),
                Amount = bill.AmountType == BillAmountType.Fixed ? bill.DefaultAmount : null,
                DueDate = DateTimeHelper.ConvertClientLocalToUtc(localDueDate),
                PeriodStart = DateTimeHelper.ConvertClientLocalToUtc(localPeriodStart),
                PeriodEnd = DateTimeHelper.ConvertClientLocalToUtc(localPeriodEnd),
                Status = BillOccurrenceStatus.Scheduled,
                OccurrenceType = BillOccurrenceType.Scheduled,
                IsGeneratedAutomatically = true,
                CreatedBy = GetAuditUser()
            };
        }

        private BillOccurrence CreateNextScheduledOccurrence(
            BillSeries bill,
            BillOccurrence currentOccurrence,
            Guid userId)
        {
            var currentLocalDueDate = GetLocalDate(currentOccurrence.DueDate);

            DateTime nextLocalDueDate;
            DateTime nextLocalPeriodStart;
            DateTime nextLocalPeriodEnd;

            switch (bill.Frequency)
            {
                case BillFrequency.Monthly:
                    var nextMonth = currentLocalDueDate.AddMonths(1);

                    nextLocalDueDate = new DateTime(
                        nextMonth.Year,
                        nextMonth.Month,
                        Math.Min(
                            bill.DueDay!.Value,
                            DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)));

                    nextLocalPeriodStart = new DateTime(
                        nextLocalDueDate.Year,
                        nextLocalDueDate.Month,
                        1);

                    nextLocalPeriodEnd = nextLocalPeriodStart
                        .AddMonths(1)
                        .AddTicks(-1);
                    break;

                case BillFrequency.Yearly:
                    nextLocalDueDate = currentLocalDueDate.AddYears(1);
                    nextLocalPeriodStart = new DateTime(nextLocalDueDate.Year, 1, 1);
                    nextLocalPeriodEnd = new DateTime(nextLocalDueDate.Year, 12, 31, 23, 59, 59);
                    break;

                default:
                    throw new InvalidOperationException("Unsupported recurring frequency");
            }

            return new BillOccurrence
            {
                Id = Guid.NewGuid(),
                BillSeriesId = bill.Id,
                AppUserId = userId,
                Title = BuildOccurrenceTitle(
                    bill.Name,
                    nextLocalDueDate,
                    BillOccurrenceType.Scheduled),
                Amount = bill.AmountType == BillAmountType.Fixed ? bill.DefaultAmount : null,
                DueDate = DateTimeHelper.ConvertClientLocalToUtc(nextLocalDueDate),
                PeriodStart = DateTimeHelper.ConvertClientLocalToUtc(nextLocalPeriodStart),
                PeriodEnd = DateTimeHelper.ConvertClientLocalToUtc(nextLocalPeriodEnd),
                Status = BillOccurrenceStatus.Scheduled,
                OccurrenceType = BillOccurrenceType.Scheduled,
                IsGeneratedAutomatically = true,
                CreatedBy = GetAuditUser()
            };
        }

        private BillOccurrence CreateSpecialOccurrence(
            BillSeries bill,
            Guid userId,
            BillOccurrenceType occurrenceType,
            decimal amount,
            DateTime localDate,
            string? notes)
        {
            return new BillOccurrence
            {
                Id = Guid.NewGuid(),
                BillSeriesId = bill.Id,
                AppUserId = userId,
                Title = BuildOccurrenceTitle(bill.Name, localDate, occurrenceType),
                Amount = amount,
                DueDate = DateTimeHelper.ConvertClientLocalToUtc(localDate),
                PeriodStart = DateTimeHelper.ConvertClientLocalToUtc(localDate),
                PeriodEnd = DateTimeHelper.ConvertClientLocalToUtc(localDate),
                Status = BillOccurrenceStatus.Scheduled,
                OccurrenceType = occurrenceType,
                IsGeneratedAutomatically = false,
                Notes = string.IsNullOrWhiteSpace(notes)
                    ? null
                    : notes.Trim(),
                CreatedBy = GetAuditUser()
            };
        }

        private async Task<Category> GetValidExpenseCategoryAsync( Guid categoryId,Guid userId)
        {
            var category = await _unitOfWork.Repository<Category, Guid>()
                .Query()
                .FirstOrDefaultAsync(c =>
                    c.IsActive &&
                    c.Id == categoryId &&
                    (c.AppUserId == null || c.AppUserId == userId));

            if (category == null)
                throw new KeyNotFoundException("Category not found");

            if (category.Type != TransactionType.Expense)
                throw new InvalidOperationException("Bill category must be an expense category");

            if (!category.IsBillCategory)
                throw new InvalidOperationException("Selected category is not allowed for bills");

            return category;
        }

        private async Task<Category> GetBalanceAdjustmentCategoryAsync(Guid userId)
        {
            var category = await _unitOfWork.Repository<Category, Guid>()
                .Query()
                .FirstOrDefaultAsync(c =>
                    c.IsActive &&
                    c.Name == "Balance Adjustment" &&
                    (c.AppUserId == null || c.AppUserId == userId));

            if (category == null)
                throw new KeyNotFoundException("Balance Adjustment category not found");

            if (category.Type != TransactionType.Income)
                throw new InvalidOperationException("Balance Adjustment category must be income");

            return category;
        }
        private async Task ApplyTransactionEffectToBalanceAsync(
            Guid userId,
            decimal amount,
            TransactionType type)
        {
            var balanceRepo = _unitOfWork.Repository<UserBalance, Guid>();

            var balance = await balanceRepo.FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (balance == null)
            {
                balance = new UserBalance
                {
                    Id = Guid.NewGuid(),
                    AppUserId = userId,
                    TotalIncome = 0,
                    TotalExpense = 0,
                    TotalBalance = 0,
                    CreatedBy = GetAuditUser()
                };

                await balanceRepo.AddAsync(balance);
            }

            if (type == TransactionType.Income)
            {
                balance.TotalIncome += amount;
                balance.TotalBalance += amount;
            }
            else
            {
                balance.TotalExpense += amount;
                balance.TotalBalance -= amount;
            }
        }

        private void ValidateCreateBillSeriesDto(CreateBillSeriesDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Bill name is required");

            if (dto.CategoryId == Guid.Empty)
                throw new ArgumentException("Category is required");

            if (dto.AmountType == BillAmountType.Fixed &&
                (!dto.DefaultAmount.HasValue || dto.DefaultAmount.Value <= 0))
                throw new ArgumentException("Default amount must be greater than zero for fixed bills");

            if (dto.AmountType == BillAmountType.Variable &&
                dto.DefaultAmount.HasValue &&
                dto.DefaultAmount.Value < 0)
                throw new ArgumentException("Default amount cannot be negative");

            if (dto.StartDate == default)
                throw new ArgumentException("Start date is required");

            if (dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate)
                throw new ArgumentException("End date must be after start date");

            if (dto.ReminderDaysBefore < 0)
                throw new ArgumentException("Reminder days before cannot be negative");

            ValidateFrequency(dto.Frequency, dto.DueDay, dto.DueDate);
        }

        private void ValidateUpdateBillSeriesDto(UpdateBillSeriesDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Bill name is required");

            if (dto.CategoryId == Guid.Empty)
                throw new ArgumentException("Category is required");

            if (dto.AmountType == BillAmountType.Fixed &&
                (!dto.DefaultAmount.HasValue || dto.DefaultAmount.Value <= 0))
                throw new ArgumentException("Default amount must be greater than zero for fixed bills");

            if (dto.AmountType == BillAmountType.Variable &&
                dto.DefaultAmount.HasValue &&
                dto.DefaultAmount.Value < 0)
                throw new ArgumentException("Default amount cannot be negative");

            if (dto.StartDate == default)
                throw new ArgumentException("Start date is required");

            if (dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate)
                throw new ArgumentException("End date must be after start date");

            if (dto.ReminderDaysBefore < 0)
                throw new ArgumentException("Reminder days before cannot be negative");

            ValidateFrequency(dto.Frequency, dto.DueDay, dto.DueDate);
        }

        private static void ValidateFrequency(
            BillFrequency frequency,
            int? dueDay,
            DateTime? dueDate)
        {
            switch (frequency)
            {
                case BillFrequency.OneTime:
                    if (!dueDate.HasValue)
                        throw new ArgumentException("Due date is required for one-time bills");
                    break;

                case BillFrequency.Monthly:
                    if (!dueDay.HasValue)
                        throw new ArgumentException("Due day is required for monthly bills");

                    if (dueDay.Value < 1 || dueDay.Value > 31)
                        throw new ArgumentException("Due day must be between 1 and 31");
                    break;

                case BillFrequency.Yearly:
                    if (!dueDate.HasValue)
                        throw new ArgumentException("Due date is required for yearly bills");
                    break;

                //case BillFrequency.Weekly:
                //    throw new InvalidOperationException("Weekly bills are not supported yet");

                //case BillFrequency.Quarterly:
                //    throw new InvalidOperationException("Quarterly bills are not supported yet");

                default:
                    throw new InvalidOperationException("Unsupported bill frequency");
            }
        }

        private BillSeriesDto MapBillSeriesToDto(BillSeries bill)
        {
            return new BillSeriesDto
            {
                Id = bill.Id,
                Name = bill.Name,
                Description = bill.Description,
                CategoryId = bill.CategoryId,
                CategoryName = bill.Category?.Name ?? "Unknown",
                DefaultAmount = bill.DefaultAmount,
                AmountType = bill.AmountType,
                Frequency = bill.Frequency,
                DueDay = bill.DueDay,
                DueDate = DateTimeHelper.EnsureUtcKind(bill.DueDate),
                StartDate = DateTimeHelper.EnsureUtcKind(bill.StartDate),
                EndDate = DateTimeHelper.EnsureUtcKind(bill.EndDate),
                IsActive = bill.IsActive,
                ReminderDaysBefore = bill.ReminderDaysBefore,
                AllowsEarlyRenewal = bill.AllowsEarlyRenewal,
                AllowsTopUp = bill.AllowsTopUp,
                CurrentOccurrence = GetCurrentOccurrenceDto(bill)
            };
        }

        private BillOccurrenceDto? GetCurrentOccurrenceDto(BillSeries bill)
        {
            var occurrence = bill.Occurrences
                .Where(o =>
                    o.Status == BillOccurrenceStatus.Scheduled ||
                    o.Status == BillOccurrenceStatus.Paid)
                .OrderBy(o => o.Status == BillOccurrenceStatus.Paid ? 1 : 0)
                .ThenBy(o => o.DueDate)
                .FirstOrDefault();

            return occurrence == null
                ? null
                : MapOccurrenceToDto(occurrence, bill);
        }

        private BillOccurrenceDto MapOccurrenceToDto(BillOccurrence occurrence,BillSeries bill)
        {
            return new BillOccurrenceDto
            {
                Id = occurrence.Id,
                BillSeriesId = occurrence.BillSeriesId,
                Title = occurrence.Title,

                CategoryName = bill.Category?.Name ?? "Unknown",

                Amount = occurrence.Amount,
                DueDate = DateTimeHelper.EnsureUtcKind(occurrence.DueDate),
                PeriodStart = DateTimeHelper.EnsureUtcKind(occurrence.PeriodStart),
                PeriodEnd = DateTimeHelper.EnsureUtcKind(occurrence.PeriodEnd),
                Status = occurrence.Status,
                DisplayStatus = GetDisplayStatus(occurrence, bill.ReminderDaysBefore),
                OccurrenceType = occurrence.OccurrenceType,
                PaidAt = DateTimeHelper.EnsureUtcKind(occurrence.PaidAt),
                Notes = occurrence.Notes,
                CanRecordPayment = occurrence.Status == BillOccurrenceStatus.Scheduled,
                CanSkip = occurrence.Status == BillOccurrenceStatus.Scheduled,
                CanCancel = occurrence.Status == BillOccurrenceStatus.Scheduled,
                CanRenewEarly = bill.IsActive && bill.AllowsEarlyRenewal,
                CanTopUp = bill.IsActive && bill.AllowsTopUp
            };
        }

        private static BillPaymentDto MapPaymentToDto(BillPayment payment)
        {
            return new BillPaymentDto
            {
                Id = payment.Id,
                BillSeriesId = payment.BillSeriesId,
                BillOccurrenceId = payment.BillOccurrenceId,
                TransactionId = payment.TransactionId,
                AmountPaid = payment.AmountPaid,
                PaidAt = DateTimeHelper.EnsureUtcKind(payment.PaidAt),
                Status = payment.Status,
                ReversalTransactionId = payment.ReversalTransactionId,
                ReversedAt = DateTimeHelper.EnsureUtcKind(payment.ReversedAt),
                Notes = payment.Notes
            };
        }

        private static string GetDisplayStatus(
            BillOccurrence occurrence,
            int reminderDaysBefore)
        {
            if (occurrence.Status == BillOccurrenceStatus.Paid)
                return "Paid";

            if (occurrence.Status == BillOccurrenceStatus.Skipped)
                return "Skipped";

            if (occurrence.Status == BillOccurrenceStatus.Cancelled)
                return "Cancelled";

            if (!occurrence.Amount.HasValue || occurrence.Amount.Value <= 0)
                return "NeedsAmount";

            var egyptTimeZone = DateTimeHelper.GetEgyptTimeZone();

            var today = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                egyptTimeZone).Date;

            var dueDate = TimeZoneInfo.ConvertTimeFromUtc(
                DateTimeHelper.EnsureUtcKind(occurrence.DueDate),
                egyptTimeZone).Date;

            if (dueDate < today)
                return "Overdue";

            if (dueDate == today)
                return "DueToday";

            if (dueDate <= today.AddDays(reminderDaysBefore))
                return "DueSoon";

            return "Upcoming";
        }

        private static DateTime GetLocalDate(DateTime value)
        {
            var utcValue = DateTimeHelper.EnsureUtcKind(value);

            return TimeZoneInfo.ConvertTimeFromUtc(
                utcValue,
                DateTimeHelper.GetEgyptTimeZone()).Date;
        }

        private static DateTime GetFirstMonthlyDueDate(
            DateTime localStartDate,
            int dueDay)
        {
            var dueDate = new DateTime(
                localStartDate.Year,
                localStartDate.Month,
                Math.Min(
                    dueDay,
                    DateTime.DaysInMonth(localStartDate.Year, localStartDate.Month)));

            if (dueDate < localStartDate.Date)
            {
                var nextMonth = localStartDate.AddMonths(1);

                dueDate = new DateTime(
                    nextMonth.Year,
                    nextMonth.Month,
                    Math.Min(
                        dueDay,
                        DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)));
            }

            return dueDate;
        }

        private static DateTime GetFirstYearlyDueDate(
            DateTime localStartDate,
            DateTime yearlyDueDate)
        {
            var safeDay = Math.Min(
                yearlyDueDate.Day,
                DateTime.DaysInMonth(localStartDate.Year, yearlyDueDate.Month));

            var dueDate = new DateTime(localStartDate.Year, yearlyDueDate.Month, safeDay);

            if (dueDate < localStartDate.Date)
            {
                var nextYear = localStartDate.Year + 1;

                safeDay = Math.Min(
                    yearlyDueDate.Day,
                    DateTime.DaysInMonth(nextYear, yearlyDueDate.Month));

                dueDate = new DateTime(nextYear, yearlyDueDate.Month, safeDay);
            }

            return dueDate;
        }

        private static string BuildOccurrenceTitle(
            string billName,
            DateTime localDueDate,
            BillOccurrenceType occurrenceType)
        {
            return occurrenceType switch
            {
                BillOccurrenceType.Scheduled =>
                    $"{billName} - {localDueDate:MMMM yyyy}",

                BillOccurrenceType.EarlyRenewal =>
                    $"{billName} Early Renewal - {localDueDate:dd MMMM yyyy}",

                BillOccurrenceType.TopUp =>
                    $"{billName} Top-up - {localDueDate:dd MMMM yyyy}",

                BillOccurrenceType.ExtraPayment =>
                    $"{billName} Extra Payment - {localDueDate:dd MMMM yyyy}",

                BillOccurrenceType.Adjustment =>
                    $"{billName} Adjustment - {localDueDate:dd MMMM yyyy}",

                _ =>
                    $"{billName} - {localDueDate:dd MMMM yyyy}"
            };
        }

        private async Task<int> GenerateUpcomingOccurrencesForUserAsync(
            Guid userId,
            int daysAhead = 90)
        {
            var bills = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query(withTracking: true)
                .Where(b => b.AppUserId == userId && b.IsActive)
                .Include(b => b.Occurrences)
                .ToListAsync();

            var createdCount = 0;

            foreach (var bill in bills)
            {
                createdCount += await GenerateUpcomingOccurrencesForBillAsync(
                    bill,
                    userId,
                    daysAhead);
            }

            if (createdCount > 0)
                await _unitOfWork.SaveChangesAsync();

            return createdCount;
        }

        private async Task<int> GenerateUpcomingOccurrencesForBillAsync(
            BillSeries bill,
            Guid userId,
            int daysAhead)
        {
            //if (bill.Frequency == BillFrequency.Weekly ||
            //    bill.Frequency == BillFrequency.Quarterly)
            //{
            //    return 0;
            //}

            var horizonUtc = DateTime.UtcNow.AddDays(daysAhead);

            var scheduledOccurrences = bill.Occurrences
                .Where(o => o.OccurrenceType == BillOccurrenceType.Scheduled)
                .OrderBy(o => o.DueDate)
                .ToList();

            var createdCount = 0;

            if (!scheduledOccurrences.Any())
            {
                var firstOccurrence = CreateFirstScheduledOccurrence(bill, userId);

                if (firstOccurrence.DueDate <= horizonUtc &&
                    IsWithinBillEndDate(bill, firstOccurrence.DueDate))
                {
                    await _unitOfWork.Repository<BillOccurrence, Guid>().AddAsync(firstOccurrence);

                    scheduledOccurrences.Add(firstOccurrence);
                    createdCount++;
                }
            }

            if (bill.Frequency == BillFrequency.OneTime)
                return createdCount;

            var existingDueDates = scheduledOccurrences
                .Select(o => o.DueDate)
                .ToHashSet();

            var latestOccurrence = scheduledOccurrences
                .OrderByDescending(o => o.DueDate)
                .FirstOrDefault();

            while (latestOccurrence != null)
            {
                var nextOccurrence = CreateNextScheduledOccurrence(
                    bill,
                    latestOccurrence,
                    userId);

                if (nextOccurrence.DueDate > horizonUtc)
                    break;

                if (!IsWithinBillEndDate(bill, nextOccurrence.DueDate))
                    break;

                if (existingDueDates.Contains(nextOccurrence.DueDate))
                    break;

                await _unitOfWork.Repository<BillOccurrence, Guid>().AddAsync(nextOccurrence);

                existingDueDates.Add(nextOccurrence.DueDate);
                latestOccurrence = nextOccurrence;
                createdCount++;
            }

            return createdCount;
        }

        private static bool IsWithinBillEndDate(BillSeries bill, DateTime dueDateUtc)
        {
            return !bill.EndDate.HasValue || dueDateUtc <= bill.EndDate.Value;
        }

        private Guid GetCurrentUserId()
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            return userId;
        }

        private string GetAuditUser()
        {
            return string.IsNullOrWhiteSpace(_currentUserService.Email)
                ? "System"
                : _currentUserService.Email;
        }
    }
}