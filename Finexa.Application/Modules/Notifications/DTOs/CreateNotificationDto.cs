using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Notifications.DTOs
{
    public class CreateNotificationDto
    {
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public NotificationSeverity Severity { get; set; }

        public bool ShouldToast { get; set; } = false;

        public string? RelatedEntityType { get; set; }

        public Guid? RelatedEntityId { get; set; }

        public string? ActionUrl { get; set; }

        public string? DeduplicationKey { get; set; }
    }
}