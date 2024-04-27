using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class SchedulerStatusController : CustomApiControllerBase
    {
        private readonly ISchedulersStatusService _schedulersStatusService;
        public SchedulerStatusController(ILogger<SchedulerStatusController> logger, ISchedulersStatusService schedulersStatusService) : base(logger)
        {
            _schedulersStatusService = schedulersStatusService;
        }


        [HttpPost("search")]
        public async Task<IActionResult> Search()
        {
            try
            {
                var result = await _schedulersStatusService.Search();
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
