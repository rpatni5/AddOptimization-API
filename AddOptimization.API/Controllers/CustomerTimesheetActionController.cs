using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Contracts.Dto;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Services;

namespace AddOptimization.API.Controllers
{
    [AllowAnonymous]
    public class CustomerTimesheetActionController : CustomApiControllerBase
    {
        private readonly ISchedulerEventService _schedulerEventService;
        private readonly ISchedulerEventHistory _schedulerEventHistoryService;
        private readonly CustomDataProtectionService _customDataProtectionService;

        public CustomerTimesheetActionController(ILogger<SchedulerEventController> logger, ISchedulerEventService schedulerEventService, ISchedulerEventHistory schedulerEventHistoryService, CustomDataProtectionService customDataProtectionService) : base(logger)
        {
            _schedulerEventService = schedulerEventService;
            _customDataProtectionService = customDataProtectionService;
            _schedulerEventHistoryService = schedulerEventHistoryService;
        }

        [HttpGet("timesheet-event-details/{id}")]
        public async Task<IActionResult> GetTimesheetEventDetails(string id)
        {
            try
            {
                var eventId = new Guid(_customDataProtectionService.Decode(id));
                var retVal = await _schedulerEventService.GetSchedulerEventDetails(eventId, false);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("timesheet-event/{id}")]
        public async Task<IActionResult> GetTimesheetEvent(string id)
        {
            try
            {
                var eventId = new Guid(_customDataProtectionService.Decode(id));
                var retVal = await _schedulerEventService.GetSchedulerEvent(eventId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("timesheet-action")]
        public async Task<IActionResult> TimesheetAction(CustomerTimesheetActionDto model)
        {
            try
            {
                var retVal = await _schedulerEventService.TimesheetAction(model);
                return HandleResponse(retVal);

            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-scheduler-history/{id}")]
        public async Task<IActionResult> GetSchedulerEventHistory(string id)
        {
            try
            {
                var eventId = new Guid(_customDataProtectionService.Decode(id));
                var retVal = await _schedulerEventHistoryService.GetSchedulerEventHistory(eventId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}
