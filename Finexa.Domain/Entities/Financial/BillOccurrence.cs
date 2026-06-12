namespace Finexa.Domain.Entities.Financial
{
    public class BillOccurrence : BaseAuditableEntity<Guid>
    {
        public Guid BillSeriesId { get; set; }
        public virtual BillSeries BillSeries { get; set; } = null!;

        public Guid AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public DateTime DueDate { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public BillOccurrenceStatus Status { get; set; } = BillOccurrenceStatus.Scheduled;

        public BillOccurrenceType OccurrenceType { get; set; } = BillOccurrenceType.Scheduled;

        public bool IsGeneratedAutomatically { get; set; } = true;

        public DateTime? PaidAt { get; set; }

        public string? Notes { get; set; }

        public virtual ICollection<BillPayment> Payments { get; set; }
            = new List<BillPayment>();
    }
}