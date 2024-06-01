using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using GraphQL;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    public class SchedulerEventTypeController : CustomApiControllerBase
    {
        private readonly ISchedulerEventTypeService _schedulersEventTypeService;
        public SchedulerEventTypeController(ILogger<SchedulerEventTypeController> logger, ISchedulerEventTypeService scheduleEventTypeService) : base(logger)
        {
            _schedulersEventTypeService = scheduleEventTypeService;
        }


        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search()
        {
            try
            {
                var result = await _schedulersEventTypeService.Search();
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
