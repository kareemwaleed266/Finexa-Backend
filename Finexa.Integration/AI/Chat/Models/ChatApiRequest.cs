using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Integration.AI.Chat.Models
{
    public class ChatApiRequest
    {
        public string Message { get; set; } = default!;

        public string? Summary { get; set; }

        public List<ChatApiMessage> History { get; set; } = new();

        public bool GenerateSummary { get; set; }
    }
}
