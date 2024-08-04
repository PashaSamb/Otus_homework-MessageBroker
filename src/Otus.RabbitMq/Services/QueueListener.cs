using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Otus.RabbitMq.Interfaces;
using Otus.RabbitMq.Settings;

namespace Otus.RabbitMq.Services
{
    public abstract class QueueListener : BackgroundService
    {
        private readonly IQueueReceiver _queueReceiver;
        private readonly QueueListenerSetting _queueListenerSetting;
        private readonly ILogger<QueueListener> _logger;

        public QueueListener(
            IQueueReceiver queueReceiver,
            IOptions<QueueListenerSetting> options,
            ILogger<QueueListener> logger)
        {
            _queueReceiver = queueReceiver;
            _queueListenerSetting = options.Value;
            _logger = logger;
        }

        protected abstract Task HandleMessageAsync(string message);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_queueListenerSetting.Enabled)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken).ConfigureAwait(false);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("ExecuteAsync", "_queueReceiver.Receive(HandleMessageAsync)");

                    _queueReceiver.Receive(HandleMessageAsync);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "_queueReceiver.Receive(HandleMessageAsync)");
                }

                _logger.LogInformation("ExecuteAsync", $"await Task.Delay({_queueListenerSetting.Period}, stoppingToken)");

                await Task.Delay(_queueListenerSetting.Period, stoppingToken);
            }
        }

        protected void LogError(Exception exception, string message) => _logger.LogError(exception, message);
    }
}