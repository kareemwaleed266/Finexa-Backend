using System.Net.Http.Headers;
using System.Text.Json;
using Finexa.Application.Common.Helpers;
using Finexa.Application.Modules.AI.OCR.DTOs;
using Finexa.Application.Modules.AI.OCR.Interfaces;
using Finexa.Integration.AI.OCR.Models;
using Microsoft.AspNetCore.Http;

namespace Finexa.Integration.AI.OCR
{
    public class OcrService : IOcrService
    {
        private readonly HttpClient _httpClient;

        public OcrService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ReceiptOcrResponseDto> ProcessAsync(IFormFile file)
        {
            var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(file.OpenReadStream());

            if (!string.IsNullOrWhiteSpace(file.ContentType))
            {
                fileContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse(file.ContentType);
            }

            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("/api/receipt-ocr", content);

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OCR AI Error: {response.StatusCode} - {raw}");

            var apiResponse = JsonSerializer.Deserialize<ReceiptOcrApiResponse>(
                raw,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (apiResponse == null)
                throw new Exception("OCR AI returned empty response");

            return new ReceiptOcrResponseDto
            {
                Amount = apiResponse.Amount,
                CategoryName = apiResponse.CategoryName,
                OccurredAt = DateTimeHelper.ParseClientLocalDateTime(apiResponse.OccurredAt),
                Merchant = apiResponse.Merchant,
                Item = apiResponse.Item
            };
        }
    }
}