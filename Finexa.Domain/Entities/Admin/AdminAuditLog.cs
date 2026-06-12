using Finexa.Domain.Entities.Identity;
using Finexa.Domain.Enums;

namespace Finexa.Domain.Entities.Admin
{
    public class AdminAuditLog : BaseAuditableEntity<Guid>
    {
        public Guid? AdminUserId { get; set; }

        public string? AdminEmail { get; set; }

        public AdminAuditAction Action { get; set; }

        public AdminTargetType TargetType { get; set; }

        public Guid? TargetId { get; set; }

        public string? TargetDisplayName { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public string? OldValuesJson { get; set; }

        public string? NewValuesJson { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public virtual AppUser? AdminUser { get; set; }
    }
}