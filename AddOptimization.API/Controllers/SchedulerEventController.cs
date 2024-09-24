using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Contracts.Dto;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using AddOptimization.Services.Services;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class SchedulerEventController : CustomApiControllerBase
    {
        private readonly ISchedulerEventService _schedulerEventService;
        public SchedulerEventController(ILogger<SchedulerEventController> logger, ISchedulerEventService schedulerEventService) : base(logger)
        {
            _schedulerEventService = schedulerEventService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] PageQueryFiterBase filters)
        {
            try

            {
                var retVal = await _schedulerEventService.Search(filters);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] List<SchedulerEventDetailsDto> schedulerEventDetails)
        {
            try
            {
                var retVal = await _schedulerEventService.Save(schedulerEventDetails);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("submit-event-details")]
        public async Task<IActionResult> SubmitEventDetails([FromBody] List<SchedulerEventDetailsDto> model)
        {
            try
            {
                var retVal = await _schedulerEventService.SubmitEventDetails(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var retVal = await _schedulerEventService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("create-timesheet")]
        public async Task<IActionResult> CreateOrViewTimeSheets([FromBody] SchedulerEventRequestDto model)
        {
            try
            {
                var retVal = await _schedulerEventService.CreateOrViewTimeSheets(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("scheduler-event-details/{id}")]
        public async Task<IActionResult> GetSchedulerEventDetails(Guid id)
        {
            try
            {
                var retVal = await _schedulerEventService.GetSchedulerEventDetails(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("scheduler-event-details")]
        public async Task<IActionResult> GetSchedulerEventDetails([FromBody] SchedulerEventRequestDto model)
        {
            try
            {
                var retVal = await _schedulerEventService.GetSchedulerEventDetails(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("scheduler-event/{id}")]
        public async Task<IActionResult> GetSchedulerEvent(Guid id)
        {
            try
            {
                var retVal = await _schedulerEventService.GetSchedulerEvent(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpPost("approve-request")]
        public async Task<IActionResult> ApproveRequest(AccountAdminActionRequestDto model)
        {
            try
            {
                var retVal = await _schedulerEventService.ApproveRequest(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("decline-request")]
        public async Task<IActionResult> DeclineRequest(AccountAdminActionRequestDto model)
        {
            try
            {
                var retVal = await _schedulerEventService.DeclineRequest(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("send-email-to-customer/{schedulerEventId}")]
        public async Task<IActionResult> SendTimesheetApprovalEmailToCustomer(Guid schedulerEventId)
        {
            try
            {
                var retVal = await _schedulerEventService.SendTimesheetApprovalEmailToCustomer(schedulerEventId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("{send-to-draft}/{timesheetid}")]
        public async Task<IActionResult> SendToDraft( Guid timesheetid)
        {
            try
            {
                var result = await _schedulerEventService.SendToDraft(timesheetid);
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
