using Finexa.Application.Modules.AI.Chat.DTOs;

namespace Finexa.Application.Modules.AI.Chat.Interfaces
{
    public interface IChatAppService
    {
        Task<ChatSessionDto> CreateSessionAsync(string? title = null);

        Task<IReadOnlyList<ChatSessionDto>> GetMySessionsAsync();

        Task<ChatDetailsDto> GetSessionDetailsAsync(Guid sessionId);

        Task<SendMessageResponseDto> SendMessageAsync(SendMessageDto dto);
    }
}