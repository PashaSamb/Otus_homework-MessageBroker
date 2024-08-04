using Otus.RabbitMq.Interfaces;
using Otus.Teaching.Pcf.ReceivingFromPartner.Core.Abstractions.Notifiers;
using System;
using System.Threading.Tasks;

namespace Otus.Teaching.Pcf.ReceivingFromPartner.Integration.Notifiers
{
    public class AdministrationNotifier : IAdministrationNotifier
    {
        private readonly IQueueSender _queueSender;

        public AdministrationNotifier(IQueueSender queueSender)
        {
            _queueSender = queueSender;
        }

        public Task NotifyAdminAboutPartnerManagerPromoCode(Guid partnerManagerId)
        {
            _queueSender.Send(partnerManagerId.ToString(), "AdministrationPromoCodeAdding");

            return Task.CompletedTask;
        }
    }
}
