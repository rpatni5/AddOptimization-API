using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class AbsenceRequestController : CustomApiControllerBase
    {
        private readonly IAbsenceRequestService _absenceRequestService;
        public AbsenceRequestController(ILogger<AbsenceRequestController> logger, IAbsenceRequestService absenceRequestService) : base(logger)
        {
            _absenceRequestService = absenceRequestService;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AbsenceRequestRequestDto model)
        {
            try
            {
                var retVal = await _absenceRequestService.Create(model);
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
                var retVal = await _absenceRequestService.Get(id);
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
                var retVal = await _absenceRequestService.Search(filters);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, AbsenceRequestRequestDto model)
        {
            try
            {
                var retVal = await _absenceRequestService.Update(id, model);
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
                var retVal = await _absenceRequestService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("getAll/{startDate}/{endDate}")]
        public async Task<IActionResult> GettotalDuration(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var retVal = await _absenceRequestService.GetDurations(startDate,endDate); 
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}
