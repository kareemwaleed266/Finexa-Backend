using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.SavingPlans.DTOs
{
    public class SavingPlanItemDto
    {
        public Guid? CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public SavingPlanCategoryType CategoryType { get; set; }

        public decimal CurrentAverage { get; set; }

        public decimal RecommendedBudget { get; set; }

        public decimal ReductionPercentage { get; set; }

        public decimal ExpectedSaving { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}