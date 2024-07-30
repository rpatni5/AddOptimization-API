using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Data.Repositories;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace AddOptimization.Services.Services
{
    public class CustomerEmployeeAssociationService : ICustomerEmployeeAssociationService
    {
        private readonly IGenericRepository<CustomerEmployeeAssociation> _customerEmployeeAssociationRepository;
        private readonly IGenericRepository<EmployeeContract> _contractRepository;
        private readonly IGenericRepository<Employee> _employeeRepository;
        private readonly ILogger<CustomerEmployeeAssociationService> _logger;
        private readonly IMapper _mapper;


        public CustomerEmployeeAssociationService(IGenericRepository<CustomerEmployeeAssociation> customerEmployeeAssociationRepository, ILogger<CustomerEmployeeAssociationService> logger, IGenericRepository<EmployeeContract> contractRepository, IMapper mapper, IGenericRepository<Employee> employeeRepository)
        {
            _customerEmployeeAssociationRepository = customerEmployeeAssociationRepository;
            _logger = logger;
            _mapper = mapper;
            _contractRepository = contractRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<ApiResult<CustomerEmployeeAssociationDto>> Create(CustomerEmployeeAssociationDto model)
        {
            try
            {
                CustomerEmployeeAssociationDto mappedEntity = null;
                if (model.Id != null)
                {
                    var existingAssociation = await _customerEmployeeAssociationRepository.FirstOrDefaultAsync(x => x.Id == model.Id);
                    if (existingAssociation != null)
                    {
                        existingAssociation.ApproverId = model.ApproverId;
                        existingAssociation.DailyWeightage = model.DailyWeightage;
                        existingAssociation.PublicHoliday = model.PublicHoliday;
                        existingAssociation.Saturday = model.Saturday;
                        existingAssociation.Sunday = model.Sunday;
                        existingAssociation.Overtime = model.Overtime;
                        await _customerEmployeeAssociationRepository.UpdateAsync(existingAssociation);
                    }
                    else
                    {
                        return ApiResult<CustomerEmployeeAssociationDto>.Failure(ValidationCodes.NotFound, ValidationErrorMessage.CustomerEmployeeAssociationNotExist);
                    }
                }
                else
                {

                    var isExists = await _customerEmployeeAssociationRepository.IsExist(t => t.CustomerId == model.CustomerId && t.EmployeeId == model.EmployeeId && !t.IsDeleted, ignoreGlobalFilter: true);
                    if (isExists)
                    {
                        return ApiResult<CustomerEmployeeAssociationDto>.Failure(ValidationCodes.CustomerEmployeeAssociationAlreadyExists, ValidationErrorMessage.CustomerEmployeeAssociationExist);
                    }

                    CustomerEmployeeAssociation entity = new CustomerEmployeeAssociation();
                    _mapper.Map(model, entity);
                    await _customerEmployeeAssociationRepository.InsertAsync(entity);
                    mappedEntity = _mapper.Map<CustomerEmployeeAssociationDto>(entity);
                }
                return ApiResult<CustomerEmployeeAssociationDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<CustomerEmployeeAssociationDto>>> Search()
        {
            try
            {
                var entities = await _customerEmployeeAssociationRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.Approver).Include(e => e.Customer).Include(e => e.ApplicationUser).Include(e => e.Contracts));

                var mappedEntities = _mapper.Map<List<CustomerEmployeeAssociationDto>>(entities.ToList());
                foreach (var entity in mappedEntities)
                {
                    entity.IsExternal = (await _employeeRepository.FirstOrDefaultAsync(e => e.UserId == entity.EmployeeId))?.IsExternal;
                }
                return ApiResult<List<CustomerEmployeeAssociationDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<PagedApiResult<CustomerEmployeeAssociationDto>> SearchAllAssociations(PageQueryFiterBase filter)
        {
            try
            {
                var entities = await _customerEmployeeAssociationRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.Approver).Include(e => e.Customer).Include(e => e.ApplicationUser).Include(e => e.Contracts));

                entities = ApplySorting(entities, filter?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filter);


                var pagedResult = PageHelper<CustomerEmployeeAssociation, CustomerEmployeeAssociationDto>.ApplyPaging(entities, filter, entities => entities.Select(e => new CustomerEmployeeAssociationDto
                {
                    Id = e.Id,
                    CustomerId = e.CustomerId,
                    EmployeeId = e.EmployeeId,
                    ApproverId =    e.ApproverId,
                    DailyWeightage = e.DailyWeightage,
                    Overtime = e.Overtime,
                    PublicHoliday = e.PublicHoliday,
                    Sunday = e.Sunday,
                    Saturday = e.Saturday,
                    CustomerName = e.Customer.Organizations,
                    EmployeeName = e.ApplicationUser.FullName,
                    ApproverName = e.Approver.FullName,
                    HasContract = e.Contracts.Count() > 0,

                }).ToList());
                var result = pagedResult;
                foreach (var entity in result.Result)
                {
                    entity.IsExternal = (await _employeeRepository.FirstOrDefaultAsync(e => e.UserId == entity.EmployeeId))?.IsExternal;
                }
                return PagedApiResult<CustomerEmployeeAssociationDto>.Success(result);
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
                var entity = await _customerEmployeeAssociationRepository.FirstOrDefaultAsync(t => t.Id == id);
                var contractData = await _contractRepository.FirstOrDefaultAsync(t => t.EmployeeAssociationId == id);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Association");
                }
                entity.IsDeleted = true;
                await _customerEmployeeAssociationRepository.UpdateAsync(entity);
                if (contractData != null)
                {
                    contractData.IsDeleted = true;
                    await _contractRepository.UpdateAsync(contractData);
                }
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<CustomerEmployeeAssociationDto>>> GetAssociatedCustomers(int employeeId)
        {
            try
            {
                var associations = await _customerEmployeeAssociationRepository.QueryAsync(e => e.EmployeeId == employeeId && !e.IsDeleted, include: entities => entities.Include(e => e.Customer).Include(e => e.Approver).Include(e => e.ApplicationUser));
                var mappedEntities = _mapper.Map<List<CustomerEmployeeAssociationDto>>(associations);
                return ApiResult<List<CustomerEmployeeAssociationDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<CustomerEmployeeAssociationDto>> Get(Guid id)
        {
            try
            {
                var entity = await _customerEmployeeAssociationRepository.FirstOrDefaultAsync(t => t.Id == id, include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.Approver).Include(e => e.Customer).Include(e => e.ApplicationUser), ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<CustomerEmployeeAssociationDto>.NotFound("association");
                }
                var mappedEntity = _mapper.Map<CustomerEmployeeAssociationDto>(entity);
                return ApiResult<CustomerEmployeeAssociationDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        private IQueryable<CustomerEmployeeAssociation> ApplyFilters(IQueryable<CustomerEmployeeAssociation> entities, PageQueryFiterBase filter)
        {
            filter.GetValue<string>("customerId", (v) =>
            {
                entities = entities.Where(e => e.CustomerId.ToString() == v);
            });

            filter.GetValue<string>("customerName", (v) =>
            {
                entities = entities.Where(e => e.Customer != null && e.Customer.Organizations.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<string>("employeeName", (v) =>
            {
                entities = entities.Where(e => e.ApplicationUser != null && e.ApplicationUser.FullName.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<string>("approverName", (v) =>
            {
                entities = entities.Where(e => e.Approver != null && e.Approver.FullName.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<string>("dailyWeightage", (v) =>
            {
                if (!string.IsNullOrEmpty(v))
                {
                    entities = entities.Where(e => e.DailyWeightage == Convert.ToInt32(v));
                }
            });

            return entities;
        }
        private IQueryable<CustomerEmployeeAssociation> ApplySorting(IQueryable<CustomerEmployeeAssociation> orders, SortModel sort)
        {
            try
            {
                if (sort?.Name == null)
                {
                    orders = orders.OrderByDescending(o => o.CreatedAt);
                    return orders;
                }
                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {
                    if (columnName.ToUpper() == nameof(CustomerEmployeeAssociationDto.DailyWeightage).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.DailyWeightage);
                    }
                    if (columnName.ToUpper() == nameof(CustomerEmployeeAssociationDto.CustomerName).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Customer.Organizations);
                    }
                    if (columnName.ToUpper() == nameof(CustomerEmployeeAssociationDto.EmployeeName).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.ApplicationUser.FullName);
                    }
                    if (columnName.ToUpper() == nameof(CustomerEmployeeAssociationDto.ApproverName).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Approver.FullName);
                    }
                }
                else
                {
                    if (columnName.ToUpper() == nameof(CustomerEmployeeAssociationDto.DailyWeightage).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.DailyWeightage);
                    }
                    if (columnName.ToUpper() == nameof(CustomerEmployeeAssociationDto.CustomerName).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.Customer.Organizations);
                    }
                    if (columnName.ToUpper() == nameof(CustomerEmployeeAssociationDto.EmployeeName).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.ApplicationUser.FullName);
                    }
                    if (columnName.ToUpper() == nameof(CustomerEmployeeAssociationDto.ApproverName).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.Approver.FullName);
                    }

                }
                return orders;

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return orders;
            }
        }


    }
}

