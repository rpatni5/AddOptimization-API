using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;

namespace CustomersManagment.API.Controllers;

[Authorize]
public class AddressesController : CustomApiControllerBase
{
    private readonly IAddressService _addressesService;

    public AddressesController(ILogger<AddressesController> logger, IAddressService addressesService) : base(logger)
    {
        _addressesService = addressesService;
    }


    [HttpPost]
    public async Task<IActionResult> Create(AddressCreateDto model)
    {
        try
        {
            var retVal = await _addressesService.Create(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, AddressCreateDto model)
    {
        try
        {
            var retVal = await _addressesService.Update(id, model);
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
            var retVal = await _addressesService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}
