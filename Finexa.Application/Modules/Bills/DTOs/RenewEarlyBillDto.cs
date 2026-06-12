namespace Finexa.Application.Modules.Bills.DTOs
{
    public class RenewEarlyBillDto
    {
        public decimal? Amount { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? Notes { get; set; }
    }
}