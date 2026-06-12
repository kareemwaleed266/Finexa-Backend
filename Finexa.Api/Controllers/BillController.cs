using Finexa.Application.Modules.Bills.DTOs;
using Finexa.Application.Modules.Bills.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "User")]
    public class BillController : ControllerBase
    {
        private readonly IBillService _billService;

        public BillController(IBillService billService)
        {
            _billService = billService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBill([FromBody] CreateBillSeriesDto dto)
        {
            var billId = await _billService.CreateBillSeriesAsync(dto);

            return Ok(new
            {
                message = "Bill created successfully",
                billId
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyBills()
        {
            var bills = await _billService.GetMyBillsAsync();

            return Ok(bills);
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _billService.GetDashboardSummaryAsync();

            return Ok(summary);
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendar(
            [FromQuery] int year,
            [FromQuery] int month)
        {
            var calendar = await _billService.GetCalendarAsync(year, month);

            return Ok(calendar);
        }

        [HttpGet("occurrences")]
        public async Task<IActionResult> GetAllOccurrences(
            [FromQuery] BillOccurrenceFilterDto filter)
        {
            var occurrences = await _billService.GetAllOccurrencesAsync(filter);

            return Ok(occurrences);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBillDetails(Guid id)
        {
            var bill = await _billService.GetBillDetailsAsync(id);

            return Ok(bill);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBill(
            Guid id,
            [FromBody] UpdateBillSeriesDto dto)
        {
            await _billService.UpdateBillSeriesAsync(id, dto);

            return Ok(new
            {
                message = "Bill updated successfully"
            });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> CancelBill(Guid id)
        {
            await _billService.CancelBillSeriesAsync(id);

            return Ok(new
            {
                message = "Bill cancelled successfully"
            });
        }

        [HttpGet("{id:guid}/occurrences")]
        public async Task<IActionResult> GetBillOccurrences(Guid id)
        {
            var occurrences = await _billService.GetBillOccurrencesAsync(id);

            return Ok(occurrences);
        }

        [HttpPost("occurrences/{occurrenceId:guid}/record-payment")]
        public async Task<IActionResult> RecordPayment(
            Guid occurrenceId,
            [FromBody] RecordBillPaymentDto dto)
        {
            var result = await _billService.RecordPaymentAsync(occurrenceId, dto);

            return Ok(new
            {
                message = "Bill payment recorded successfully",
                result
            });
        }

        [HttpPost("payments/{paymentId:guid}/reverse")]
        public async Task<IActionResult> ReversePayment(Guid paymentId)
        {
            await _billService.ReversePaymentAsync(paymentId);

            return Ok(new
            {
                message = "Bill payment reversed successfully"
            });
        }

        [HttpPost("occurrences/{occurrenceId:guid}/skip")]
        public async Task<IActionResult> SkipOccurrence(
            Guid occurrenceId,
            [FromQuery] string? notes = null)
        {
            await _billService.SkipOccurrenceAsync(occurrenceId, notes);

            return Ok(new
            {
                message = "Bill occurrence skipped successfully"
            });
        }

        [HttpPost("occurrences/{occurrenceId:guid}/cancel")]
        public async Task<IActionResult> CancelOccurrence(
            Guid occurrenceId,
            [FromQuery] string? notes = null)
        {
            await _billService.CancelOccurrenceAsync(occurrenceId, notes);

            return Ok(new
            {
                message = "Bill occurrence cancelled successfully"
            });
        }

        [HttpPost("{billSeriesId:guid}/renew-early")]
        public async Task<IActionResult> RenewEarly(
            Guid billSeriesId,
            [FromBody] RenewEarlyBillDto dto)
        {
            var result = await _billService.RenewEarlyAsync(billSeriesId, dto);

            return Ok(new
            {
                message = "Early renewal recorded successfully",
                result
            });
        }

        [HttpPost("{billSeriesId:guid}/top-up")]
        public async Task<IActionResult> AddTopUp(
            Guid billSeriesId,
            [FromBody] AddBillTopUpDto dto)
        {
            var result = await _billService.AddTopUpAsync(billSeriesId, dto);

            return Ok(new
            {
                message = "Top-up recorded successfully",
                result
            });
        }
    }
}   