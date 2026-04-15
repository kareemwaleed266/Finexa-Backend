using Finexa.Api.Middleware;

namespace Finexa.Api.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IServiceCollection AddGlobalMiddlewares(this IServiceCollection services)
        {
            //services.AddTransient<ExceptionHandlingMiddleware>();
            //services.AddTransient<RequestLoggingMiddleware>();
            return services;
        }

        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            return app;
        }
    }
}
