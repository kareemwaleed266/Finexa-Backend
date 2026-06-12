using Finexa.Application.Common.DTOs;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminUserFilterDto : BaseFilterDto
    {
        public string? Search { get; set; }

        public string? Role { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsLocked { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedTo { get; set; }
    }
}