using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
   [Authorize]
    

    public class LeaveStatusesController : CustomApiControllerBase
    {
        private readonly ILeaveStatusesService _leaveStatusesService;
        public LeaveStatusesController(ILogger<LeaveStatusesController> logger, ILeaveStatusesService  leaveStatusesService) : base(logger)
        {
            _leaveStatusesService = leaveStatusesService;
        }
      


        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] PageQueryFiterBase filters)
        {
            try
            {
                var result = await _leaveStatusesService.Search(filters);
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
