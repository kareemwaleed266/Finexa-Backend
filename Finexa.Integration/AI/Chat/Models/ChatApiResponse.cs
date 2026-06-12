using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Integration.AI.Chat.Models
{
    using System.Text.Json.Serialization;
    using Finexa.Application.Modules.AI.ParseTransaction.DTOs;

    public class ChatApiResponse
    {
        [JsonPropertyName("message")]
        public string Reply { get; set; } = default!;

        [JsonPropertyName("summary")]

        public string? Summary { get; set; }

        [JsonPropertyName("summary_updated")]
        public bool Summary_Updated { get; set; }

        [JsonPropertyName("transactions")]

        public List<ParsedTransactionItemDto>? Transactions { get; set; }

    }
}
