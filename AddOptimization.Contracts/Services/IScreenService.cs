using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services;

public interface IScreenService
{
    Task<ApiResult<List<ScreenDto>>> Search();
    Task<ApiResult<ScreenDto>> Create(ScreenCreateDto model);
    Task<ApiResult<ScreenDto>> Update(Guid id,ScreenCreateDto model);
}
