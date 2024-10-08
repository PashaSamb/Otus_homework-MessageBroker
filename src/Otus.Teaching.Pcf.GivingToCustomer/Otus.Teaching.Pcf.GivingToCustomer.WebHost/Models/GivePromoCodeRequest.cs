﻿using Otus.Teaching.Pcf.GivingToCustomer.Core.Abstractions;
using System;

namespace Otus.Teaching.Pcf.GivingToCustomer.WebHost.Models
{
    public class GivePromoCodeRequest : IGivePromoCodeRequest
    {
        public string ServiceInfo { get; set; }

        public Guid PartnerId { get; set; }

        public Guid PromoCodeId { get; set; }
        
        public string PromoCode { get; set; }

        public Guid PreferenceId { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }
    }
}