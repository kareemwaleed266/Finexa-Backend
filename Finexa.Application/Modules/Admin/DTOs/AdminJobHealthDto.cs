using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminJobHealthDto
    {
        public Guid? LatestJobId { get; set; }

        public SystemJobName? LatestJobName { get; set; }

        public SystemJobStatus? LatestJobStatus { get; set; }

        public DateTime? LatestJobStartedAt { get; set; }

        public DateTime? LatestJobFinishedAt { get; set; }

        public int FailedJobsLast24Hours { get; set; }
    }
}