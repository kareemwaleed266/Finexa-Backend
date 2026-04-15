using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using FluentValidation;
using System.Reflection;
using Finexa.Application.Modules.Identity.Interfaces;
using Finexa.Application.Modules.Identity.Services;

namespace Finexa.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
