using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Integration.AI.Chat.Models
{
    using System.Text.Json.Serialization;

    public class ChatApiResponse
    {
        [JsonPropertyName("message")]
        public string Reply { get; set; } = default!;

        public string? Summary { get; set; }

        [JsonPropertyName("summary_updated")]
        public bool Summary_Updated { get; set; }
    }
}
