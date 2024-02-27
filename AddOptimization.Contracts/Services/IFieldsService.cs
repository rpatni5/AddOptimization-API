using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface IFieldService
{
    Task<ApiResult<List<FieldDto>>> Search(PageQueryFiterBase filter);
    Task<ApiResult<bool>> Create(FieldCreateDto model);
}
