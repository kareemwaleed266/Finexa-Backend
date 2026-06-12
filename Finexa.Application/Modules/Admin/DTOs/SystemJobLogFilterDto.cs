using Finexa.Application.Common.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class SystemJobLogFilterDto : BaseFilterDto
    {
        public SystemJobName? JobName { get; set; }

        public SystemJobStatus? Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}