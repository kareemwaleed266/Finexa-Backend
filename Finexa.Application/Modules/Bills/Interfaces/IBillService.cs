using Finexa.Application.Common.Models;
using Finexa.Application.Modules.Bills.DTOs;

namespace Finexa.Application.Modules.Bills.Interfaces
{
    public interface IBillService
    {
        Task<Guid> CreateBillSeriesAsync(CreateBillSeriesDto dto);

        Task UpdateBillSeriesAsync(Guid billSeriesId, UpdateBillSeriesDto dto);

        Task CancelBillSeriesAsync(Guid billSeriesId);

        Task<List<BillSeriesDto>> GetMyBillsAsync();
        Task<PagedResult<BillOccurrenceDto>> GetAllOccurrencesAsync(BillOccurrenceFilterDto filter);
        Task<BillSeriesDetailsDto> GetBillDetailsAsync(Guid billSeriesId);

        Task<List<BillOccurrenceDto>> GetBillOccurrencesAsync(Guid billSeriesId);

        Task<BillDashboardSummaryDto> GetDashboardSummaryAsync();

        Task<BillCalendarDto> GetCalendarAsync(int year, int month);

        Task<int> GenerateUpcomingOccurrencesForAllUsersAsync(int daysAhead = 90);

        Task<BillPaymentResultDto> RecordPaymentAsync(Guid occurrenceId, RecordBillPaymentDto dto);

        Task ReversePaymentAsync(Guid paymentId);

        Task SkipOccurrenceAsync(Guid occurrenceId, string? notes = null);

        Task CancelOccurrenceAsync(Guid occurrenceId, string? notes = null);

        Task<BillPaymentResultDto> RenewEarlyAsync(Guid billSeriesId, RenewEarlyBillDto dto);

        Task<BillPaymentResultDto> AddTopUpAsync(Guid billSeriesId, AddBillTopUpDto dto);
    }
}