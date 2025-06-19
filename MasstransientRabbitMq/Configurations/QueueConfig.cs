using Microsoft.Extensions.Configuration;

namespace Masstransient.RabbitMq.Configurations;

public static class QueueConfig
{


    public static Uri PaymentMessage => new Uri($"queue:{EnvironmentPrefix()}.demo.payment.event.v1");
    public static Uri OrderMessage => new Uri($"queue:{EnvironmentPrefix()}.demo.order.event.v1");
    public static Uri DemoMessage => new Uri($"queue:{EnvironmentPrefix()}.demo.message.event.v1");

    private static IConfigurationBuilder GetConfigBuilder()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environmentName}.json", true, true)
            .AddEnvironmentVariables();

        return builder;
    }

    public static string EnvironmentPrefix()
    {
        var configuration = GetConfigBuilder().Build();

        return configuration?.GetSection("RabbitMqTransportOptions:Prefix").Value!;
    }

}

