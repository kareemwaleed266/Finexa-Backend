using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Goals.DTOs
{
    public class CreateGoalDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public decimal TargetAmount { get; set; }

        public int? DurationValue { get; set; } 

        public GoalDurationUnit? DurationUnit { get; set; } // Months / Years



        public decimal? MonthlyAmount { get; set; }
    }
}