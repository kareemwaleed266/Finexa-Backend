using System.Text.Json.Serialization;

namespace Finexa.Integration.AI.SavingPlan.Models
{
    public class SavingPlanRecommendationApiModel
    {
        [JsonPropertyName("categoryId")]
        public string? CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("categoryType")]
        public string CategoryType { get; set; } = string.Empty;

        [JsonPropertyName("currentAverage")]
        public decimal CurrentAverage { get; set; }

        [JsonPropertyName("recommendedBudget")]
        public decimal RecommendedBudget { get; set; }

        [JsonPropertyName("reductionPercentage")]
        public decimal ReductionPercentage { get; set; }

        [JsonPropertyName("expectedSaving")]
        public decimal ExpectedSaving { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
