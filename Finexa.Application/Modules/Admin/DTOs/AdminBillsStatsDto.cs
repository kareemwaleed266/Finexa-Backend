namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminBillsStatsDto
    {
        public int TotalBillSeries { get; set; }

        public int ActiveBillSeries { get; set; }

        public int TotalOccurrences { get; set; }

        public int ScheduledOccurrences { get; set; }

        public int PaidOccurrences { get; set; }

        public int OverdueOccurrences { get; set; }

        public int DueThisWeek { get; set; }

        public int PaidThisMonth { get; set; }

        public decimal ExpectedThisMonth { get; set; }
    }
}