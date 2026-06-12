using Finexa.Api.Hubs;
using Finexa.Application.Modules.Notifications.DTOs;
using Finexa.Application.Modules.Notifications.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Finexa.Api.Realtime
{
    public class SignalRNotificationSender : INotificationRealtimeSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationSender> _logger;

        public SignalRNotificationSender(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRNotificationSender> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendToUserAsync(Guid userId, NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send realtime notification to user {UserId}",
                    userId);
            }
        }
    }
}