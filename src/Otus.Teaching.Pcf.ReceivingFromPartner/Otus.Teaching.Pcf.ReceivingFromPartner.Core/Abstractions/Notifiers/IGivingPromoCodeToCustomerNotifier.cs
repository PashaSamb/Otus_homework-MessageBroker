using Otus.Teaching.Pcf.ReceivingFromPartner.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.Teaching.Pcf.ReceivingFromPartner.Core.Abstractions.Notifiers
{
    public interface IGivingPromoCodeToCustomerNotifier
    {
        Task GivePromoCodeToCustomer(PromoCode promoCode);
    }
}
