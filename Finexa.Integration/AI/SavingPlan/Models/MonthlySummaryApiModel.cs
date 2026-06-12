using System.Text.Json.Serialization;

namespace Finexa.Integration.AI.SavingPlan.Models
{
    public class MonthlySummaryApiModel
    {
        [JsonPropertyName("month")]
        public string Month { get; set; } = string.Empty;

        [JsonPropertyName("income")]
        public decimal Income { get; set; }

        [JsonPropertyName("expenses")]
        public decimal Expenses { get; set; }

        [JsonPropertyName("saving")]
        public decimal Saving { get; set; }
    }
}
