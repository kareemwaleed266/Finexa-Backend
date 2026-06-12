using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Bills.DTOs
{
    public class BillSeriesDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal? DefaultAmount { get; set; }

        public BillAmountType AmountType { get; set; }

        public BillFrequency Frequency { get; set; }

        public int? DueDay { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public int ReminderDaysBefore { get; set; }

        public bool AllowsEarlyRenewal { get; set; }

        public bool AllowsTopUp { get; set; }

        public BillOccurrenceDto? CurrentOccurrence { get; set; }
    }
}