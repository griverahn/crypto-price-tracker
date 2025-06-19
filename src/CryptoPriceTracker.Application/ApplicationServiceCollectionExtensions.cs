using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoPriceTracker.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR (handlers, pipeline behaviours…)
        services.AddMediatR(assembly);

        // FluentValidation (scans same assembly)
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
