using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.RabbitMq.Settings
{
    public class RabbitSetting
    {
        /// <summary>
        /// Логин
        /// </summary>
        public string UserName { get; set; } = "guest";


        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Сервер
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// Виртуальный хост
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// Порт
        /// </summary>
        public int? Port { get; set; } = -1;

        /// <summary>
        /// Максимальное количество каналов, которое будет хранится в пуле
        /// </summary>
        public int ChannelPoolSize { get; set; } = 20;

        /// <summary>
        /// Максимальный размер сообщения
        /// </summary>
        public uint? MaxMessageSize { get; set; }

        //
        // Summary:
        //     Maximum channel number to ask for.
        public ushort? RequestedChannelMax { get; set; } = 2047;
    }
}
