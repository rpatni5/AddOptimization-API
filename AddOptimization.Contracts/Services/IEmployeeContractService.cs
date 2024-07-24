using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface IEmployeeContractService
{
    Task<ApiResult<EmployeeContractResponseDto>> Create(EmployeeContractRequestDto model);
    Task<ApiResult<EmployeeContractResponseDto>> GetEmployeeContractById(Guid id);
    Task<ApiResult<EmployeeContractResponseDto>> Update(Guid id, EmployeeContractRequestDto model);
    Task<ApiResult<bool>> SignContract(Guid contractId);
    Task<ApiResult<EmployeeContractResponseDto>> GetEmployeeContractByEmployeeId(int id);

    Task<PagedApiResult<EmployeeContractResponseDto>> Search(PageQueryFiterBase filters);

    Task<ApiResult<List<EmployeeContractResponseDto>>> GetContractsByAsscociationId(Guid id);

    Task<PagedApiResult<EmployeeContractResponseDto>> SearchAllContracts(PageQueryFiterBase filters);
    Task<ApiResult<bool>> Delete(Guid id);


}
