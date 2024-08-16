using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class CustomerEmployeeAssociationController : CustomApiControllerBase
    {
        private readonly ICustomerEmployeeAssociationService _customerEmployeeAssociationService;
        public CustomerEmployeeAssociationController(ILogger<CustomerEmployeeAssociationController> logger, ICustomerEmployeeAssociationService customerEmployeeAssociationService) : base(logger)
        {
            _customerEmployeeAssociationService = customerEmployeeAssociationService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerEmployeeAssociationDto model)
        {
            try
            {
                var retVal = await _customerEmployeeAssociationService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Get([FromBody] CustomerEmployeeAssociationDto filter)
        {
            try
            {
                var retVal = await _customerEmployeeAssociationService.Search();
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
                var retVal = await _customerEmployeeAssociationService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpGet("get-associated-customers/{employeeId}")]
        public async Task<IActionResult> GetAssociatedCustomers(int employeeId)
        {
            try
            {
                var retVal = await _customerEmployeeAssociationService.GetAssociatedCustomers(employeeId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var retVal = await _customerEmployeeAssociationService.Get(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost("search-all")]
        public async Task<IActionResult> SearchALL([FromBody] PageQueryFiterBase filter)
        {
            try
            {
                var retVal = await _customerEmployeeAssociationService.SearchAllAssociations(filter);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-customer/{id}/{empId}")]
        public async Task<IActionResult> GetCustomerById(Guid id ,int empId)
        {
            try
            {
                var retVal = await _customerEmployeeAssociationService.GetCustomerById(id ,empId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
