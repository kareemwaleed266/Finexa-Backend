using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminRecentAuditLogDto
    {
        public Guid Id { get; set; }

        public string? AdminEmail { get; set; }

        public AdminAuditAction Action { get; set; }

        public AdminTargetType TargetType { get; set; }

        public string? TargetDisplayName { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}