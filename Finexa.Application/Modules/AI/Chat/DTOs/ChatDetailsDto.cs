using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.AI.Chat.DTOs
{
    public class ChatDetailsDto
    {
        public Guid SessionId { get; set; }

        public string? Title { get; set; }

        public List<ChatMessageDto> Messages { get; set; } = new();
    }
}
