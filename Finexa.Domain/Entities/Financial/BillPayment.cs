namespace Finexa.Domain.Entities.Financial
{
    public class BillPayment : BaseAuditableEntity<Guid>
    {
        public Guid BillSeriesId { get; set; }
        public virtual BillSeries BillSeries { get; set; } = null!;

        public Guid BillOccurrenceId { get; set; }
        public virtual BillOccurrence BillOccurrence { get; set; } = null!;

        public Guid AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; } = null!;

        public Guid TransactionId { get; set; }
        public virtual Transaction Transaction { get; set; } = null!;

        public decimal AmountPaid { get; set; }

        public DateTime PaidAt { get; set; }

        public BillPaymentStatus Status { get; set; } = BillPaymentStatus.Completed;

        public Guid? ReversalTransactionId { get; set; }
        public virtual Transaction? ReversalTransaction { get; set; }

        public DateTime? ReversedAt { get; set; }

        public string? Notes { get; set; }
    }
}