﻿using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.API.Controllers
{
    [Authorize]
    public class PublicHolidayController : CustomApiControllerBase
    {

        private readonly IPublicHolidayService _timesheetService;
        public PublicHolidayController(ILogger<PublicHolidayController> logger, IPublicHolidayService timesheetService) : base(logger)
        {
            _timesheetService = timesheetService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search()
        {
            try
            {
                var retVal = await _timesheetService.Search();
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PublicHolidayDto model)
        {
            try
            {
                var retVal = await _timesheetService.Create(model);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            try
            {
                var retVal = await _timesheetService.Get(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PublicHolidayDto model)
        {
            try
            {
                var retVal = await _timesheetService.Update(id, model);
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
                var retVal = await _timesheetService.Delete(id);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpGet("get-by-countryid")]
        public async Task<IActionResult> GetByCountryId(Guid countryid)
        {
            try
            {
                var retVal = await _timesheetService.GetByCountryId(countryid);
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpGet("get-all-countries")]
        public async Task<IActionResult> GetAllCountry()
        {
            try
            {
                var retVal = await _timesheetService.GetAllCountry();
                return HandleResponse(retVal);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

    }
}