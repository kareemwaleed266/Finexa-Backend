using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Bills.DTOs
{
    public class BillOccurrenceDto
    {
        public Guid Id { get; set; }

        public Guid BillSeriesId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal? Amount { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public BillOccurrenceStatus Status { get; set; }

        public string DisplayStatus { get; set; } = string.Empty;

        public BillOccurrenceType OccurrenceType { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? Notes { get; set; }

        public bool CanRecordPayment { get; set; }

        public bool CanSkip { get; set; }

        public bool CanCancel { get; set; }

        public bool CanRenewEarly { get; set; }

        public bool CanTopUp { get; set; }
    }
}