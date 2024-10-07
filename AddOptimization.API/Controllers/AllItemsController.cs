using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    public class AllItemsController : CustomApiControllerBase
    {
        private readonly IAllItemsService _allItemsService;
        public AllItemsController(ILogger<AllItemsController> logger, IAllItemsService allItemsService) : base(logger)
        {
            _allItemsService = allItemsService;
        }
        [HttpPost("search")]
        public async Task<IActionResult> GetAllItems([FromBody] PageQueryFiterBase filter)
        {
            try
            {
                var retVal = await _allItemsService.getAllTemplates();
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}
