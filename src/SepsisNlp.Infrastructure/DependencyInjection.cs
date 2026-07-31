using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SepsisNlp.Application.Common.Interfaces;
using SepsisNlp.Infrastructure.Data.Context;
using SepsisNlp.Infrastructure.Services;

namespace SepsisNlp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Banco de Dados PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Mensageria RabbitMQ via MassTransit
        services.AddMassTransit(x =>
        {
            // Pede para o MassTransit procurar Consumers na camada Application
            x.AddConsumers(typeof(SepsisNlp.Application.DependencyInjection).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                // Configuração padrão do RabbitMQ local
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                // Essa linha faz o MassTransit criar as filas automaticamente!
                cfg.ConfigureEndpoints(context);
            });
        });

        // ==========================================================
        // INTEGRAÇÃO COM A IA EM PYTHON (TAILSCALE VPN)
        // ==========================================================
        services.AddHttpClient<IPythonNlpClient, PythonNlpClient>((provider, client) =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(config["PythonApi:BaseUrl"]!);
        });

        return services;
    }
}