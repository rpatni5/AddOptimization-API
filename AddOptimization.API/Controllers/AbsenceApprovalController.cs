using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;
using GraphQL;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class AbsenceApprovalController : CustomApiControllerBase
    {
        private readonly IAbsenceApprovalService _absenceApprovalService;
        public AbsenceApprovalController(ILogger<AbsenceApprovalController> logger, IAbsenceApprovalService absenceApprovalService) : base(logger)
        {
            _absenceApprovalService = absenceApprovalService;
        }
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] PageQueryFiterBase filters)
        {
            try
            {
                var retVal = await _absenceApprovalService.Search(filters);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("action-request")]
        public async Task<IActionResult> AbsenceAction(AdminApprovalRequestActionDto model)
        {
            try
            {
                var retVal = await _absenceApprovalService.AbsenceAction(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

       


    }
}
