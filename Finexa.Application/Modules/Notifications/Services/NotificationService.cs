using Finexa.Application.Common.Models;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Notifications.DTOs;
using Finexa.Application.Modules.Notifications.Interfaces;
using Finexa.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationRealtimeSender _realtimeSender;
        public NotificationService(
         IUnitOfWork unitOfWork,
         ICurrentUserService currentUserService,
         INotificationRealtimeSender realtimeSender)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _realtimeSender = realtimeSender;
        }

        public async Task<NotificationDto?> CreateForUserAsync(
            Guid userId,
            CreateNotificationDto dto)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User id is required");

            ValidateCreateNotificationDto(dto);

            var deduplicationKey = string.IsNullOrWhiteSpace(dto.DeduplicationKey)
                ? null
                : dto.DeduplicationKey.Trim();

            if (!string.IsNullOrWhiteSpace(deduplicationKey))
            {
                var exists = await _unitOfWork.Repository<Notification, Guid>()
                    .ExistsAsync(x =>
                        x.AppUserId == userId &&
                        x.DeduplicationKey == deduplicationKey);

                if (exists)
                    return null;
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                AppUserId = userId,
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                Type = dto.Type,
                Severity = dto.Severity,
                ShouldToast = dto.ShouldToast,

                RelatedEntityType = string.IsNullOrWhiteSpace(dto.RelatedEntityType)
                    ? null
                    : dto.RelatedEntityType.Trim(),

                RelatedEntityId = dto.RelatedEntityId,

                ActionUrl = string.IsNullOrWhiteSpace(dto.ActionUrl)
                    ? null
                    : dto.ActionUrl.Trim(),

                DeduplicationKey = deduplicationKey,

                CreatedBy = GetAuditUser()
            };

            await _unitOfWork.Repository<Notification, Guid>().AddAsync(notification);

            await _unitOfWork.SaveChangesAsync();

            var notificationDto = MapToDto(notification);

            await _realtimeSender.SendToUserAsync(userId, notificationDto);

            return notificationDto;
        }

        public async Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(
            NotificationFilterDto filter)
        {
            var userId = GetCurrentUserId();

            NormalizeFilter(filter);

            var query = _unitOfWork.Repository<Notification, Guid>()
                .Query()
                .Where(x => x.AppUserId == userId);

            if (filter.IsRead.HasValue)
                query = query.Where(x => x.IsRead == filter.IsRead.Value);

            if (filter.Type.HasValue)
                query = query.Where(x => x.Type == filter.Type.Value);

            if (filter.Severity.HasValue)
                query = query.Where(x => x.Severity == filter.Severity.Value);

            var totalCount = await query.CountAsync();

            var isDescending =
                filter.SortDirection?.ToString().Equals("Descending", StringComparison.OrdinalIgnoreCase) == true ||
                filter.SortDirection?.ToString().Equals("Desc", StringComparison.OrdinalIgnoreCase) == true ||
                filter.SortDirection == null;

            query = isDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt);

            var notifications = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<NotificationDto>
            {
                Items = notifications.Select(MapToDto).ToList(),
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var userId = GetCurrentUserId();

            return await _unitOfWork.Repository<Notification, Guid>()
                .Query()
                .CountAsync(x =>
                    x.AppUserId == userId &&
                    !x.IsRead);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var userId = GetCurrentUserId();

            var notification = await _unitOfWork.Repository<Notification, Guid>()
                .Query(withTracking: true)
                .FirstOrDefaultAsync(x =>
                    x.Id == notificationId &&
                    x.AppUserId == userId);

            if (notification == null)
                throw new KeyNotFoundException("Notification not found");

            notification.MarkAsRead();
            notification.LastModifiedBy = GetAuditUser();

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync()
        {
            var userId = GetCurrentUserId();

            var notifications = await _unitOfWork.Repository<Notification, Guid>()
                .Query(withTracking: true)
                .Where(x =>
                    x.AppUserId == userId &&
                    !x.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.MarkAsRead();
                notification.LastModifiedBy = GetAuditUser();
            }

            if (notifications.Any())
                await _unitOfWork.SaveChangesAsync();
        }

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Severity = notification.Severity,
                ShouldToast = notification.ShouldToast,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                RelatedEntityType = notification.RelatedEntityType,
                RelatedEntityId = notification.RelatedEntityId,
                ActionUrl = notification.ActionUrl,
                CreatedAt = notification.CreatedAt
            };
        }

        private static void ValidateCreateNotificationDto(
            CreateNotificationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Notification title is required");

            if (dto.Title.Length > 150)
                throw new ArgumentException("Notification title cannot exceed 150 characters");

            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new ArgumentException("Notification message is required");

            if (dto.Message.Length > 1000)
                throw new ArgumentException("Notification message cannot exceed 1000 characters");

            if (!string.IsNullOrWhiteSpace(dto.RelatedEntityType) &&
                dto.RelatedEntityType.Length > 100)
                throw new ArgumentException("Related entity type cannot exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(dto.ActionUrl) &&
                dto.ActionUrl.Length > 300)
                throw new ArgumentException("Action URL cannot exceed 300 characters");

            if (!string.IsNullOrWhiteSpace(dto.DeduplicationKey) &&
                dto.DeduplicationKey.Length > 250)
                throw new ArgumentException("Deduplication key cannot exceed 250 characters");
        }

        private static void NormalizeFilter(NotificationFilterDto filter)
        {
            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 10;

            if (filter.PageSize > 100)
                filter.PageSize = 100;
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