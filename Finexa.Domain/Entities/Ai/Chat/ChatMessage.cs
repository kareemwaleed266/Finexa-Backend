using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Domain.Entities.Ai.Chat
{
    public class ChatMessage : BaseAuditableEntity<Guid>
    {
        public Guid SessionId { get; set; }
        public virtual ChatSession Session { get; set; }

        public ChatRole Role { get; set; }

        public string Content { get; set; } = default!;
    }
}
