using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.Dashboard.DTOs;
using Finexa.Application.Modules.Transactions.DTOs;

namespace Finexa.Application.Modules.Transactions.Interfaces
{
    public interface ITransactionService
    {
        Task AddTransactionAsync(CreateTransactionDto dto, TransactionSource source);

        Task<List<TransactionDto>> GetTransactionsAsync(TransactionFilterDto filter);
        Task<TransactionDto> GetTransactionByIdAsync(Guid id);
        Task UpdateTransactionAsync(Guid transactionId, UpdateTransactionDto dto);

        Task DeleteTransactionAsync(Guid transactionId);
        Task AdjustBalanceAsync(AdjustBalanceDto dto);
        Task<List<CreateTransactionDto>> ConfirmTransactionsAsync(
        List<ConfirmParsedTransactionDto> dtos);
    }
}