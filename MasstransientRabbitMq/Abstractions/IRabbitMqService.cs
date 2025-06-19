using MassTransit;
using RabbitMQ.Client;

namespace Masstransient.RabbitMq.Abstractions;

public interface IRabbitMqService
{
    Guid InstanceId { get; }

    IBusControl Bus { get; }

    Task Send<T>(T message, Uri queueName, CancellationToken cancellationToken = default)
    where T : class;

    Task Send<T>(T message, CancellationToken cancellationToken = default)
    where T : class;
}
