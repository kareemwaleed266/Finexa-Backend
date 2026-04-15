using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.AI.Chat.DTOs
{
    public class ChatRequestDto
    {
        public string Message { get; set; } = default!;

        public string? Summary { get; set; }

        public List<ChatMessageDto> History { get; set; } = new();

        public bool GenerateSummary { get; set; }
    }
}
