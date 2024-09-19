using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;

namespace AddOptimization.API.Controllers
{

    [Authorize]
    public class SavedSearchesController : CustomApiControllerBase
    {
        private readonly ISavedSearchService _savedSearchService;

        public SavedSearchesController(ILogger<SavedSearchesController> logger, ISavedSearchService savedSearchService) : base(logger)
        {
            _savedSearchService = savedSearchService;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SavedSearchDto model)
        {
            try
            {
                var result = await _savedSearchService.Create(model);
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SavedSearchDto model)
        {
            try
            {
                var result = await _savedSearchService.Update(id, model);
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
                var result = await _savedSearchService.Delete(id);
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("currentUserSearches/{searchScreen}")]
        public async Task<IActionResult> GetCurrentUserSearches(string searchScreen)
        {
            try
            {
                var result = await _savedSearchService.GetCurrentUserSearches(searchScreen);
                return HandleResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
