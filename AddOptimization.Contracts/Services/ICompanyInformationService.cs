using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services;

public interface ICompanyInformationService
{
    Task<ApiResult<CompanyInformationDto>> Create(CompanyInformationDto model);
    Task<ApiResult<CompanyInformationDto>> GetCompanyInformation();
    
}
