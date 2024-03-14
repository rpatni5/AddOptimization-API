using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface ILicenseService
{
    Task<PagedApiResult<LicenseDetailsDto>> Search(PageQueryFiterBase filter);
    Task<ApiResult<LicenseDetailsDto>> Get(Guid orderId);
    Task<ApiResult<LicenseDetailsDto>> Update(Guid id,LicenseUpdateDto model);
    Task<ApiResult<LicenseDetailsDto>> Create(LicenseCreateDto model);
}
