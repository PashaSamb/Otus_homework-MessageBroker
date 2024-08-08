using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Otus.RabbitMq.Implementations;
using Otus.RabbitMq.Interfaces;
using Otus.RabbitMq.Settings;

namespace Otus.Pcf.RabbitMq
{
    public static class RabbitServiceCollectionExtension
    {
        public static IServiceCollection ConfigureRabbitServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<RabbitSetting>(configuration.GetSection(nameof(RabbitSetting)));
            services.Configure<QueueSetting>(configuration.GetSection(nameof(QueueSetting)));
            services.Configure<QueueListenerSetting>(configuration.GetSection(nameof(QueueListenerSetting)));


            services.AddSingleton<IRabbitManager, RabbitManager>();
            services.AddSingleton<IQueueReceiver, QueueReceiver>();
            services.AddScoped<IQueueSender, QueueSender>();

            return services;
        }
    }
}
