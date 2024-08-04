using Microsoft.Extensions.Options;
using Otus.RabbitMq.Interfaces;
using Otus.RabbitMq.Settings;

namespace Otus.RabbitMq.Implementations
{
    public class QueueSender : IQueueSender
    {
        private readonly IRabbitManager _rabbitManager;
        private readonly QueueSetting _queueSetting;

        public QueueSender(IRabbitManager rabbitManager, IOptions<QueueSetting> options)
        {
            _rabbitManager = rabbitManager;
            _queueSetting = options.Value;
        }

        public void Send(string message, string routingKey, string correlationId = null)
        {
            _rabbitManager.Publish(message, _queueSetting, routingKey, correlationId);
        }

        public void Send<T>(T message, string routingKey, string correlationId = null)
            where T : class
        {
            _rabbitManager.Publish(message, _queueSetting, routingKey, correlationId);
        }
    }
}
