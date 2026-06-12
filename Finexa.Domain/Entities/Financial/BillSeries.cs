namespace Finexa.Domain.Entities.Financial
{
    public class BillSeries : BaseAuditableEntity<Guid>
    {
        public Guid AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public decimal? DefaultAmount { get; set; }

        public BillAmountType AmountType { get; set; }

        public BillFrequency Frequency { get; set; }

        public int? DueDay { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public int ReminderDaysBefore { get; set; } = 3;

        public bool AllowsEarlyRenewal { get; set; }

        public bool AllowsTopUp { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        //public bool AllowsExtraPayment { get; set; }

        public virtual ICollection<BillOccurrence> Occurrences { get; set; }
            = new List<BillOccurrence>();
    }
}