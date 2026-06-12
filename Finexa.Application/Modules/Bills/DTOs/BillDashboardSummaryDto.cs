namespace Finexa.Application.Modules.Bills.DTOs
{
    public class BillDashboardSummaryDto
    {
        public decimal ExpectedThisMonth { get; set; }

        public int BillsThisMonthCount { get; set; }

        public decimal DueThisWeekAmount { get; set; }

        public int DueThisWeekCount { get; set; }

        public decimal PaidThisMonthAmount { get; set; }

        public int PaidThisMonthCount { get; set; }

        public decimal PaidCompletionPercentage { get; set; }

        public decimal OverdueAmount { get; set; }

        public int OverdueCount { get; set; }

        public BillOccurrenceDto? NextBillDue { get; set; }

        public List<BillOccurrenceDto> UpcomingBills { get; set; } = new();

        public List<BillOccurrenceDto> OverdueBills { get; set; } = new();
    }
}