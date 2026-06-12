namespace Finexa.Application.Modules.SavingPlans.DTOs
{
    public class SavingPlanCategoryProgressDto
    {
        public Guid? CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal RecommendedBudget { get; set; }

        public decimal ActualSpent { get; set; }

        public decimal Difference { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}