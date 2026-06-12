using Finexa.Application.Common.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminCategoryFilterDto : BaseFilterDto
    {
        public string? Search { get; set; }

        public TransactionType? Type { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsDefault { get; set; }
    }
}