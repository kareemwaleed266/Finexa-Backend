namespace Finexa.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy.WithOrigins(
                                    "https://sass-pearl.vercel.app",
                                    "http://localhost:5173",
                                    "http://localhost:8081",
                                    "https://finexa-admin-six.vercel.app"
                                      )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); 
                });
            });
            return services;
        }
    }
}