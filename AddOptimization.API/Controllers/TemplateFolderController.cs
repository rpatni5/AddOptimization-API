using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AddOptimization.Contracts.Services;
using AddOptimization.API.Common;
using AddOptimization.Services.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Constants;

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
        [HttpPost]
        public async Task<IActionResult> Create(TemplateFolderDto model)
        {
            try
            {
                var retVal = await _folderService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
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

        [HttpPost("search")]
        public async Task<IActionResult> Search()
        {
            try
            {
                var result = await _folderService.GetAllTemplateFolders();
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, TemplateFolderDto model)
        {
            try
            {
                var retVal = await _folderService.Update(id, model);
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
                var retVal = await _folderService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-by-folderId/{id}")]
        public async Task<IActionResult> GetTemplates(Guid id)
        {
            try
            {
                var retVal = await _folderService.GetTemplates(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}

