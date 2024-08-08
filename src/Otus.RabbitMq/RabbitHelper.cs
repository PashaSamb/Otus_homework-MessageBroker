using Otus.RabbitMq.Settings;
using RabbitMQ.Client;

namespace Otus.Pcf.RabbitMq
{
    internal static class RabbitHelper
    {
        public static IConnection GetRabbitConnection(RabbitSetting rabbitSetting)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                UserName = rabbitSetting.UserName,
                Password = rabbitSetting.Password,
                VirtualHost = GetRabbitVirtualHost(rabbitSetting),
                HostName = GetRabbitHostName(rabbitSetting),
                Port = GetRabbitPort(rabbitSetting),
                MaxMessageSize = GetRabbitMaxMessageSize(rabbitSetting),
                RequestedChannelMax = GetRabbitRequestedChannelMax(rabbitSetting),
            };
            IConnection conn = factory.CreateConnection();
            return conn;
        }
        public static uint GetRabbitMaxMessageSize(RabbitSetting rabbitSetting) =>
    rabbitSetting.MaxMessageSize ?? ConnectionFactory.DefaultMaxMessageSize;

        public static ushort GetRabbitRequestedChannelMax(RabbitSetting rabbitSetting) =>
            rabbitSetting.RequestedChannelMax ?? ConnectionFactory.DefaultChannelMax;

        public static string GetRabbitVirtualHost(RabbitSetting rabbitSetting) =>
            string.IsNullOrEmpty(rabbitSetting.VirtualHost) ? ConnectionFactory.DefaultVHost : rabbitSetting.VirtualHost;

        public static string GetRabbitHostName(RabbitSetting rabbitSetting) =>
            string.IsNullOrEmpty(rabbitSetting.Host) ? "localhost" : rabbitSetting.Host;

        public static int GetRabbitPort(RabbitSetting rabbitSetting) => rabbitSetting.Port != 0 ? rabbitSetting.Port : 5672;
    }
}
