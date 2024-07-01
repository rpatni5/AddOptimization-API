using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface IEmployeeService
{
    Task<ApiResult<bool>> Save(EmployeeDto model);
    Task<ApiResult<EmployeeDto>> Update(Guid id, EmployeeDto model);
    Task<ApiResult<bool>> SignNDA(Guid id, bool isNDASigned);
    Task<ApiResult<List<EmployeeDto>>> Search(PageQueryFiterBase filters);
    Task<ApiResult<EmployeeDto>> GetEmployeeById(Guid id);
}
