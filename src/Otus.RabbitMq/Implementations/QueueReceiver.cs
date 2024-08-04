using Microsoft.Extensions.Options;
using Otus.RabbitMq.Interfaces;
using Otus.RabbitMq.Settings;

namespace Otus.RabbitMq.Implementations
{

    public class QueueReceiver : IQueueReceiver
    {
        private readonly IRabbitManager _rabbitManager;
        private readonly QueueSetting _queueSetting;

        public QueueReceiver(IRabbitManager rabbitManager, IOptions<QueueSetting> options)
        {
            _rabbitManager = rabbitManager;
            _queueSetting = options.Value;
        }

        public void Receive(Func<string, Task> handleMessageAsync)
        {
            _rabbitManager.Consume(_queueSetting, async (message, _) => await handleMessageAsync!.Invoke(message));
        }

        public void Receive(Action<string> handleMessage)
        {
            _rabbitManager.Consume(_queueSetting, (message, _) => handleMessage!.Invoke(message));
        }
    }
}

