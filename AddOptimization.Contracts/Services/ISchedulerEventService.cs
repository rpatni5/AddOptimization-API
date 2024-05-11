using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface ISchedulerEventService
    {
        Task<PagedApiResult<CreateViewTimesheetResponseDto>> Search(PageQueryFiterBase filters);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<bool>> Save(List<SchedulerEventDetailsDto> schedulerEventDetails);
        Task<ApiResult<bool>> SubmitEventDetails(List<SchedulerEventDetailsDto> model);
        Task<ApiResult<CreateViewTimesheetResponseDto>> CreateOrViewTimeSheets(CreateViewTimesheetRequestDto model);
        Task<ApiResult<List<SchedulerEventDetailsDto>>> GetSchedulerEventDetails(Guid id); 
        Task<ApiResult<List<SchedulerEventDetailsDto>>> GetSchedulerEventDetails(CreateViewTimesheetRequestDto model); 
        Task<ApiResult<CreateViewTimesheetResponseDto>> GetSchedulerEvent(Guid id);
        Task<ApiResult<bool>> ApproveRequest(CreateViewTimesheetResponseDto model);
        Task<ApiResult<bool>> DeclineRequest(CreateViewTimesheetResponseDto model);
    }
}
