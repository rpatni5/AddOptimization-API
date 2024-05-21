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
        private readonly CustomDataProtectionService _customDataProtectionService;

        public CustomerTimesheetActionController(ILogger<SchedulerEventController> logger, ISchedulerEventService schedulerEventService, CustomDataProtectionService customDataProtectionService) : base(logger)
        {
            _schedulerEventService = schedulerEventService;
            _customDataProtectionService = customDataProtectionService;
        }

        [HttpGet("timesheet-event-details/{id}")]
        public async Task<IActionResult> GetTimesheetEventDetails(string id)
        {
            try
            {
                var eventId = new Guid(_customDataProtectionService.Decode(id));
                var retVal = await _schedulerEventService.GetSchedulerEventDetails(eventId);
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
        public async Task<IActionResult> ApproveTimesheetRequest(CustomerTimesheetActionDto model)
        {
            try
            {
                var retVal = await _schedulerEventService.ApprovedTimesheetByCustomer(model);
                return HandleResponse(retVal);

            }
            catch(Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}
