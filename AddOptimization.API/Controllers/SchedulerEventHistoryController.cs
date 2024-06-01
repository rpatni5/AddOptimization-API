using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.Contracts.Services;
using AddOptimization.API.Common;

namespace AddOptimization.API.Controllers
{

    [Authorize]
    public class SchedulerEventHistoryController : CustomApiControllerBase
    {
        private readonly ISchedulerEventHistory _schedulerEventHistory;
        public SchedulerEventHistoryController(ILogger<SchedulerEventHistoryController> logger, ISchedulerEventHistory schedulerEventHistory) : base(logger)
        {
            _schedulerEventHistory = schedulerEventHistory;
        }

        [HttpGet("get-history/{id}")]
        public async Task<IActionResult> GetSchedulerEventHistory(Guid id)
        {
            try
            {
                var retVal = await _schedulerEventHistory.GetSchedulerEventHistory(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

       

    }
}

