namespace Finexa.Application.Modules.Bills.DTOs
{
    public class BillSeriesDetailsDto : BillSeriesDto
    {
        public List<BillOccurrenceDto> Occurrences { get; set; } = new();

        public List<BillPaymentDto> Payments { get; set; } = new();
    }
}