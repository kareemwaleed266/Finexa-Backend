using Finexa.Domain.Enums;

namespace Finexa.Domain.Entities.Financial
{
    public class SavingPlan : BaseAuditableEntity<Guid>
    {
        public Guid AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; } = null!;

        public int AnalysisPeriodMonths { get; set; }

        public SavingPlanType PlanType { get; set; }

        public decimal? TargetMonthlySaving { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal AverageIncome { get; set; }

        public decimal AverageExpenses { get; set; }

        public decimal CurrentAverageSaving { get; set; }

        public decimal ForecastedIncome { get; set; }

        public decimal ForecastedExpenses { get; set; }

        public decimal ForecastedSaving { get; set; }

        public decimal RecommendedMonthlySaving { get; set; }

        public decimal ExtraSavingOpportunity { get; set; }

        public SavingPlanDifficulty Difficulty { get; set; }

        public SavingPlanStatus Status { get; private set; } = SavingPlanStatus.Draft;

        public string PlanStatusLabel { get; set; } = string.Empty;

        public string SummaryMessage { get; set; } = string.Empty;
        public string? InsightsJson { get; set; }

        public string? WarningsJson { get; set; }
        public DateTime? AppliedAt { get; private set; } 

        public DateTime? DeactivatedAt { get; private set; }

        public virtual ICollection<SavingPlanItem> Items { get; set; }
            = new List<SavingPlanItem>();

        public virtual ICollection<SavingPlanMonthlyProgress> MonthlyProgress { get; set; }
            = new List<SavingPlanMonthlyProgress>();

        public bool IsDraft()
            => Status == SavingPlanStatus.Draft;

        public bool IsActive()
            => Status == SavingPlanStatus.Active;

        public void Activate()
        {
            if (Status == SavingPlanStatus.Active)
                return;

            Status = SavingPlanStatus.Active;
            AppliedAt = DateTime.UtcNow;
            DeactivatedAt = null;
        }

        public void Deactivate()
        {
            if (Status == SavingPlanStatus.Deactivated)
                return;

            Status = SavingPlanStatus.Deactivated;
            DeactivatedAt = DateTime.UtcNow;
            EndDate ??= DateTime.UtcNow;
        }

    }
}