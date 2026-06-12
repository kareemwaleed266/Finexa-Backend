namespace Finexa.Application.Modules.Bills.DTOs
{
    public class RecordBillPaymentDto
    {
        public decimal? Amount { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? Notes { get; set; }
    }
}