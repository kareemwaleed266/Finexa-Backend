using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.SavingPlans.DTOs
{
    public class CategorySummaryDto
    {
        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public SavingPlanCategoryType CategoryType { get; set; }

        public decimal AverageMonthlyAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PercentageOfExpenses { get; set; }

        public string Trend { get; set; } = "Stable";
    }
}