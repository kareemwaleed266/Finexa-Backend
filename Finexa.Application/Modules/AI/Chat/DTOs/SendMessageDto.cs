using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.AI.Chat.DTOs
{
    public class SendMessageDto
    {
        public Guid SessionId { get; set; }

        public string Message { get; set; } = default!;
    }
}
