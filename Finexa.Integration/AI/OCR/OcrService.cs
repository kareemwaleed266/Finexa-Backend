using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Finexa.Application.Modules.AI.OCR.DTOs;
using Finexa.Application.Modules.AI.OCR.Interfaces;
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

        public async Task<ReceiptOcrResponseDto> ProcessAsync(
            IFormFile file,
            List<string> availableCategories)
        {
            var content = new MultipartFormDataContent();

            // 📸 file
            content.Add(
                new StreamContent(file.OpenReadStream()),
                "file",
                file.FileName);

            // 🧠 categories
            content.Add(
                new StringContent(
                    JsonSerializer.Serialize(availableCategories),
                    Encoding.UTF8,
                    "application/json"),
                "available_categories");

            var response = await _httpClient.PostAsync("/api/receipt-ocr", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OCR AI Error: {error}");
            }

            var result = await response.Content
                .ReadFromJsonAsync<ReceiptOcrResponseDto>();

            if (result == null)
                throw new Exception("OCR AI returned empty response");

            return result;
        }
    }
}
