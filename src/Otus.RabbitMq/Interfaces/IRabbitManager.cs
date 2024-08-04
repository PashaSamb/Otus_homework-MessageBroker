using Otus.RabbitMq.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.RabbitMq.Interfaces
{
    public interface IRabbitManager
    {
        /// <summary>
        /// Отправка сообщения в очередь
        /// </summary>
        /// <typeparam name="T">Тип сообщения</typeparam>
        /// <param name="message">Объект сообщения</param>
        /// <param name="queueSetting">Настройки очереди RabbitMq, куда нужно отправить</param>
        /// <param name="routingKey"></param>
        /// <param name="correlationId"></param>
        void Publish<T>(
            T message,
            QueueSetting queueSetting,
            string routingKey = null,
            string correlationId = null)
        where T : class;

        /// <summary>
        /// Отправка сообщения в очередь
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="queueSetting">Настройки очереди RabbitMq, куда нужно отправить</param>
        /// <param name="routingKey"></param>
        /// <param name="correlationId"></param>
        void Publish(
            string message,
            QueueSetting queueSetting,
            string routingKey = null,
            string correlationId = null);

        /// <summary>
        /// Получение сообщения из очереди
        /// </summary>
        /// <typeparam name="T">Тип сообщения</typeparam>
        /// <param name="queueSetting">Настройки очереди RabbitMq, откуда нужно получить сообщение</param>
        /// <param name="handleMessage">Делегат, который обработает сериализованное сообщение из json</param>
        /// <returns>Объект T, сериализованный из Json</returns>
        void Consume<T>(QueueSetting queueSetting, Action<T, string> handleMessage);

        /// <summary>
        /// Получение сообщения из очереди
        /// </summary>
        /// <typeparam name="T">Тип сообщения</typeparam>
        /// <param name="queueSetting">Настройки очереди RabbitMq, откуда нужно получить сообщение</param>
        /// <param name="handleMessage">Делегат, который обработает сообщение</param>
        void Consume(QueueSetting queueSetting, Action<string, string> handleMessage);
    }
}
