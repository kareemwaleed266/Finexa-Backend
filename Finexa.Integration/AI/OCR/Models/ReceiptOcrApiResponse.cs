using System.Text.Json.Serialization;

namespace Finexa.Integration.AI.OCR.Models
{
    public class ReceiptOcrApiResponse
    {
        [JsonPropertyName("total")]
        public decimal Amount { get; set; }

        [JsonPropertyName("merchant")]
        public string? Merchant { get; set; }

        [JsonPropertyName("date")]
        public string? OccurredAt { get; set; }

        [JsonPropertyName("categoryName")]
        public string? CategoryName { get; set; }

        [JsonPropertyName("item")]
        public string? Item { get; set; }





        //[JsonPropertyName("currency")]
        //public string? Currency { get; set; }


        //[JsonPropertyName("category")]
        //public string? Category { get; set; }


        //[JsonPropertyName("transaction_type")]
        //public string? TransactionType { get; set; }

        //[JsonPropertyName("type")]
        //public string? Type { get; set; }

        //[JsonPropertyName("notes")]
        //public string? Notes { get; set; }

        //[JsonPropertyName("date")]
        //public DateTime? Date { get; set; }

    }
}