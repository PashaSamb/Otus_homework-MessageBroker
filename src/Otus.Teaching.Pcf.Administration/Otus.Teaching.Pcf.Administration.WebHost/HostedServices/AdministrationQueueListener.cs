using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Otus.RabbitMq.Interfaces;
using Otus.RabbitMq.Services;
using Otus.RabbitMq.Settings;
using Otus.Teaching.Pcf.Administration.Core.Services;
using System;
using System.Threading.Tasks;

namespace Otus.Teaching.Pcf.Administration.WebHost.HostedServices
{
    public class AdministrationQueueListener : QueueListener
    {
        private readonly IServiceProvider _serviceProvider;

        public AdministrationQueueListener(
            IQueueReceiver queueReceiver, 
            IOptions<QueueListenerSetting> options,
            ILogger<QueueListener> logger,
            IServiceProvider serviceProvider)
            : base(queueReceiver, options, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleMessageAsync(string message)
        {
            if (!Guid.TryParse(message, out var id))
            {
                LogError(new Exception($"'{message}' not Guid"), "Error parse message");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var applyPromocodesService = scope.ServiceProvider.GetService<ApplyPromocodesService>();
            var isAppliedPromocodes = await applyPromocodesService.UpdateAppliedPromocodesAsync(id);
            if (!isAppliedPromocodes)
            {
                LogError(new Exception($"Employee with id {id} not found"), "Error handle message");
            }
        }
    }
}
