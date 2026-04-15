using System.Net.Http.Json;
using Finexa.Application.Modules.AI.Chat.DTOs;
using Finexa.Application.Modules.AI.Chat.Interfaces;
using Finexa.Integration.AI.Chat.Models;

namespace Finexa.Integration.AI.Chat
{
 
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;

        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request)
        {
            var apiRequest = new ChatApiRequest
            {
                Message = request.Message,
                Summary = request.Summary,
                GenerateSummary = request.GenerateSummary,
                History = request.History.Select(x => new ChatApiMessage
                {
                    Role = x.Role,
                    Content = x.Content,
                    Created_At = x.CreatedAt.ToString("o")
                }).ToList()
            };

            // Call API
            var response = await _httpClient.PostAsJsonAsync("/api/chat", apiRequest);

            if (!response.IsSuccessStatusCode)
                throw new Exception("AI Service failed");

            var apiResponse = await response.Content.ReadFromJsonAsync<ChatApiResponse>();

            if (apiResponse == null)
                throw new Exception("Invalid AI response");

            //  Map API → Application
            return new ChatResponseDto
            {
                Reply = apiResponse.Reply,
                Summary = apiResponse.Summary,
                SummaryUpdated = apiResponse.Summary_Updated,
                //Intent = apiResponse.Intent,
                //ToolCalled = apiResponse.Tool_Called
            };
        }
    }
}
