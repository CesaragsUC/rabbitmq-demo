using Masstransient.RabbitMq.Abstractions;
using Masstransient.RabbitMq.Configurations;
using Masstransient.RabbitMq.Consumers;
using Masstransient.RabbitMq.EventsMessages;
using Masstransient.RabbitMq.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Host = Microsoft.Extensions.Hosting.Host;

namespace Masstransient.RabbitMq;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();

    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
       services.AddMassTransitSetup(
           typeof(MessageConsumer),
           typeof(OrderConsumer),
           typeof(PaymentConsumer));

        services.AddWorkerDemo();
    });

}