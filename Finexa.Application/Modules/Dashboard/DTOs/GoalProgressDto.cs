using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Dashboard.DTOs
{
    public class GoalProgressDto
    {
        public Guid GoalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public double ProgressPercentage { get; set; }
        public GoalStatus Status { get; set; }
    }
}