using AddOptimization.API.Common;
using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
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

        [HttpPost("search")]
        public async Task<IActionResult> Get([FromBody] PageQueryFiterBase filter)
        {
            try
            {
                var retVal = await _guiVersionService.Search(filter);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            try
            {
                var retVal = await _guiVersionService.Delete(Id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("update-status/{id}")]
        public async Task<IActionResult> ToggleEmailsEnabled(Guid id)
        {
            try
            {
                var retVal = await _guiVersionService.UpdateStatus(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }



        [AllowAnonymous]
        [HttpGet("latest-version")]
        public async Task<IActionResult> GetLatestVersion()
        {
            try
            {
                var retVal = await _guiVersionService.GetLatestversion();
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
