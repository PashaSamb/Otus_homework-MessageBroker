namespace Otus.RabbitMq.Interfaces
{
    public interface IQueueSender
    {
        void Send(string message, string routingKey, string correlationId = null);

        void Send<T>(T message, string routingKey, string correlationId = null)
            where T : class;
    }
}
