using System.Text.Json.Serialization;

namespace Finexa.Integration.AI.SavingPlan.Models
{
    public class SavingPlanApiRequest
    {
        [JsonPropertyName("months")]
        public int Months { get; set; }

        [JsonPropertyName("planType")]
        public string PlanType { get; set; } = string.Empty;

        [JsonPropertyName("targetMonthlySaving")]
        public decimal? TargetMonthlySaving { get; set; }

        [JsonPropertyName("monthlySummary")]
        public List<MonthlySummaryApiModel> MonthlySummary { get; set; } = new();

        [JsonPropertyName("categorySummary")]
        public List<CategorySummaryApiModel> CategorySummary { get; set; } = new();
    }
}
