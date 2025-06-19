namespace Masstransient.RabbitMq.EventsMessages;


public class MessageCreateEvent
{
    public string Text { get; set; }
}

public class MessageCreateEventError
{
    public string Text { get; set; }
}
