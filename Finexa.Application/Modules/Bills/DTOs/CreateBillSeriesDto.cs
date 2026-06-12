using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Bills.DTOs
{
    public class CreateBillSeriesDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid CategoryId { get; set; }

        public decimal? DefaultAmount { get; set; }

        public BillAmountType AmountType { get; set; }

        public BillFrequency Frequency { get; set; }

        public int? DueDay { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int ReminderDaysBefore { get; set; } = 3;

        public bool AllowsEarlyRenewal { get; set; }

        public bool AllowsTopUp { get; set; }
    }
}