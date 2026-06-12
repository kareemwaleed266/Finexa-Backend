using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminSystemHealthDto
    {
        public bool DatabaseAvailable { get; set; }

        public DateTime CheckedAt { get; set; }

        public int TotalUsers { get; set; }

        public int TotalTransactions { get; set; }

        public int TotalBillSeries { get; set; }

        public int TotalBillOccurrences { get; set; }

        public int FailedJobsLast24Hours { get; set; }

        public SystemJobStatus? LatestJobStatus { get; set; }

        public DateTime? LatestJobStartedAt { get; set; }

        public DateTime? LatestJobFinishedAt { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}