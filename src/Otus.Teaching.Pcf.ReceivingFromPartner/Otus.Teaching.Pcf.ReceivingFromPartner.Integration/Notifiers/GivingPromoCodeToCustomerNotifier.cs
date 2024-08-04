using Otus.RabbitMq.Interfaces;
using Otus.Teaching.Pcf.ReceivingFromPartner.Core.Abstractions.Notifiers;
using Otus.Teaching.Pcf.ReceivingFromPartner.Core.Domain;
using Otus.Teaching.Pcf.ReceivingFromPartner.Integration.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.Teaching.Pcf.ReceivingFromPartner.Integration.Notifiers
{
    public class GivingPromoCodeToCustomerNotifier : IGivingPromoCodeToCustomerNotifier
    {
        private readonly IQueueSender _queueSender;

        public GivingPromoCodeToCustomerNotifier(IQueueSender queueSender)
        {
            _queueSender = queueSender;
        }

        public Task GivePromoCodeToCustomer(PromoCode promoCode)
        {
            var dto = new GivePromoCodeToCustomerDto()
            {
                PartnerId = promoCode.Partner.Id,
                BeginDate = promoCode.BeginDate.ToShortDateString(),
                EndDate = promoCode.EndDate.ToShortDateString(),
                PreferenceId = promoCode.PreferenceId,
                PromoCode = promoCode.Code,
                ServiceInfo = promoCode.ServiceInfo,
                PartnerManagerId = promoCode.PartnerManagerId
            };

            _queueSender.Send(dto, "GivePromoCodeToCustomerPromoCodeAdding");

            return Task.CompletedTask;
        }
    }
}
