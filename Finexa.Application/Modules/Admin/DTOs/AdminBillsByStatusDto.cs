using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminBillsByStatusDto
    {
        public BillOccurrenceStatus Status { get; set; }

        public int Count { get; set; }

        public decimal TotalAmount { get; set; }
    }
}