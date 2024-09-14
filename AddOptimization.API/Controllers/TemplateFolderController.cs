using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.Contracts.Services;
using AddOptimization.API.Common;

namespace AddOptimization.API.Controllers
{

    [Authorize]
    public class TemplateFolderController : CustomApiControllerBase
    {
        private readonly ITemplateFolderService _folderService;
        public TemplateFolderController(ILogger<TemplateFolderController> logger, ITemplateFolderService folderService) : base(logger)
        {
            _folderService = folderService;
        }

       
        [HttpGet("get-folders")]
        public async Task<IActionResult> GetAllFolders()
        {
            try
            {
                var retVal = await _folderService.GetAllTemplateFolders();
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}

