using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface ISchedulerEventService
    {
        Task<PagedApiResult<SchedulerEventDetailsDto>> Search(PageQueryFiterBase filter);
     
        Task<ApiResult<bool>> Delete(Guid id);

        Task<ApiResult<bool>> Save(List<SchedulerEventDetailsDto> model);
        Task<ApiResult<bool>> SubmitEventDetails(List<SchedulerEventDetailsDto> model);
        Task<ApiResult<CreateViewTimesheetResponseDto>> CreateOrViewTimeSheets(CreateViewTimesheetRequestDto model);
        Task<ApiResult<List<SchedulerEventDetailsDto>>> GetSchedularEventDetails(Guid id);
        Task<ApiResult<CreateViewTimesheetResponseDto>> GetSchedulerEvent(Guid id);
    }
}
