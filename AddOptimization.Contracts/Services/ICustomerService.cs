using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface ICustomerService
{
    Task<ApiResult<List<CustomerSummaryDto>>> GetSummary(PageQueryFiterBase filter);
    Task<PagedApiResult<CustomerDto>> Search(PageQueryFiterBase filter);
    Task<ApiResult<CustomerDto>> Update(Guid id, CustomerCreateDto model);
    Task<ApiResult<CustomerDto>> Create(CustomerCreateDto model);
    Task<ApiResult<CustomerDetailsDto>> Get(Guid id, bool includeOrderStats);
    Task<ApiResult<bool>> Delete(Guid id);
    // Task<ApiResult<List<CustomerLicenseDto>>> GetLicense(Guid customerId);
}
