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
                var retVal = await _schedulerEventService.Search( filters);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save( [FromBody] List<SchedulerEventDetailsDto> model)
        {
            try
            {
                var retVal = await _schedulerEventService.Save( model);
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
        public async Task<IActionResult> CreateOrViewTimeSheets([FromBody] CreateViewTimesheetRequestDto model)
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

        [HttpGet("schedular-event-details/{id}")]
        public async Task<IActionResult> GetSchedularEventDetails(Guid id)
        {
            try
            {
                var retVal = await _schedulerEventService.GetSchedularEventDetails(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("schedular-event/{id}")]
        public async Task<IActionResult> GetSchedularEvent(Guid id)
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

    }
}
