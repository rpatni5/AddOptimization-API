using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.Contracts.Services;
using AddOptimization.API.Common;

namespace AddOptimization.API.Controllers
{

    [Authorize]
    public class TemplateController : CustomApiControllerBase
    {
        private readonly ITemplatesService _templateService;
        public TemplateController(ILogger<TemplateController> logger, ITemplatesService templateService) : base(logger)
        {
            _templateService = templateService;
        }

       
        [HttpGet("get-template")]
        public async Task<IActionResult> GetAllTemplate()
        {
            try
            {
                var retVal = await _templateService.GetAllTemplate();
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-template-by-id/{id}")]
        public async Task<IActionResult> GetTemplateById(Guid id)
        {
            try
            {
                var retVal = await _templateService.GetTemplateById(id);
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
                var retVal = await _templateService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}

