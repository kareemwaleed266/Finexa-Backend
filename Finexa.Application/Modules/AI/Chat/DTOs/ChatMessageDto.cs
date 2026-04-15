using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.AI.Chat.DTOs
{
    public class ChatMessageDto
    {
        public string Role { get; set; } = default!;

        public string Content { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
    }
}
