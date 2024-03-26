using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Contracts.Constants;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AddOptimization.Services.Services;
public class CustomerService : ICustomerService
{
    private readonly IGenericRepository<Customer> _customerRepository;
   // private readonly IGenericRepository<License> _licenseRepository;
    private readonly IGenericRepository<CustomerStatus> _customerStatusRepository;
    private readonly ILogger<CustomerService> _logger;
    private readonly IMapper _mapper;
    private readonly IAddressService _addressService;
    private readonly IUnitOfWork _unitOfWork;
    public CustomerService(IGenericRepository<Customer> customerRepository, ILogger<CustomerService> logger, IMapper mapper, 
        IAddressService addressService, IUnitOfWork unitOfWork, IGenericRepository<CustomerStatus> customerStatusRepository)//, IGenericRepository<Order> orderRepository)
    {
        _customerRepository = customerRepository;
        _logger = logger;
        _mapper = mapper;
        _addressService = addressService;
        _unitOfWork = unitOfWork;
        _customerStatusRepository = customerStatusRepository;
       // _orderRepository = orderRepository;
    }
    public async Task<ApiResult<List<CustomerSummaryDto>>> GetSummary(PageQueryFiterBase filter)
    {
        try
        {
            var includeDeleted = false;
            filter.GetValue<bool>("includeInactive", v => includeDeleted = v);
            var entities = await _customerRepository.QueryMappedAsync(s => new CustomerSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
            }, e => includeDeleted || (e.CustomerStatus != null && e.CustomerStatus.Name != CustomerStatuses.Inactive), orderBy:(entities)=> entities.OrderBy(c=> c.Name));
            return ApiResult<List<CustomerSummaryDto>>.Success(entities.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<List<CustomerDto>>> Search(PageQueryFiterBase filter)
    {
        try
        {
            var entities = await _customerRepository.QueryAsync(include:entities=> entities
            .Include(e=> e.CustomerStatus).Include(e=>e.Licenses),orderBy: (entities) => entities.OrderBy(t => t.Name));
            var mappedEntities = _mapper.Map<List<CustomerDto>>(entities.ToList());
            return ApiResult<List<CustomerDto>>.Success(mappedEntities);
        }
        catch (Exception ex) 
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<CustomerDetailsDto>> Get(Guid id,bool includeOrderStats)
    {
        try
        {
            var entity = await _customerRepository.FirstOrDefaultAsync(t => t.Id == id,include:entity=> entity.Include(e=> e.Addresses.Where(a=> !a.IsDeleted).OrderByDescending(e=> e.CreatedAt)).Include(e => e.CustomerStatus));
            if (entity == null)
            {
                return ApiResult<CustomerDetailsDto>.NotFound("Customer");
            }
            var mappedEntity=_mapper.Map<CustomerDetailsDto>(entity);
            if (includeOrderStats)
            {
                //var ordersData = (await _orderRepository.QueryMappedAsync(e => new {
                //    e.CreatedAt,
                //    OrderAmount = e.Totals == null ? 0 : e.Totals.total
                //}, e => e.CustomerId == id)).ToList();
                //if (ordersData.Any())
                //{
                //    mappedEntity.AverageOrderAmount = (int)Math.Ceiling((double)(ordersData.Sum(e => e.OrderAmount) / ordersData.Count));
                //    mappedEntity.OrderCount = ordersData.Count;
                //    mappedEntity.LastOrdered = ordersData.OrderByDescending(e => e.CreatedAt).FirstOrDefault()?.CreatedAt;
                //}
            }
            return ApiResult<CustomerDetailsDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<CustomerDto>> Create(CustomerCreateDto model)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var isExists = await _customerRepository.IsExist(t => t.Name.ToLower() == model.Name.ToLower() || (t.Email!= null && t.Email.ToLower() == model.Email.ToLower()));
            if (isExists)
            {
                return ApiResult<CustomerDto>.EntityAlreadyExists("Customer", "name or email");
            }
            var entity = _mapper.Map<Customer>(model);
            var billingAddressId = entity.BillingAddressId;
            entity.BillingAddressId = null;
            entity = await _customerRepository.InsertAsync(entity);
            if (model.Addresses.Any())
            {
                model.Addresses.ForEach(a =>
                {
                    a.CustomerId = entity.Id;
                });
                await _addressService.BulkCreate(model.Addresses);
                if(billingAddressId != null)
                {
                    entity.BillingAddressId = billingAddressId;
                    await _customerRepository.UpdateAsync(entity);
                }
            }
            await _unitOfWork.CommitTransactionAsync();
            var mappedEntity = _mapper.Map<CustomerDto>(entity);
            return ApiResult<CustomerDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<CustomerDto>> Update(Guid id, CustomerCreateDto model)
    {
        try
        {
            var isExists = await _customerRepository.IsExist(t => t.Id != id && t.Name.ToLower() == model.Name.ToLower());
            if (isExists)
            {
                return ApiResult<CustomerDto>.EntityAlreadyExists("Customer", "name");
            }

            var entity = await _customerRepository.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return ApiResult<CustomerDto>.NotFound("Customer");
            }
            _mapper.Map(model, entity);
            await _customerRepository.UpdateAsync(entity);
            var mappedEntity = _mapper.Map<CustomerDto>(entity);
            return ApiResult<CustomerDto>.Success(mappedEntity);
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
            var entity = await _customerRepository.FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return ApiResult<bool>.NotFound("Customer");
            }
            var inActiveStatus =await _customerStatusRepository.FirstOrDefaultAsync(e => e.Name == CustomerStatuses.Inactive);
            if (inActiveStatus == null)
            {
                return ApiResult<bool>.NotFound("Customer Status");
            }
            entity.CustomerStatusId=inActiveStatus.Id;
            await _customerRepository.UpdateAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    //public async Task<ApiResult<List<CustomerOrderDto>>> GetOrders(Guid customerId)
    //{
    //    try
    //    {
    //        var entities = await _orderRepository.QueryMappedAsync(e=> new CustomerOrderDto
    //        {
    //            Id=e.Id,
    //            InvoiceNumber= e.InvoiceNumber,
    //            Duedate=e.Duedate,
    //            OrderStatusId=e.OrderStatusId,
    //            OrderStatusName=e.OrderStatus.Name,
    //            UserId=e.UserId,
    //            UserFullName=e.UserFullName,
    //            ShippingAddressString = e.ShippingAddress==null?null:e.ShippingAddress.address1+","+e.ShippingAddress.zip + "," + e.ShippingAddress.city,
    //            City = e.ShippingAddress==null?null: e.ShippingAddress.city,
    //            Total = e.Totals==null ? null :e.Totals.total/100
    //        },e=> e.CustomerId==customerId,include: source => source.Include(o => o.OrderStatus),orderBy:entities=> entities.OrderByDescending(o=> o.CreatedAt));
        
    //        return ApiResult<List<CustomerOrderDto>>.Success(entities.ToList());
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogException(ex);
    //        throw;
    //    }
    //}

   
}
