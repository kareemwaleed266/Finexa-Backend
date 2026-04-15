using System.Text.Json.Serialization;

namespace Finexa.Integration.AI.ParseTransaction.Models
{
    public class ParseTransactionApiResponse
    {
        [JsonPropertyName("transactions")]
        public List<ParsedTransactionApiItem> Transactions { get; set; } = new();
    }
}