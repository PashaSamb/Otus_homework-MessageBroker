using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Otus.RabbitMq.Interfaces;
using Otus.RabbitMq.Services;
using Otus.RabbitMq.Settings;
using Otus.Teaching.Pcf.GivingToCustomer.Core.Services;
using Otus.Teaching.Pcf.GivingToCustomer.Dto;
using Otus.Teaching.Pcf.GivingToCustomer.WebHost.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Otus.Teaching.Pcf.GivingToCustomer.WebHost.HostedService
{
    public class GivingToCustomerQueueListener : QueueListener
    {
        private readonly IServiceProvider _serviceProvider;

        public GivingToCustomerQueueListener(
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
            var dto = ConvertToGivePromoCodeToCustomer(message);
            if (dto == null) return;

            var request = new GivePromoCodeRequest
            {
                PreferenceId = dto.PreferenceId,
                PromoCode = dto.PromoCode,
                BeginDate = dto.BeginDate,
                EndDate = dto.EndDate,
                PartnerId = dto.PartnerId,
                ServiceInfo = dto.ServiceInfo,
                PromoCodeId = dto.PromoCodeId
            };

            using var scope = _serviceProvider.CreateScope();
            var givePromoCodesToCustomersService = scope.ServiceProvider.GetService<GivePromoCodesToCustomersService>();

            var isGive = await givePromoCodesToCustomersService.GivePromoCodesToCustomersWithPreferenceAsync(request);
            if (!isGive)
            {
                LogError(new Exception($"Preference with id {dto.PreferenceId} not found"), "Error handle message");
                return;
            }
        }

        private GivePromoCodeToCustomerDto ConvertToGivePromoCodeToCustomer(string message)
        {
            try
            {
                return JsonSerializer.Deserialize<GivePromoCodeToCustomerDto>(message);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error parse message");
                return null;
            }
        }
    }
}
