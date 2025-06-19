using Masstransient.RabbitMq.Abstractions;
using Masstransient.RabbitMq.RabbitMqSetup;
using MassTransit;
using MassTransit.Caching;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;

namespace Masstransient.RabbitMq.Services;

[ExcludeFromCodeCoverage]
public class RabbitMqService : IRabbitMqService
{
    public Guid InstanceId { get; }
    public IBusControl Bus { get; }


    public RabbitMqService(IOptions<RabbitMqConfig> options,
        Action<IRabbitMqBusFactoryConfigurator>? extraConfig = null)
    {
        InstanceId = Guid.NewGuid();

        var config = options.Value;
        Bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(config.Host, hostConfigurator =>
            {
                hostConfigurator.Username(config.User);
                hostConfigurator.Password(config.Pass);

                if (options.Value.UseSsl)
                    hostConfigurator.UseSsl(ssl => ssl.Protocol = SslProtocols.Tls12);
            });

            // Se houver "extraConfig", chamamos para permitir configurações adicionais.  
            extraConfig?.Invoke(cfg);
        });

    }


    public async Task Send<T>(T message, Uri queueName, CancellationToken cancellationToken = default) where T : class
    {
        var endpoint = await Bus.GetSendEndpoint(queueName).ConfigureAwait(false);
        await endpoint.Send(message, cancellationToken).ConfigureAwait(false);
    }

    public async Task Send<T>(T message, CancellationToken cancellationToken = default)
    where T : class
    {
        if (!EndpointConvention.TryGetDestinationAddress<T>(out var destinationAddress))
            throw new ArgumentException($"A convention for the message type {TypeCache<T>.ShortName} was not found");

        var endpoint = await Bus.GetSendEndpoint(destinationAddress).ConfigureAwait(false);

        await endpoint.Send(message, cancellationToken).ConfigureAwait(false);
    }
}
