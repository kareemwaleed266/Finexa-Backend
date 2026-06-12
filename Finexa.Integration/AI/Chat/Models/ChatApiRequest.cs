using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Finexa.Integration.AI.Chat.Models
{
    public class ChatApiRequest
    {
        [JsonPropertyName("message")]

        public string Message { get; set; } = default!;

        [JsonPropertyName("summary")]

        public string? Summary { get; set; }
        [JsonPropertyName("history")]

        public List<ChatApiMessage> History { get; set; } = new();
        [JsonPropertyName("generate_summary")]

        public bool GenerateSummary { get; set; }
    }
}
