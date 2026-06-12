using System.Net.Http.Json;
using System.Text.Json;
using Finexa.Application.Modules.SavingPlans.DTOs;
using Finexa.Application.Modules.SavingPlans.Interfaces;
using Finexa.Domain.Enums;
using Finexa.Integration.AI.SavingPlan.Models;
using Microsoft.Extensions.Configuration;

namespace Finexa.Integration.AI.SavingPlan
{
    public class SavingPlanAiService : ISavingPlanAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public SavingPlanAiService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<SavingPlanResponseDto> GenerateAsync(
            SavingPlanAiRequestDto request)
        {
            var endpoint = _configuration["ForecastAI:SavingPlanEndpoint"]
                ?? "/api/saving-plan";

            var apiRequest = MapToApiRequest(request);

            var response = await _httpClient.PostAsJsonAsync(
                endpoint,
                apiRequest,
                JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new InvalidOperationException(
                    $"AI Saving Plan Error: {(int)response.StatusCode} - {error}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<SavingPlanApiResponse>(
                JsonOptions);

            if (apiResponse == null)
                throw new InvalidOperationException("AI returned empty saving plan response.");

            return MapToApplicationResponse(apiResponse);
        }

        private static SavingPlanApiRequest MapToApiRequest(
            SavingPlanAiRequestDto request)
        {
            return new SavingPlanApiRequest
            {
                Months = request.Months,
                PlanType = request.PlanType.ToString(),
                TargetMonthlySaving = request.TargetMonthlySaving ?? 0,
                MonthlySummary = request.MonthlySummary.Select(x =>
                    new MonthlySummaryApiModel
                    {
                        Month = x.Month,
                        Income = x.Income,
                        Expenses = x.Expenses,
                        Saving = x.Saving
                    }).ToList(),

                CategorySummary = request.CategorySummary.Select(x =>
                    new CategorySummaryApiModel
                    {
                        CategoryId = x.CategoryId.ToString(),
                        CategoryName = x.CategoryName,
                        CategoryType = x.CategoryType.ToString(),
                        AverageMonthlyAmount = x.AverageMonthlyAmount,
                        TotalAmount = x.TotalAmount,
                        PercentageOfExpenses = x.PercentageOfExpenses,
                        Trend = x.Trend
                    }).ToList()
            };
        }

        private static SavingPlanResponseDto MapToApplicationResponse(
            SavingPlanApiResponse response)
        {
            return new SavingPlanResponseDto
            {
                AverageIncome = Math.Max(0, response.AverageIncome),
                AverageExpenses = Math.Max(0, response.AverageExpenses),
                CurrentAverageSaving = response.CurrentAverageSaving,

                ForecastedIncome = Math.Max(0, response.ForecastedIncome),
                ForecastedExpenses = Math.Max(0, response.ForecastedExpenses),
                ForecastedSaving = response.ForecastedSaving,

                RecommendedMonthlySaving = Math.Max(0, response.RecommendedMonthlySaving),
                ExtraSavingOpportunity = Math.Max(0, response.ExtraSavingOpportunity),

                Difficulty = ParseDifficulty(response.Difficulty),

                // الـ AI بيرجعها باسم planStatus
                // والموديل عندنا بيستقبلها في PlanStatusLabel بـ JsonPropertyName
                PlanStatusLabel = string.IsNullOrWhiteSpace(response.PlanStatusLabel)
                    ? "Unknown"
                    : response.PlanStatusLabel,

                SummaryMessage = response.SummaryMessage ?? string.Empty,

                // الـ AI بيرجع recommendations
                // والموديل عندنا بيستقبلها في Items بـ JsonPropertyName
                Items = response.Items?
                    .Select(item => MapItem(item))
                    .ToList() ?? new List<SavingPlanItemDto>(),

                Insights = response.Insights ?? new List<string>(),
                Warnings = response.Warnings ?? new List<string>()
            };
        }

        private static SavingPlanItemDto MapItem(SavingPlanRecommendationApiModel item)
        {
            return new SavingPlanItemDto
            {
                CategoryId = Guid.TryParse(item.CategoryId, out var categoryId)
                    ? categoryId
                    : null,

                CategoryName = item.CategoryName ?? string.Empty,
                CategoryType = ParseCategoryType(item.CategoryType),

                CurrentAverage = Math.Max(0, item.CurrentAverage),
                RecommendedBudget = Math.Max(0, item.RecommendedBudget),
                ReductionPercentage = Math.Max(0, item.ReductionPercentage),
                ExpectedSaving = Math.Max(0, item.ExpectedSaving),

                Reason = item.Reason ?? string.Empty
            };
        }

        private static SavingPlanDifficulty ParseDifficulty(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return SavingPlanDifficulty.Medium;

            var normalized = value.Trim().ToLowerInvariant();

            return normalized switch
            {
                "low" => SavingPlanDifficulty.Easy,
                "easy" => SavingPlanDifficulty.Easy,

                "medium" => SavingPlanDifficulty.Medium,
                "moderate" => SavingPlanDifficulty.Medium,

                "high" => SavingPlanDifficulty.Hard,
                "hard" => SavingPlanDifficulty.Hard,

                _ => SavingPlanDifficulty.Medium
            };
        }

        private static SavingPlanCategoryType ParseCategoryType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return SavingPlanCategoryType.Flexible;

            return Enum.TryParse<SavingPlanCategoryType>(
                value,
                ignoreCase: true,
                out var result)
                    ? result
                    : SavingPlanCategoryType.Flexible;
        }
    }
}