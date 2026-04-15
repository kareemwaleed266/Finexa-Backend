using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Domain.Entities.Ai.Chat
{
    public class ChatSession : BaseAuditableEntity<Guid>
    {
        public Guid AppUserId { get; set; }

        public string? Title { get; set; }

        public string? Summary { get; set; }

        public DateTime LastActivityAt { get; set; }

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
