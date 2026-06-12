using Finexa.Application.Common.Models;
using Finexa.Application.Modules.Notifications.DTOs;

namespace Finexa.Application.Modules.Notifications.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto?> CreateForUserAsync(
            Guid userId,
            CreateNotificationDto dto);

        Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(
            NotificationFilterDto filter);

        Task<int> GetUnreadCountAsync();

        Task MarkAsReadAsync(Guid notificationId);

        Task MarkAllAsReadAsync();
    }
}