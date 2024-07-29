using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class EmployeeContractController : CustomApiControllerBase
    {

        private readonly IEmployeeContractService _contractService;
        public EmployeeContractController(ILogger<EmployeeContractController> logger, IEmployeeContractService contractService) : base(logger)
        {
            _contractService = contractService;
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeContractRequestDto model)
        {
            try
            {
                var retVal = await _contractService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-contract-details/{id}")]
        public async Task<IActionResult> GetEmployeeContractById(Guid id)
        {
            try
            {
                var retVal = await _contractService.GetEmployeeContractById(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EmployeeContractRequestDto model)
        {
            try
            {
                var retVal = await _contractService.Update(id, model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("contract/{contractId}")]
        public async Task<IActionResult> SignContract(Guid contractId)
        {
            try
            {
                var retVal = await _contractService.SignContract(contractId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpGet("get-contract/{id}")]
        public async Task<IActionResult> GetEmployeeContractByEmployeeId(int id)
        {
            try
            {
                var retVal = await _contractService.GetEmployeeContractByEmployeeId(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] PageQueryFiterBase filters)
        {
            try
            {
                var retVal = await _contractService.Search(filters);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("contracts/{id}")]
        public async Task<IActionResult> GetContractsByAsscociationId(Guid id)
        {
            try
            {
                var retVal = await _contractService.GetContractsByAsscociationId(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("search-all")]
        public async Task<IActionResult> SearchAllContracts([FromBody] PageQueryFiterBase filters)
        {
            try
            {
                var retVal = await _contractService.SearchAllContracts(filters);
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
                var retVal = await _contractService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("internal-contract")]
        public async Task<IActionResult> CreateInternalContract([FromBody] EmployeeContractRequestDto model)
        {
            try
            {
                var retVal = await _contractService.CreateInternalEmployeeContract(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpGet("internal-contract/{id}")]
        public async Task<IActionResult> GetInternalContractByEmployeeId(int id)
        {
            try
            {
                var retVal = await _contractService.GetInternalContractByEmployeeId(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("contract-employee/{id}")]
        public async Task<IActionResult> GetContractByEmployeeId(int id)
        {
            try
            {
                var retVal = await _contractService.GetContractByEmployeeId(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


    }
}
