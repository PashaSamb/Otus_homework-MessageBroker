using RabbitMQ.Client;
using System.Diagnostics;

namespace Otus.Pcf.RabbitMq
{
    internal sealed class RabbitChannelPool
    {
        [DebuggerDisplay("{Channel}")]
        private struct ChannelWrapper
        {
            public IModel Channel;
        }

        private IModel _currentChannel;
        private readonly ChannelWrapper[] _channels;

        /// <summary>
        /// Создание экземпляра <see cref="RabbitChannelPool"/>.
        /// </summary>
        public RabbitChannelPool() : this(Environment.ProcessorCount * 2)
        {
        }

        /// <summary>
        /// Создание экземпляра <see cref="RabbitChannelPool"/>.
        /// </summary>
        /// <param name="maximumRetained">Максимальное количество объектов, которые нужно сохранить в пуле.</param>
        public RabbitChannelPool(int maximumRetained)
        {
            // -1 due to _currentElement
            _channels = new ChannelWrapper[maximumRetained - 1];
        }

        /// <summary>
        /// Возвращает один канал из пула.
        /// Если в пуле нет свободных каналов, то создается новый канал для указанного соединения.
        /// </summary>
        /// <param name="connection">Соединение в рамках которого, будет создаваться канал</param>
        /// <returns>Канал из пула, либо новый, созданный канал</returns>
        public IModel Get(IConnection connection)
        {
            // Если в _currentChannel содержится канал, то делается попытка с помощью атомарной операции записать в _currentChannel null,
            // а содержащееся там значение вернуть в качестве результата выполнения метода.
            var item = _currentChannel;
            if (item != null && Interlocked.CompareExchange(ref _currentChannel, null, item) == item)
            {
                return item;
            }

            for (var i = 0; i < _channels.Length; i++)
            {
                item = _channels[i].Channel;
                if (item != null && Interlocked.CompareExchange(ref _channels[i].Channel, null, item) == item)
                {
                    return item;
                }
            }

            return connection.CreateModel();
        }

        /// <summary>
        /// Возвращает канал в пул.
        /// Если пул уже полон, то канал закрывается и его ресурсы освобождаются.
        /// </summary>
        /// <param name="channel">Канал</param>
        public void Return(IModel channel)
        {
            if (channel == null)
            {
                return;
            }

            if (!channel.IsOpen)
            {
                // неработоспособные каналы в пуле не нужны
                channel.Dispose();
                return;
            }

            // Если в _currentChannel нет канала, то делается попытка с помощью атомарной операции заполнить его переданным объектом channel.
            // В случае успеха работа метода завершается.
            if (_currentChannel == null && Interlocked.CompareExchange(ref _currentChannel, channel, null) == null)
            {
                return;
            }

            for (var i = 0; i < _channels.Length; i++)
            {
                if (Interlocked.CompareExchange(ref _channels[i].Channel, channel, null) == null)
                {
                    return;
                }
            }

            // в пуле нет свободных мест, закрываем канал
            channel.Dispose();
        }
    }
}
