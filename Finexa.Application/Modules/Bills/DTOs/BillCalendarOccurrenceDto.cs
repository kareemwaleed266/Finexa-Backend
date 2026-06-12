using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Bills.DTOs
{
    public class BillCalendarOccurrenceDto
    {
        public Guid Id { get; set; }

        public Guid BillSeriesId { get; set; }

        public string BillName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public decimal? Amount { get; set; }

        public DateTime DueDate { get; set; }

        public BillOccurrenceStatus Status { get; set; }

        public string DisplayStatus { get; set; } = string.Empty;

        public BillOccurrenceType OccurrenceType { get; set; }
    }
}