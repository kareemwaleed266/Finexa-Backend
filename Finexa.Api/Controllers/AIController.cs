using Finexa.Application.Modules.AI.OCR.Interfaces;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.AI.ParseTransaction.Interfaces;
using Finexa.Application.Modules.AI.STT.Interfaces;
using Finexa.Application.Modules.AI.STT.Services;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Application.Modules.Transactions.Interfaces;
using Finexa.Application.Modules.Transactions.Services;
using Finexa.Domain.Enums;
using Finexa.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
       private readonly ISpeechAppService _speechAppService;
       private readonly IParseTransactionAppService _parseAppService;
       private readonly ITransactionService _transactionService;
       private readonly IUnitOfWork _unitOfWork;
        public AIController(
            ISpeechAppService speechAppService,
            IParseTransactionAppService parseAppService,
            ITransactionService transactionService,IUnitOfWork unitOfWork)
        {
            _speechAppService = speechAppService;
            _parseAppService = parseAppService;
            _transactionService = transactionService;
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "User")]

        [HttpPost("voice-to-text")]
        public async Task<IActionResult> VoiceToText(IFormFile file)
        {
            var text = await _speechAppService.ConvertSpeechAsync(file);

            return Ok(new { text });
        }



        [HttpPost("parse-transaction")]
        public async Task<IActionResult> ParseTransactionAsync([FromBody] ParseRequestDto request)
        {
            var parsed = await _parseAppService.ParseAsync(request.Text);
            
            return Ok(parsed);
        }
    }
}