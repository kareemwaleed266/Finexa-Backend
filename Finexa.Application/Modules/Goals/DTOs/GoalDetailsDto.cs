namespace Finexa.Application.Modules.Goals.DTOs
{
    public class GoalDetailsDto
    {
        public Guid GoalId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public double ProgressPercentage { get; set; }

        public decimal MonthlyAmount { get; set; }

        public DateTime TargetDate { get; set; }

        public List<GoalHistoryDto> History { get; set; } = new();
    }
}