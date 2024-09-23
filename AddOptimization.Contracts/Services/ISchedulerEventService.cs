using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface ISchedulerEventService
    {
        Task<PagedApiResult<SchedulerEventResponseDto>> Search(PageQueryFiterBase filters);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<bool>> Save(List<SchedulerEventDetailsDto> schedulerEventDetails);
        Task<ApiResult<bool>> SubmitEventDetails(List<SchedulerEventDetailsDto> model);
        Task<ApiResult<SchedulerEventResponseDto>> CreateOrViewTimeSheets(SchedulerEventRequestDto model);
        Task<ApiResult<List<SchedulerEventDetailsDto>>> GetSchedulerEventDetails(Guid id, bool getRoleBasedData = true); 
        Task<ApiResult<List<SchedulerEventDetailsDto>>> GetSchedulerEventDetails(SchedulerEventRequestDto model); 
        Task<ApiResult<SchedulerEventResponseDto>> GetSchedulerEvent(Guid id);
        Task<ApiResult<List<SchedulerEventResponseDto>>> GetSchedulerEventsForEmailReminder(Guid customerId, int userId);
        Task<ApiResult<List<SchedulerEventResponseDto>>> GetSchedulerEventsForApproveEmailReminder();
        Task<ApiResult<bool>> SendNotificationToEmployee(List<SchedulerEventResponseDto> model);
        Task<ApiResult<bool>> ApproveRequest(AccountAdminActionRequestDto model);
        Task<ApiResult<bool>> DeclineRequest(AccountAdminActionRequestDto model);
        Task<ApiResult<bool>> TimesheetAction(CustomerTimesheetActionDto model);
        Task<ApiResult<bool>> SendTimesheetApprovalEmailToCustomer(Guid schedulerEventId);
        Task<bool> IsTimesheetApproved(Guid customerId, List<int> employeeIds, MonthDateRange month);
    }
}
