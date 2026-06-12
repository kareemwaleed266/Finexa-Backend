using Finexa.Application.Modules.Notifications.DTOs;

namespace Finexa.Application.Modules.Notifications.Interfaces
{
    public interface INotificationRealtimeSender
    {
        Task SendToUserAsync(Guid userId, NotificationDto notification);
    }
}