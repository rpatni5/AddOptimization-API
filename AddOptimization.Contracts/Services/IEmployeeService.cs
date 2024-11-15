﻿using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface IEmployeeService
{
    Task<ApiResult<bool>> Save(EmployeeDto model);
    Task<ApiResult<EmployeeDto>> Update(Guid id, EmployeeDto model);
    Task<ApiResult<bool>> SignNDA(bool isNDASigned);
    Task<PagedApiResult<EmployeeDto>> Search(PageQueryFiterBase filter);
    Task<ApiResult<EmployeeDto>> GetEmployeeById(Guid id);
    Task<ApiResult<EmployeeDto>> GetEmployeeByUserId(int id);
    Task<PagedApiResult<EmployeeDto>> SearchEmployeesNda(PageQueryFiterBase filters);
}
