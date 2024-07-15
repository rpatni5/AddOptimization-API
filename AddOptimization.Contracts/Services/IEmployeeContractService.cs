using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface IEmployeeContractService
{
    Task<ApiResult<EmployeeContractResponseDto>> Create(EmployeeContractRequestDto model);
    Task<ApiResult<EmployeeContractResponseDto>> GetEmployeeContractById(Guid id);
    Task<ApiResult<EmployeeContractResponseDto>> Update(Guid id, EmployeeContractRequestDto model);


}
