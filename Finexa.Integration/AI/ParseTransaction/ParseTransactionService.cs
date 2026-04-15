using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.AI.ParseTransaction.Interfaces;
using Finexa.Integration.AI.ParseTransaction.Models;

namespace Finexa.Integration.AI.ParseTransaction
{
    public class ParseTransactionService : IParseTransactionService
    {
        private readonly HttpClient _httpClient;

        public ParseTransactionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ParseTransactionResponseDto> ParseAsync(ParseTransactionRequestDto dto)
        {
            // 🔥 request للـ AI (بنبعت categories)
            var json = JsonSerializer.Serialize(new
            {
                text = dto.Text,
                available_categories = dto.AvailableCategories
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/parse-transaction", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"AI Parse Error: {error}");
            }

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ParseTransactionApiResponse>();

            if (apiResponse == null || apiResponse.Transactions == null)
                throw new Exception("AI returned empty parse response");

            // 🔁 Mapping → Application DTO
            var result = new ParseTransactionResponseDto
            {
                Transactions = apiResponse.Transactions.Select(x => new ParsedTransactionItemDto
                {
                    Amount = x.Amount,
                    Currency = x.Currency,
                    CategoryName = x.Category, // 🔥 مهم جدًا
                    Type = x.Type ?? "expense",
                    Notes = x.Merchant ?? x.Category,
                    OccurredAt = x.OccurredAt
                }).ToList()
            };

            return result;
        }
    }
}