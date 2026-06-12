using Finexa.Domain.Enums;

namespace Finexa.Domain.Entities.Financial
{
    public class SavingPlanItem : BaseAuditableEntity<Guid>
    {
        public Guid SavingPlanId { get; set; }
        public virtual SavingPlan SavingPlan { get; set; } = null!;

        public Guid? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public SavingPlanCategoryType CategoryType { get; set; }

        public decimal CurrentAverage { get; set; }

        public decimal RecommendedBudget { get; set; }

        public decimal ReductionPercentage { get; set; }

        public decimal ExpectedSaving { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}