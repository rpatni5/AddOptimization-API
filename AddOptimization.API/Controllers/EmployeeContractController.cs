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
        public async Task<IActionResult> Search()
        {
            try
            {
                var retVal = await _contractService.Search();
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
