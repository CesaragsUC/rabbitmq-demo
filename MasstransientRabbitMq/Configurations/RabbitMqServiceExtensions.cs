using Masstransient.RabbitMq.Abstractions;
using Masstransient.RabbitMq.Consumers;
using Masstransient.RabbitMq.RabbitMqSetup;
using Masstransient.RabbitMq.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using System.Security.Authentication;

namespace Masstransient.RabbitMq.Configurations;

public static class RabbitMqServiceExtensions
{
    public static IServiceCollection AddMassTransitSetup(
        this IServiceCollection services,
        params Type[] consumerAssemblies)
    {

        var configuration = GetConfigBuilder().Build();

        services.Configure<RabbitMqConfig>(configuration.GetSection("RabbitMqTransportOptions"));

        var rabbitMqOptions = new RabbitMqConfig();
        configuration.GetSection("RabbitMqTransportOptions").Bind(rabbitMqOptions);

        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddHostedService<RabbitMqHostedService>();
        services.AddMassTransit(x =>
        {
            foreach (var assembly in consumerAssemblies)
            {
                x.AddConsumers(assembly.Assembly);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.Host, host =>
                {
                    host.Username(rabbitMqOptions.User);
                    host.Password(rabbitMqOptions.Pass);

                    if (rabbitMqOptions.UseSsl)
                        host.UseSsl(ssl => ssl.Protocol = SslProtocols.Tls12);
                });

                foreach (var consumerType in consumerAssemblies)
                {
                    var queueConfig = GetRabbitEndpointConfig(consumerType, rabbitMqOptions);

                    var method = typeof(RabbitMqServiceExtensions)
                        .GetMethod(nameof(ConfigureEndpoint),
                         BindingFlags.NonPublic |
                         BindingFlags.Static)
                        ?.MakeGenericMethod(consumerType);

                    method?.Invoke(null, new object[] { cfg, context, queueConfig });
                }

            });

        });


        return services;
    }

    private static void ConfigureEndpoint<TConsumer>(
    this IRabbitMqBusFactoryConfigurator configRabbit,
    IBusRegistrationContext context,
    RabbitMqEndpointConfig endpointConfig)
    where TConsumer : class, IConsumer
    {

        configRabbit.ReceiveEndpoint(endpointConfig.QueueName!, configureEndpoint =>
        {
            configureEndpoint.ConfigureConsumeTopology = endpointConfig.ConfigureConsumeTopology;
            configureEndpoint.PrefetchCount = endpointConfig.PrefetchCount;

            configureEndpoint.UseMessageRetry(retry =>
            {
                retry.Interval(endpointConfig.RetryLimit, endpointConfig.Interval);
                retry.Ignore<ConsumerCanceledException>();
                retry.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5))
                    .Handle<Exception>();

                retry.Handle<Exception>(ex =>
                {
                    Log.Error(ex, $"An Error occour on retry: {ex.Message}");
                    return true;
                });
            });

            configureEndpoint.AutoDelete = false;
            configureEndpoint.ConfigureConsumer<TConsumer>(context);
        });

    }

    private static RabbitMqEndpointConfig GetRabbitEndpointConfig(Type consumerType, RabbitMqConfig rabbitMqConfig)
    {
        var eventName = consumerType.Name.Replace("Consumer", ".event");

        return new RabbitMqEndpointConfig
        {
            QueueName = $"{rabbitMqConfig.Prefix}.demo.{eventName.ToLower()}.v1",
            RoutingKey = eventName,
            ExchangeType = RabbitMQ.Client.ExchangeType.Fanout,
            RetryLimit = 3,
            Interval = TimeSpan.FromSeconds(3),
            ConfigureConsumeTopology = false,
            PrefetchCount = 1
        };
    }

    private static IConfigurationBuilder GetConfigBuilder()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environmentName}.json", true, true)
            .AddEnvironmentVariables();

        return builder;
    }
}
