using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services;

public interface IAddressService
{
    Task<ApiResult<AddressDto>> Update(Guid id, AddressCreateDto model);
    Task<ApiResult<AddressDto>> Create(AddressCreateDto model);
    Task<ApiResult<bool>> Delete(Guid id);
    Task<ApiResult<List<AddressDto>>> BulkCreate(List<AddressCreateDto> model);
}
