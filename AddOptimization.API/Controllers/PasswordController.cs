using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    public class PasswordController : CustomApiControllerBase
    {
        private readonly IPasswordService _passwordService;
        public PasswordController(ILogger<PasswordController> logger, IPasswordService passwordService) : base(logger)
        {
            _passwordService = passwordService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(TemplateEntryDto model)
        {
            try
            {
                var retVal = await _passwordService.SavePasswordDetails(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-by-passwordId/{id}")]
        public async Task<IActionResult> GetPasswordById(Guid id)
        {
            try
            {
                var retVal = await _passwordService.GetPasswordById(id);
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
                var result = await _passwordService.Search();
                return HandleResponse(result);
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
                var retVal = await _passwordService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}

