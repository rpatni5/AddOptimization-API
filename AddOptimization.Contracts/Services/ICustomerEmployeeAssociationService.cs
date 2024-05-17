using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface ICustomerEmployeeAssociationService
    {
        Task<ApiResult<CustomerEmployeeAssociationDto>> Create(CustomerEmployeeAssociationDto model);
        Task<ApiResult<List<CustomerEmployeeAssociationDto>>> Search();
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<List<CustomerEmployeeAssociationDto>>> GetAssociatedCustomers(int employeeId);
    }
}
