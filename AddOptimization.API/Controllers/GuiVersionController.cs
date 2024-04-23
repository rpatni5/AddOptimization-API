using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class GuiVersionController : CustomApiControllerBase
    {
        private readonly IGuiVersionService _guiVersionService;
        public GuiVersionController(ILogger<GuiVersionController> logger, IGuiVersionService guiVersionService) : base(logger)
        {
            _guiVersionService = guiVersionService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] GuiVersionCreateDto model)
        {
            try
            {
                var retVal = await _guiVersionService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}
