using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
//using AddOptimization.Inkwork.Contracts.Interfaces;
//using AddOptimization.Contracts.Constants;
//using AddOptimization.Inkwork.Contracts.Models;

namespace AddOptimization.Services.Services;

public class AddressService : IAddressService
{
    private readonly IGenericRepository<Address> _addressRepository;
   // private readonly IGenericRepository<Order> _orderRepository;
    private readonly ILogger<AddressService> _logger;
    private readonly IMapper _mapper;

    public AddressService(IGenericRepository<Address> addressRepository, ILogger<AddressService> logger, IMapper mapper)//, IGenericRepository<Order> orderRepository)
    {
        _addressRepository = addressRepository;
        _logger = logger;
        _mapper = mapper;
       // _orderRepository = orderRepository;
    }


    public async Task<ApiResult<AddressDto>> Create(AddressCreateDto model)
    {
        try
        {
            var entity = _mapper.Map<Address>(model);
            entity = await _addressRepository.InsertAsync(entity);
            var mappedEntity = _mapper.Map<AddressDto>(entity);
            return ApiResult<AddressDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<List<AddressDto>>> BulkCreate(List<AddressCreateDto> model)
    {
        try
        {
            var entities = new List<Address>();
            foreach (var item in model)
            {
                var entity = _mapper.Map<Address>(item);
                entities.Add(entity);
            }
            entities = await _addressRepository.BulkInsertAsync(entities);
            var mappedEntities = _mapper.Map<List<AddressDto>>(entities);
            return ApiResult<List<AddressDto>>.Success(mappedEntities);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<AddressDto>> Update(Guid id, AddressCreateDto model)
    {
        try
        {
            var entity = await _addressRepository.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return ApiResult<AddressDto>.NotFound("Address");
            }
            _mapper.Map(model, entity);
            await _addressRepository.UpdateAsync(entity);
            var mappedEntity = _mapper.Map<AddressDto>(entity);
            //try
            //{
            //    var orderEntities = (await _orderRepository.QueryAsync(e => e.ShippingAddressId == id)).ToList();
            //    if (orderEntities.Any())
            //    {
            //        orderEntities.ForEach(e =>
            //        {
            //            e.ShippingAddress = _mapper.Map<ShippingAddress>(entity);
            //        });
            //        await _orderRepository.BulkUpdateAsync(orderEntities);
            //    }
            //}
            //catch (Exception)
            //{
            //    return ApiResult<AddressDto>.Success(mappedEntity,"Address updated but not able to update existing orders data");
            //}
            return ApiResult<AddressDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> Delete(Guid id)
    {
        try
        {
            var entity = await _addressRepository.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return ApiResult<bool>.NotFound("Address");
            }
            entity.IsDeleted = true;
            await _addressRepository.UpdateAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}
