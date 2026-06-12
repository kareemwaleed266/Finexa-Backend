using System.Text.Json.Serialization;

namespace Finexa.Integration.AI.SavingPlan.Models
{
    public class CategorySummaryApiModel
    {
        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; } = string.Empty;

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("categoryType")]
        public string CategoryType { get; set; } = string.Empty;

        [JsonPropertyName("averageMonthlyAmount")]
        public decimal AverageMonthlyAmount { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("percentageOfExpenses")]
        public decimal PercentageOfExpenses { get; set; }

        [JsonPropertyName("trend")]
        public string Trend { get; set; } = "Stable";
    }
}
