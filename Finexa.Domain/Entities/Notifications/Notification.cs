using Finexa.Domain.Entities.Identity;
using Finexa.Domain.Enums;

namespace Finexa.Domain.Entities.Notifications
{
    public class Notification : BaseAuditableEntity<Guid>
    {
        public Guid AppUserId { get; set; }

        public virtual AppUser AppUser { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public NotificationSeverity Severity { get; set; }

        public bool ShouldToast { get; set; } = false;

        public bool IsRead { get; private set; } = false;

        public DateTime? ReadAt { get; private set; }

        public string? RelatedEntityType { get; set; }

        public Guid? RelatedEntityId { get; set; }

        public string? ActionUrl { get; set; }

        public string? DeduplicationKey { get; set; }

        public void MarkAsRead()
        {
            if (IsRead)
                return;

            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}