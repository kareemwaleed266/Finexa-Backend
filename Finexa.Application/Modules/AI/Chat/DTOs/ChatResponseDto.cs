using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;

namespace Finexa.Application.Modules.AI.Chat.DTOs
{
    public class ChatResponseDto
    {
        public string Reply { get; set; } = default!;

        public string? Summary { get; set; }

        public bool SummaryUpdated { get; set; }
        public List<ParsedTransactionItemDto> Transactions { get; set; } = new();

        public string? Intent { get; set; }

        public string? ToolCalled { get; set; }
    }
}
