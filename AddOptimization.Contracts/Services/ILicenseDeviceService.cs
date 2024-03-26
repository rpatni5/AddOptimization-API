using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services;

public interface ILicenseDeviceService
{
    Task<ApiResult<List<LicenseDeviceDto>>> GetByLicenseId(Guid licenseId);
    Task<ApiResult<bool>> Delete(Guid id);
}
