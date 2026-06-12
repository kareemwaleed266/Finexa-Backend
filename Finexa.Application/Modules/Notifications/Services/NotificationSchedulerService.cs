using Finexa.Application.Common.Helpers;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Notifications.DTOs;
using Finexa.Application.Modules.Notifications.Interfaces;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Notifications.Services
{
    public class NotificationSchedulerService : INotificationSchedulerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public NotificationSchedulerService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task RunDailyChecksAsync()
        {
            await CreateBillReminderNotificationsAsync();
        }

        private async Task CreateBillReminderNotificationsAsync()
        {
            var egyptTimeZone = DateTimeHelper.GetEgyptTimeZone();

            var today = TimeZoneInfo
                .ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone)
                .Date;

            var occurrences = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query()
                .Where(o =>
                    o.Status == BillOccurrenceStatus.Scheduled &&
                    o.BillSeries.IsActive)
                .Include(o => o.BillSeries)
                .ToListAsync();

            foreach (var occurrence in occurrences)
            {
                var bill = occurrence.BillSeries;

                var dueDate = TimeZoneInfo
                    .ConvertTimeFromUtc(
                        DateTimeHelper.EnsureUtcKind(occurrence.DueDate),
                        egyptTimeZone)
                    .Date;

                if (dueDate < today)
                {
                    await CreateBillOverdueNotificationAsync(
                        occurrence,
                        bill);

                    continue;
                }

                if (dueDate == today)
                {
                    await CreateBillDueTodayNotificationAsync(
                        occurrence,
                        bill);

                    continue;
                }

                if (bill.ReminderDaysBefore > 0 &&
                    dueDate > today &&
                    dueDate <= today.AddDays(bill.ReminderDaysBefore))
                {
                    await CreateBillDueSoonNotificationAsync(
                        occurrence,
                        bill,
                        dueDate,
                        today);
                }
            }
        }

        private async Task CreateBillDueSoonNotificationAsync(
            BillOccurrence occurrence,
            BillSeries bill,
            DateTime dueDate,
            DateTime today)
        {
            var daysLeft = (dueDate - today).Days;

            await _notificationService.CreateForUserAsync(
                bill.AppUserId,
                new CreateNotificationDto
                {
                    Title = "Bill due soon",
                    Message = $"{bill.Name} is due in {daysLeft} day{(daysLeft == 1 ? "" : "s")}.",
                    Type = NotificationType.Bill,
                    Severity = NotificationSeverity.Warning,
                    ShouldToast = false,
                    RelatedEntityType = nameof(BillOccurrence),
                    RelatedEntityId = occurrence.Id,
                    ActionUrl = "/bills",
                    DeduplicationKey = $"bill-due-soon-{occurrence.Id}"
                });
        }

        private async Task CreateBillDueTodayNotificationAsync(
            BillOccurrence occurrence,
            BillSeries bill)
        {
            await _notificationService.CreateForUserAsync(
                bill.AppUserId,
                new CreateNotificationDto
                {
                    Title = "Bill due today",
                    Message = $"{bill.Name} is due today.",
                    Type = NotificationType.Bill,
                    Severity = NotificationSeverity.Critical,
                    ShouldToast = true,
                    RelatedEntityType = nameof(BillOccurrence),
                    RelatedEntityId = occurrence.Id,
                    ActionUrl = "/bills",
                    DeduplicationKey = $"bill-due-today-{occurrence.Id}"
                });
        }

        private async Task CreateBillOverdueNotificationAsync(
            BillOccurrence occurrence,
            BillSeries bill)
        {
            await _notificationService.CreateForUserAsync(
                bill.AppUserId,
                new CreateNotificationDto
                {
                    Title = "Bill overdue",
                    Message = $"{bill.Name} is overdue.",
                    Type = NotificationType.Bill,
                    Severity = NotificationSeverity.Critical,
                    ShouldToast = true,
                    RelatedEntityType = nameof(BillOccurrence),
                    RelatedEntityId = occurrence.Id,
                    ActionUrl = "/bills",
                    DeduplicationKey = $"bill-overdue-{occurrence.Id}"
                });
        }
    }
}