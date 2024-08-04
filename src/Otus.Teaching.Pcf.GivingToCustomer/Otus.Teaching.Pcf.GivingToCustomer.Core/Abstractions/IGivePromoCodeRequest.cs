using System;

namespace Otus.Teaching.Pcf.GivingToCustomer.Core.Abstractions
{
    public interface IGivePromoCodeRequest
    {
        string ServiceInfo { get; set; }

        Guid PartnerId { get; set; }

        Guid PromoCodeId { get; set; }

        string PromoCode { get; set; }

        Guid PreferenceId { get; set; }

        string BeginDate { get; set; }

        string EndDate { get; set; }
    }
}
