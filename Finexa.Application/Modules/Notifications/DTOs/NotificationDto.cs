using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Notifications.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public NotificationSeverity Severity { get; set; }

        public bool ShouldToast { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public string? RelatedEntityType { get; set; }

        public Guid? RelatedEntityId { get; set; }

        public string? ActionUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}