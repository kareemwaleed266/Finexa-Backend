using Finexa.Application.Modules.Notifications.DTOs;
using Finexa.Application.Modules.Notifications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] NotificationFilterDto filter)
        {
            var result = await _notificationService.GetMyNotificationsAsync(filter);

            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var unreadCount = await _notificationService.GetUnreadCountAsync();

            return Ok(new
            {
                unreadCount
            });
        }

        [HttpPatch("{id:guid}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);

            return Ok(new
            {
                message = "Notification marked as read successfully"
            });
        }

        [HttpPatch("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync();

            return Ok(new
            {
                message = "All notifications marked as read successfully"
            });
        }
    }
}