using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.Transactions.DTOs;

namespace Finexa.Application.Modules.AI.ParseTransaction.Interfaces
{
    public interface IAiTransactionMapperService
    {
        Task<CreateTransactionDto> MapAsync(ParsedTransactionItemDto parsed);
    }
}
