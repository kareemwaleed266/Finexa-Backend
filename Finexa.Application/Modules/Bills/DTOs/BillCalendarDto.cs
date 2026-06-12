namespace Finexa.Application.Modules.Bills.DTOs
{
    public class BillCalendarDto
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public List<BillCalendarOccurrenceDto> Occurrences { get; set; } = new();
    }
}