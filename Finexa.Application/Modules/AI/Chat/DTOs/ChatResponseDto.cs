using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.AI.Chat.DTOs
{
    public class ChatResponseDto
    {
        public string Reply { get; set; } = default!;

        public string? Summary { get; set; }

        public bool SummaryUpdated { get; set; }

        public string? Intent { get; set; }

        public string? ToolCalled { get; set; }
    }
}
