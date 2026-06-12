using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.SavingPlans.DTOs
{
    public class SavingPlanResponseDto
    {
        public Guid? Id { get; set; }

        public int AnalysisPeriodMonths { get; set; }

        public SavingPlanType PlanType { get; set; }

        public decimal? TargetMonthlySaving { get; set; }

        //public string Currency { get; set; } = "EGP";

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        public decimal AverageIncome { get; set; }

        public decimal AverageExpenses { get; set; }

        public decimal CurrentAverageSaving { get; set; }

        public decimal ForecastedIncome { get; set; }

        public decimal ForecastedExpenses { get; set; }

        public decimal ForecastedSaving { get; set; }

        public decimal RecommendedMonthlySaving { get; set; }

        public decimal ExtraSavingOpportunity { get; set; }

        public SavingPlanDifficulty Difficulty { get; set; }

        public string PlanStatusLabel { get; set; } = string.Empty;

        public SavingPlanStatus? Status { get; set; }

        public string SummaryMessage { get; set; } = string.Empty;

        public DateTime? AppliedAt { get; set; }

        public List<SavingPlanItemDto> Items { get; set; } = new();

        public List<string> Insights { get; set; } = new();

        public List<string> Warnings { get; set; } = new();
    }
}