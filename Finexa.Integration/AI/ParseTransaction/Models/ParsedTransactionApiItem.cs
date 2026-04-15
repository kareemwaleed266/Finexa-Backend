using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Finexa.Integration.AI.ParseTransaction.Models
{
    public class ParsedTransactionApiItem
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("merchant")]
        public string? Merchant { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("transaction_type")]
        public string? Type { get; set; } // "income" | "expense"

        //[JsonPropertyName("notes")]
        //public string? Notes { get; set; }

        [JsonPropertyName("date")]
        public DateTime? OccurredAt { get; set; }
    }
}
