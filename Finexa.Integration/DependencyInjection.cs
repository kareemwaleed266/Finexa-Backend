using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Finexa.Application.Modules.AI.Chat.Interfaces;
using Finexa.Integration.AI.Chat;
using Finexa.Integration.AI.STT;
using Finexa.Application.Modules.AI.ParseTransaction.Interfaces;
using Finexa.Integration.AI.ParseTransaction;
using Finexa.Application.Modules.AI.OCR.Interfaces;
using Finexa.Integration.AI.OCR;

namespace Finexa.Integration
{
   
    public static class DependencyInjection
    {
        public static IServiceCollection AddIntegrationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddHttpClient<IChatService, ChatService>(client =>
            {
                client.BaseAddress = new Uri(configuration["AI:BaseUrl"]);
            });

            services.AddHttpClient<ISpeechToTextService, SpeechToTextService>(client =>
            {
                client.BaseAddress = new Uri(configuration["AI:BaseUrl"]);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IParseTransactionService, ParseTransactionService>(client =>
            {
                client.BaseAddress = new Uri(configuration["AI:BaseUrl"]);
                client.Timeout = TimeSpan.FromMinutes(2);
            });

            services.AddHttpClient<IOcrService, OcrService>(client =>
            {
                client.BaseAddress = new Uri(configuration["AI:BaseUrl"]);
            });
            return services;
        }
    }
}
