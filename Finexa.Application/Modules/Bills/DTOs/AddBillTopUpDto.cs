namespace Finexa.Application.Modules.Bills.DTOs
{
    public class AddBillTopUpDto
    {
        public decimal Amount { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? Notes { get; set; }
    }
}