using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class HolidayAllocationController : CustomApiControllerBase
    {
        private readonly IHolidayAllocationService _holidayAllocationService;
        public HolidayAllocationController(ILogger<HolidayAllocationController> logger, IHolidayAllocationService holidayAllocationService) : base(logger)
        {
            _holidayAllocationService = holidayAllocationService;
        }
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] PageQueryFiterBase filter)
        {
            try
            {
                var retVal = await _holidayAllocationService.Search(filter);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HolidayAllocationRequestDto model)
        {
            try
            {
                var retVal = await _holidayAllocationService.Create(model);
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
                var retVal = await _holidayAllocationService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-allocated-holiday/{employeeId}")]
        public async Task<IActionResult> GetAllocatedHolidays(int employeeId)
        {
            try
            {
                var retVal = await _holidayAllocationService.GetAllocatedHolidays(employeeId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("get-holiday-balanced/{employeeId}")]
        public async Task<IActionResult> GetEmployeeLeaveBalance(int employeeId)
        {
            try
            {
                var retVal = await _holidayAllocationService.GetEmployeeLeaveBalance(employeeId);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }



    }
}
