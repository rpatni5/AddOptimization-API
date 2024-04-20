using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services;

public interface ILicenseDeviceService
{
    Task<ApiResult<List<LicenseDeviceDto>>> GetByLicenseId(Guid licenseId);
    Task<ApiResult<LicenseDeviceDto>> ActivateLicense(LicenseDeviceManagementDto request);
    Task<ApiResult<bool>> ValidateLicense(LicenseDeviceManagementDto request);
    Task<ApiResult<bool>> RemoveLicense(LicenseDeviceManagementDto request);
    Task<ApiResult<bool>> Delete(Guid id);
}
