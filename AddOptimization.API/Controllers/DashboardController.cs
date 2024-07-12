using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class DashboardController : CustomApiControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(ILogger<DashboardController> logger, IDashboardService dashboardService) : base(logger)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("dashboard-details/{id}")]
        public async Task<IActionResult> GetAllDashboardDetail(string id)
        {
            try
            {
               
                var retVal = await _dashboardService.GetAllDashboardDetail(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
