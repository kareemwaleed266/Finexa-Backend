using Finexa.Application.Common.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Notifications.DTOs
{
    public class NotificationFilterDto : BaseFilterDto
    {
        public bool? IsRead { get; set; }

        public NotificationType? Type { get; set; }

        public NotificationSeverity? Severity { get; set; }
    }
}