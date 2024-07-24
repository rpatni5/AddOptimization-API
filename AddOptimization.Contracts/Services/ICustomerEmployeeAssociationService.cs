using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;
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
        Task<PagedApiResult<CustomerEmployeeAssociationDto>> SearchAllAssociations(PageQueryFiterBase filter);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<List<CustomerEmployeeAssociationDto>>> GetAssociatedCustomers(int employeeId);
        Task<ApiResult<CustomerEmployeeAssociationDto>> Get(Guid id);
    }
}
