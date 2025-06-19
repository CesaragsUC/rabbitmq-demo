using Masstransient.RabbitMq.EventsMessages;
using MassTransit;
using Serilog;

namespace Masstransient.RabbitMq.Consumers;


public class MessageConsumer : IConsumer<MessageCreateEvent>
{
    public Task Consume(ConsumeContext<MessageCreateEvent> context)
    {
        Log.Information("MessageConsumer Received Text: {Text}", context.Message.Text);

        return Task.CompletedTask;
    }
}
