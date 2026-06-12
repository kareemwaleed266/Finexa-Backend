namespace Finexa.Application.Modules.Bills.DTOs
{
    public class BillPaymentResultDto
    {
        public Guid OccurrenceId { get; set; }

        public Guid PaymentId { get; set; }

        public Guid TransactionId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaidAt { get; set; }
    }
}