using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Otus.Pcf.RabbitMq;
using Otus.RabbitMq.Interfaces;
using Otus.RabbitMq.Settings;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Otus.RabbitMq.Implementations
{

    public class RabbitManager : IRabbitManager
    {
        private static readonly object _lockObject = new object();
        private IConnection connection;
        private RabbitChannelPool _channelPool;
        private readonly RabbitSetting _rabbitSetting;
        private readonly ILogger<RabbitManager> _logger;
        private readonly ConcurrentDictionary<string, string> _rabbitQueue = new ConcurrentDictionary<string, string>();

        public RabbitManager(
            IOptions<RabbitSetting> rabbitOptions,
            ILogger<RabbitManager> logger)
        {
            _rabbitSetting = rabbitOptions.Value;
            _channelPool = new RabbitChannelPool(_rabbitSetting.ChannelPoolSize);
            _logger = logger;
        }

        /// <summary>
        /// Отправка сообщения в очередь
        /// </summary>
        /// <typeparam name="T">Тип сообщения</typeparam>
        /// <param name="message">Объект сообщения</param>
        /// <param name="queueSetting">Настройки очереди RabbitMq, куда нужно отправить</param>
        /// <param name="routingKey"></param>
        /// <param name="correlationId"></param>
        public void Publish<T>(
            T message,
            QueueSetting queueSetting,
            string routingKey = null,
            string correlationId = null)
        where T : class
        {
            if (message == null)
            {
                return;
            }

            var messageBodyStr = JsonSerializer.Serialize(message);

            Publish(messageBodyStr, queueSetting, routingKey, correlationId);
        }

        /// <summary>
        /// Отправка сообщения в очередь
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="queueSetting">Настройки очереди RabbitMq, куда нужно отправить</param>
        /// <param name="routingKey"></param>
        /// <param name="correlationId"></param>
        public void Publish(
            string message,
            QueueSetting queueSetting,
            string routingKey = null,
            string correlationId = null)
        {
            if (message == null)
            {
                return;
            }

            try
            {
                var sendBytes = Encoding.UTF8.GetBytes(message);
                var connection = GetConnection();
                var channel = _channelPool.Get(connection);

                try
                {
                    var queue = EnsureProducerQueue(channel, queueSetting);

                    var properties = channel.CreateBasicProperties();

                    properties.Persistent = true; //Отметка говорит, что сообщения постоянные
                    properties.CorrelationId = correlationId ?? Guid.NewGuid().ToString();

                    //Первый параметр (exchange) — название.
                    //Пустая строка обозначает обмен по умолчанию
                    //или безымянный: сообщения направляются в очередь с именем,
                    //указанным в routingKey , если оно существует.
                    channel.BasicPublish(exchange: queueSetting.Exchange,
                                         routingKey: routingKey ?? queue, //ключ маршрутизации
                                         basicProperties: properties,
                                         body: sendBytes);

                    _logger.LogInformation("Send message [{0}]: {1}", properties.CorrelationId, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Publish message error");
                    DeleteQueue(queueSetting);
                    throw;
                }
                finally
                {
                    _channelPool.Return(channel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ error connection");
            }
        }

        /// <summary>
        /// Получение сообщения из очереди
        /// </summary>
        /// <typeparam name="T">Тип сообщения</typeparam>
        /// <param name="queueSetting">Настройки очереди RabbitMq, откуда нужно получить сообщение</param>
        /// <param name="handleMessage">Делегат, который обработает сериализованное сообщение из json</param>
        /// <returns>Объект T, сериализованный из Json</returns>
        public void Consume<T>(QueueSetting queueSetting, Action<T, string> handleMessage)
        {
            var channel = GetConnection().CreateModel();
            var consumer = new EventingBasicConsumer(channel);

            var queue = EnsureConsumerQueue(channel, queueSetting);

            consumer.Received += (ch, ea) =>
            {
                T content = default;

                try
                {
                    // received message  
                    var messageBodyStr = Encoding.UTF8.GetString(ea.Body.ToArray());

                    content = JsonSerializer.Deserialize<T>(messageBodyStr);

                    _logger.LogInformation("Receive message [{0}]: {1}", ea.BasicProperties.CorrelationId, content);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON serialization error");
                    channel.BasicNack(ea.DeliveryTag, false, false);
                    _logger.LogInformation("Package acked");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "JSON parsing error");

                    channel.BasicAck(ea.DeliveryTag, false);

                    _logger.LogInformation("Package nacked");
                    return;
                }

                if (!EqualityComparer<T>.Default.Equals(content, default))
                {
                    // handle the received message  
                    handleMessage(content, ea.BasicProperties.CorrelationId);

                    channel.BasicAck(ea.DeliveryTag, false);

                    _logger.LogInformation("Package acked");
                }
                else
                {
                    _logger.LogError("JSON object is null!");

                    channel.BasicNack(ea.DeliveryTag, false, false);

                    _logger.LogInformation("Package nacked");
                }
            };

            _ = channel.BasicConsume(
                queue: queue,
                autoAck: false,//авто подтверждение
                consumer: consumer);
        }

        /// <summary>
        /// Получение сообщения из очереди
        /// </summary>
        /// <typeparam name="T">Тип сообщения</typeparam>
        /// <param name="queueSetting">Настройки очереди RabbitMq, откуда нужно получить сообщение</param>
        /// <param name="handleMessage">Делегат, который обработает сообщение</param>
        public void Consume(QueueSetting queueSetting, Action<string, string> handleMessage)
        {
            var channel = GetConnection().CreateModel();
            var consumer = new EventingBasicConsumer(channel);

            var queue = EnsureConsumerQueue(channel, queueSetting);

            consumer.Received += (ch, ea) =>
            {
                // received message 
                string content = Encoding.UTF8.GetString(ea.Body.ToArray());

                _logger.LogInformation("Receive message [{0}]: {1}", ea.BasicProperties.CorrelationId, content);

                // handle the received message  
                handleMessage(content, ea.BasicProperties.CorrelationId);

                channel.BasicAck(ea.DeliveryTag, false);

                _logger.LogInformation("Package acked");
            };

            _ = channel.BasicConsume(
                queue: queue,
                autoAck: false,//авто подтверждение
                consumer: consumer);
        }

        private IConnection GetConnection()
        {
            lock (_lockObject)
            {
                connection ??= RabbitHelper.GetRabbitConnection(_rabbitSetting);
                return connection;
            }
        }

        private string EnsureProducerQueue(IModel channel, QueueSetting queueSetting)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(queueSetting.Exchange))
                {
                    return EnsureQueue(channel, queueSetting);
                }

                return EnsureProducerExchange(channel, queueSetting);
            }
        }

        private string EnsureProducerExchange(IModel channel, QueueSetting queueSetting)
        {
            EnsureExchange(channel, queueSetting);

            // !Сообщения будут потеряны, если к обмену еще не привязана очередь

            if (string.IsNullOrEmpty(queueSetting.Queue) &&
                (queueSetting.BindingKeys?.Length ?? 0) == 0)
            {
                return null;
            }

            var queueName = EnsureQueue(channel, new QueueSetting { Queue = queueSetting.Queue });

            EnsureQueueBind(channel, queueSetting.Exchange, queueName, queueSetting.BindingKeys);

            return queueName;
        }

        private string EnsureConsumerQueue(IModel channel, QueueSetting queueSetting)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(queueSetting.Exchange))
                {
                    return EnsureQueue(channel, queueSetting, true);
                }

                return EnsureConsumerExchange(channel, queueSetting);
            }
        }

        private string EnsureConsumerExchange(IModel channel, QueueSetting queueSetting)
        {
            EnsureExchange(channel, queueSetting);

            // !Сообщения будут потеряны, если к обмену еще не привязана очередь

            var queueName = EnsureQueue(channel, new QueueSetting { Queue = queueSetting.Queue, PrefetchCount = queueSetting.PrefetchCount }, true);

            EnsureQueueBind(channel, queueSetting.Exchange, queueName, queueSetting.BindingKeys);

            return queueName;
        }

        private string EnsureQueue(IModel channel, QueueSetting queueSetting, bool needSetBasicQos = false)
        {
            var queueName = queueSetting.Queue ?? "";

            if (!_rabbitQueue.ContainsKey(queueName))
            {
                //! параметры очереди назначаются только один раз. 
                //! операция создания очереди идемпотентна
                //! при повторном вызове новая очередь создана не будет 
                var result = channel.QueueDeclare(
                    queue: queueName,
                    durable: true,//Прочность или надежность очереди (позволит сообщениям в очереди пережить перезапуск RabbitMQ)
                    exclusive: false,
                    autoDelete: false, //авто удаление
                    arguments: null);

                if (needSetBasicQos && queueSetting.PrefetchCount > 0)
                {
                    channel.BasicQos(prefetchSize: 0, prefetchCount: queueSetting.PrefetchCount.Value, global: false);
                }

                _rabbitQueue.TryAdd(queueName, result.QueueName);
            }

            return _rabbitQueue[queueName];
        }

        private void EnsureExchange(IModel channel, QueueSetting queueSetting)
        {
            var exchangeQueue = $"{queueSetting.Exchange}-{queueSetting.ExchangeType}";

            if (!_rabbitQueue.ContainsKey(exchangeQueue))
            {
                channel.ExchangeDeclare(exchange: queueSetting.Exchange, type: queueSetting.ExchangeType);

                _rabbitQueue.TryAdd(exchangeQueue, null);
            }
        }

        private void EnsureQueueBind(IModel channel, string exchange, string queueName, string[] bindingKeys)
        {
            //Связывание — это отношение между обменом и очередью (очередь заинтересована в сообщениях от этого обмена)

            if (bindingKeys?.Length > 0)
            {
                foreach (var key in bindingKeys)
                {
                    channel.QueueBind(
                        queue: queueName,
                        exchange: exchange,
                        routingKey: key, //ключ привязки
                        arguments: null);
                }
            }
            else
            {
                channel.QueueBind(
                    queue: queueName,
                    exchange: exchange,
                    routingKey: "",
                    arguments: null);
            }
        }

        private void DeleteQueue(QueueSetting queueSetting)
        {
            if (!string.IsNullOrEmpty(queueSetting.Queue))
            {
                if (_rabbitQueue.ContainsKey(queueSetting.Queue))
                {
                    _rabbitQueue.TryRemove(queueSetting.Queue, out _);
                }

                return;
            }

            var exchangeQueue = $"{queueSetting.Exchange}-{queueSetting.ExchangeType}";

            if (!_rabbitQueue.ContainsKey(exchangeQueue))
            {
                _rabbitQueue.TryRemove(exchangeQueue, out _);
            }
        }

    }
}