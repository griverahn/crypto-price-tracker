using CryptoPriceTracker.Application.Interfaces;
using CryptoPriceTracker.Infrastructure.ExternalApis;
using CryptoPriceTracker.Infrastructure.Persistence;
using CryptoPriceTracker.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

namespace CryptoPriceTracker.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
                                                       IConfiguration configuration)
    {
        // DbContext
        var conn = configuration.GetConnectionString("Default") ?? "Data Source=crypto.db";
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(conn));

        // Repository
        services.AddScoped<ICryptoRepository, EfCryptoRepository>();

        // HTTP client + Polly retry (nuevo patrón v3)
        services.AddHttpClient<IPriceFetcher, CoinGeckoHttpClient>()
                .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt =>
                TimeSpan.FromSeconds(Math.Pow(2, attempt))); 
}
