using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Integration.AI.Chat.Models
{
    public class ChatApiMessage
    {
        public string Role { get; set; } = default!;

        public string Content { get; set; } = default!;

        public string Created_At { get; set; } = default!;
    }
}
