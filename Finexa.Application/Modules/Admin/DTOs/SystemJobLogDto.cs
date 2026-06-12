using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class SystemJobLogDto
    {
        public Guid Id { get; set; }

        public SystemJobName JobName { get; set; }

        public SystemJobStatus Status { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public long? DurationMs { get; set; }

        public int ProcessedCount { get; set; }

        public int CreatedCount { get; set; }

        public int UpdatedCount { get; set; }

        public int FailedCount { get; set; }

        public string? ErrorMessage { get; set; }

        public string? TriggeredBy { get; set; }

        public string? MetadataJson { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}