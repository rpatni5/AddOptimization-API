using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using AddOptimization.Contracts.Dto;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class SchedulersController : CustomApiControllerBase
    {
        private readonly ISchedulersService _schedulersService;
        public SchedulersController(ILogger<SchedulersController> logger, ISchedulersService schedulersService) : base(logger)
        {
            _schedulersService = schedulersService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromBody] PageQueryFiterBase filters)
        {
            try
            {
                var retVal = await _schedulersService.Search( filters);
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
                var retVal = await _schedulersService.Upsert( model);
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
                var retVal = await _schedulersService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}
