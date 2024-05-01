using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Contracts.Dto;

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

        [HttpGet("search")]
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

        [HttpPost("upsert")]
        public async Task<IActionResult> Upsert( [FromBody] List<SchedulersDto> model)
        {
            try
            {
                var retVal = await _schedulerEventService.Upsert( model);
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

    }
}
