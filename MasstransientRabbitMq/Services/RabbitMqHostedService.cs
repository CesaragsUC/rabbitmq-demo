using Masstransient.RabbitMq.Abstractions;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Masstransient.RabbitMq.Services;


[ExcludeFromCodeCoverage]
public class RabbitMqHostedService : IHostedService
{
    private readonly IRabbitMqService _rabbitMqService;

    public RabbitMqHostedService(IRabbitMqService rabbitMqService)
    {
        _rabbitMqService = rabbitMqService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _rabbitMqService.Bus.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _rabbitMqService.Bus.StopAsync(cancellationToken);
    }
}
