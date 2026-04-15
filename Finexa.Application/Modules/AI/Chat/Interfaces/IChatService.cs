using Finexa.Application.Modules.AI.Chat.DTOs;

namespace Finexa.Application.Modules.AI.Chat.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request);
    }
}