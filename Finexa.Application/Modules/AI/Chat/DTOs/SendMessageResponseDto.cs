using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;

namespace Finexa.Application.Modules.AI.Chat.DTOs
{
    public class SendMessageResponseDto
    {
        public Guid SessionId { get; set; }

        public string Reply { get; set; } = default!;
        public List<ParsedTransactionItemDto> Transactions { get; set; } = new();

    }
}
