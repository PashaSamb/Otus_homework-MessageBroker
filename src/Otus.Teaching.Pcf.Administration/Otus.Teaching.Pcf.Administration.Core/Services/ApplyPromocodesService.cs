using Otus.Teaching.Pcf.Administration.Core.Abstractions.Repositories;
using Otus.Teaching.Pcf.Administration.Core.Domain.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.Teaching.Pcf.Administration.Core.Services
{
    public class ApplyPromocodesService
    {
        private readonly IRepository<Employee> _employeeRepository;

        public ApplyPromocodesService(IRepository<Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<bool> UpdateAppliedPromocodesAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return false;

            employee.AppliedPromocodesCount++;

            await _employeeRepository.UpdateAsync(employee);

            return true;
        }
    }
}
