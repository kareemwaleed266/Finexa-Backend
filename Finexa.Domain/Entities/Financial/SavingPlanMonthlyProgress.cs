using Finexa.Domain.Enums;

namespace Finexa.Domain.Entities.Financial
{
    public class SavingPlanMonthlyProgress : BaseAuditableEntity<Guid>
    {
        public Guid SavingPlanId { get; set; }
        public virtual SavingPlan SavingPlan { get; set; } = null!;

        public int Year { get; set; }

        public int Month { get; set; }

        public decimal RecommendedMonthlySaving { get; set; }

        public decimal ActualIncome { get; set; }

        public decimal ActualExpenses { get; set; }

        public decimal ActualSaving { get; set; }

        public decimal Difference { get; set; }

        public decimal ProgressPercentage { get; set; }

        public SavingPlanMonthlyStatus Status { get; set; }

        public string Summary { get; set; } = string.Empty;

        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }
}