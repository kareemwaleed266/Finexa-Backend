using System.Text.Json.Serialization;

namespace Finexa.Integration.AI.SavingPlan.Models
{
    public class SavingPlanApiResponse
    {
        [JsonPropertyName("analysisPeriodMonths")]
        public int AnalysisPeriodMonths { get; set; }

        [JsonPropertyName("averageIncome")]
        public decimal AverageIncome { get; set; }

        [JsonPropertyName("averageExpenses")]
        public decimal AverageExpenses { get; set; }

        [JsonPropertyName("currentAverageSaving")]
        public decimal CurrentAverageSaving { get; set; }

        [JsonPropertyName("forecastedIncome")]
        public decimal ForecastedIncome { get; set; }

        [JsonPropertyName("forecastedExpenses")]
        public decimal ForecastedExpenses { get; set; }

        [JsonPropertyName("forecastedSaving")]
        public decimal ForecastedSaving { get; set; }

        [JsonPropertyName("recommendedMonthlySaving")]
        public decimal RecommendedMonthlySaving { get; set; }

        [JsonPropertyName("extraSavingOpportunity")]
        public decimal ExtraSavingOpportunity { get; set; }

        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; } = "Medium";

        // AI returns planStatus, but the application DTO uses PlanStatusLabel.
        [JsonPropertyName("planStatus")]
        public string PlanStatusLabel { get; set; } = string.Empty;

        [JsonPropertyName("summaryMessage")]
        public string SummaryMessage { get; set; } = string.Empty;

        // AI returns recommendations, but the application DTO/entity uses Items.
        [JsonPropertyName("recommendations")]
        public List<SavingPlanRecommendationApiModel> Items { get; set; } = new();

        [JsonPropertyName("insights")]
        public List<string> Insights { get; set; } = new();

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new();
    }
}
