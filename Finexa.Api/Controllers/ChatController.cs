using Finexa.Application.Modules.AI.Chat.DTOs;
using Finexa.Application.Modules.AI.Chat.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class ChatController : ControllerBase
    {
        private readonly IChatAppService _chatAppService;

        public ChatController(IChatAppService chatAppService)
        {
            _chatAppService = chatAppService;
        }

        [HttpPost("create-session")]
        public async Task<ActionResult<ChatSessionDto>> CreateSession([FromBody] string? title)
        {
            var result = await _chatAppService.CreateSessionAsync(title);

            return Ok(result);
        }

        [HttpGet("get-sessions")]
        public async Task<ActionResult<IReadOnlyList<ChatSessionDto>>> GetSessions()
        {
            var result = await _chatAppService.GetMySessionsAsync();

            return Ok(result);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<ActionResult<ChatDetailsDto>> GetSessionDetails(Guid sessionId)
        {
            var result = await _chatAppService.GetSessionDetailsAsync(sessionId);

            return Ok(result);
        }

        [HttpPost("send")]
        public async Task<ActionResult<SendMessageResponseDto>> SendMessage(
            [FromBody] SendMessageDto dto)
        {
            var result = await _chatAppService.SendMessageAsync(dto);

            return Ok(result);
        }
    }
}