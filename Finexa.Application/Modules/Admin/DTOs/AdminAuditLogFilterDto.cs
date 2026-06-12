using Finexa.Application.Common.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminAuditLogFilterDto : BaseFilterDto
    {
        public Guid? AdminUserId { get; set; }

        public AdminAuditAction? Action { get; set; }

        public AdminTargetType? TargetType { get; set; }

        public Guid? TargetId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}