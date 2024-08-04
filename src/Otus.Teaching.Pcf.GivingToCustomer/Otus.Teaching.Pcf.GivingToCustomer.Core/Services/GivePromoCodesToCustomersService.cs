using Otus.Teaching.Pcf.GivingToCustomer.Core.Abstractions;
using Otus.Teaching.Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Otus.Teaching.Pcf.GivingToCustomer.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.Teaching.Pcf.GivingToCustomer.Core.Services
{
    public class GivePromoCodesToCustomersService
    {
        private readonly IRepository<Preference> _preferencesRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<PromoCode> _promoCodeRepository;

        public GivePromoCodesToCustomersService(
            IRepository<Preference> preferencesRepository,
            IRepository<Customer> customerRepository,
            IRepository<PromoCode> promoCodeRepository)
        {
            _preferencesRepository = preferencesRepository;
            _customerRepository = customerRepository;
            _promoCodeRepository = promoCodeRepository;
        }

        public async Task<bool> GivePromoCodesToCustomersWithPreferenceAsync(IGivePromoCodeRequest request)
        {
            //Получаем предпочтение по имени
            var preference = await _preferencesRepository.GetByIdAsync(request.PreferenceId);

            if (preference == null)
            {
                return false;
            }

            //  Получаем клиентов с этим предпочтением:
            var customers = await _customerRepository
                .GetWhere(d => d.Preferences.Any(x =>
                    x.Preference.Id == preference.Id));

            PromoCode promoCode = MapFromModel(request, preference, customers);

            await _promoCodeRepository.AddAsync(promoCode);

            return true;
        }

        private static PromoCode MapFromModel(IGivePromoCodeRequest request, Preference preference, IEnumerable<Customer> customers)
        {

            var promocode = new PromoCode
            {
                Id = request.PromoCodeId,

                PartnerId = request.PartnerId,
                Code = request.PromoCode,
                ServiceInfo = request.ServiceInfo,

                BeginDate = DateTime.Parse(request.BeginDate),
                EndDate = DateTime.Parse(request.EndDate),

                Preference = preference,
                PreferenceId = preference.Id,

                Customers = new List<PromoCodeCustomer>()
            };

            foreach (var item in customers)
            {
                promocode.Customers.Add(new PromoCodeCustomer()
                {
                    CustomerId = item.Id,
                    Customer = item,
                    PromoCodeId = promocode.Id,
                    PromoCode = promocode
                });
            };

            return promocode;
        }
    }
}
