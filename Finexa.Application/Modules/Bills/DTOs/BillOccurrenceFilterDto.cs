using Finexa.Application.Common.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Bills.DTOs
{
    public class BillOccurrenceFilterDto : BaseFilterDto
    {
        public string? Search { get; set; }

        public BillOccurrenceStatus? Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}