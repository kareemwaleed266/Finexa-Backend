using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Bills.DTOs
{
    public class BillPaymentDto
    {
        public Guid Id { get; set; }

        public Guid BillSeriesId { get; set; }

        public Guid BillOccurrenceId { get; set; }

        public Guid TransactionId { get; set; }

        public decimal AmountPaid { get; set; }

        public DateTime PaidAt { get; set; }

        public BillPaymentStatus Status { get; set; }

        public Guid? ReversalTransactionId { get; set; }

        public DateTime? ReversedAt { get; set; }

        public string? Notes { get; set; }
    }
}