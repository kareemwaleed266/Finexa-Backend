using Finexa.Application.Common.Files;
using Finexa.Application.Common.Models;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.Dashboard.DTOs;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Transactions.Interfaces
{
    public interface ITransactionService
    {
        Task<Guid> AddTransactionAsync(CreateTransactionDto dto, TransactionSource source, Guid? GoalId);
        Task AddTransactionAttachmentAsync(Guid transactionId,FileUploadResultDto file,AttachmentType type);
        Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionFilterDto filter);
        Task<TransactionDto> GetTransactionByIdAsync(Guid id);
        Task UpdateTransactionAsync(Guid transactionId, UpdateTransactionDto dto);

        Task DeleteTransactionAsync(Guid transactionId);
        Task AdjustBalanceAsync(AdjustBalanceDto dto);
        Task<CreateTransactionDto> ConfirmTransactionsAsync(ConfirmParsedTransactionDto dto);
        Task<int> AddParsedTransactionsAsync(List<ParsedTransactionItemDto> transactions,TransactionSource source);
    }
}