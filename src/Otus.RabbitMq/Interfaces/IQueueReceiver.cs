namespace Otus.RabbitMq.Interfaces
{
    public interface IQueueReceiver
    {
        void Receive(Func<string, Task> handleMessageAsync);
        void Receive(Action<string> handleMessage);
    }
}
