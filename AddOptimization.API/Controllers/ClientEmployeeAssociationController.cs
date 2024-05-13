using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class ClientEmployeeAssociationController : CustomApiControllerBase
    {
        private readonly IClientEmployeeAssociationService _clientEmployeeAssociationService;
        public ClientEmployeeAssociationController(ILogger<ClientEmployeeAssociationController> logger, IClientEmployeeAssociationService clientEmployeeAssociationService) : base(logger)
        {
            _clientEmployeeAssociationService = clientEmployeeAssociationService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ClientEmployeeAssociationDto model)
        {
            try
            {
                var retVal = await _clientEmployeeAssociationService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Get([FromBody] ClientEmployeeAssociationDto filter)
        {
            try
            {
                var retVal = await _clientEmployeeAssociationService.Search();
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
                var retVal = await _clientEmployeeAssociationService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
