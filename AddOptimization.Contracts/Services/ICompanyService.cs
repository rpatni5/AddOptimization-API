using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services;

public interface ICompanyService
{
    Task<ApiResult<CompanyDto>> Create(CompanyDto model);
    Task<ApiResult<CompanyDto>> GetCompanyInformation();
    
}
