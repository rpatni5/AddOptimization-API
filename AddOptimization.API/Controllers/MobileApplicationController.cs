using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    public class MobileApplicationController : CustomApiControllerBase
    {
        private readonly IMobileApplicationService _mobileAppService;
        public MobileApplicationController(ILogger<MobileApplicationController> logger, IMobileApplicationService mobileAppService) : base(logger)
        {
            _mobileAppService = mobileAppService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(TemplateEntryDto model)
        {
            try
            {
                var retVal = await _mobileAppService.SaveMobileAppDetails(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-by-mobileAppId/{id}")]
        public async Task<IActionResult> GetMobileAppById(Guid id)
        {
            try
            {
                var retVal = await _mobileAppService.GetById(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search()
        {
            try
            {
                var result = await _mobileAppService.Search();
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TemplateEntryDto model)
        {
            try
            {
                var retVal = await _mobileAppService.Update(id, model);
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
                var retVal = await _mobileAppService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
