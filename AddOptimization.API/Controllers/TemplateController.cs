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

    }
}

