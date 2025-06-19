using Masstransient.RabbitMq.Abstractions;
using Masstransient.RabbitMq.Configurations;
using Masstransient.RabbitMq.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Masstransient.RabbitMq.Workers;

public class WorkerDemo : IHostedService
{
    private readonly CancellationTokenSource _cts = new();
    private Task? _executingTask;
    private readonly IServiceProvider _serviceProvider;

    public WorkerDemo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = DoWorkAsync(_cts.Token);

    }

    // The ideal would be create a worker for each message type, but here is just an example
    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Log.Information("Worker running at: {0}", DateTimeOffset.Now);

                var rabbitMqService = _serviceProvider.GetRequiredService<IRabbitMqService>();

                await rabbitMqService.Send(FakeData.GenerateMessage(), QueueConfig.DemoMessage);

                await rabbitMqService.Send(FakeData.GeneratePaymentMessage(), QueueConfig.PaymentMessage);

                await rabbitMqService.Send(FakeData.GenerateOrderMessage(), QueueConfig.OrderMessage);

                Log.Information("Worker finished at", DateTimeOffset.Now);

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message!);
            }

        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();

        if (_executingTask != null)
        {
            await _executingTask;
        }
    }
}