using Microsoft.OpenApi.Models;

namespace Finexa.Api.Extensions
{
    public static class SwaggerExtensions
    {
        //    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        //    {
        //        services.AddSwaggerGen(options =>
        //        {
        //            options.SwaggerDoc("v1", new OpenApiInfo
        //            {
        //                Title = "Finexa API",
        //                Version = "v1"
        //            });

        //            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        //            {
        //                Name = "Authorization",
        //                Type = SecuritySchemeType.Http,
        //                Scheme = "Bearer",
        //                BearerFormat = "JWT",
        //                In = ParameterLocation.Header,
        //                Description = "Enter JWT Token"
        //            });

        //            options.AddSecurityRequirement(new OpenApiSecurityRequirement
        //{
        //    {
        //        new OpenApiSecurityScheme
        //        {
        //            Reference = new OpenApiReference
        //            {
        //                Type = ReferenceType.SecurityScheme,
        //                Id = "Bearer"
        //            }
        //        },
        //        Array.Empty<string>()
        //    }
        //});
        //        });
        //        return services;
        //    }
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Finexa API",
                    Version = "v1",
                    Description = "Personal Finance Management System with AI Features",
                    Contact = new OpenApiContact
                    {
                        Name = "Finexa Team"
                    }
                });

                // 🔐 JWT
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {your token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

                // 🔥 تخلي كل endpoints محتاجة auth تلقائي
                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            return services;
        }

        //public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //    return app;
        //}

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Finexa API v1");

                options.DocumentTitle = "Finexa API Docs";

                options.DisplayRequestDuration(); 

                options.DefaultModelsExpandDepth(-1);

                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); 
            });

            return app;
        }
    }
}
