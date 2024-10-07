using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
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

        [HttpGet("get-All-Items")]
        public async Task<IActionResult> GetAllItems()
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
