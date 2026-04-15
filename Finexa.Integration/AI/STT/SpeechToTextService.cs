using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using Finexa.Integration.AI.STT.Models;

namespace Finexa.Integration.AI.STT
{
 
    public class SpeechToTextService : ISpeechToTextService
    {
        private readonly HttpClient _httpClient;

        public SpeechToTextService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ConvertToTextAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();

            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("/api/voice-to-text", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"AI Speech Error: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<SpeechResponse>();

            if (result == null || string.IsNullOrWhiteSpace(result.Text))
                throw new Exception("AI returned empty text");

            return result.Text;
        }
    }
}
