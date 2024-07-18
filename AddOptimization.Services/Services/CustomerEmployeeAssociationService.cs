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

namespace AddOptimization.Services.Services
{
    public class CustomerEmployeeAssociationService : ICustomerEmployeeAssociationService
    {
        private readonly IGenericRepository<CustomerEmployeeAssociation> _customerEmployeeAssociationRepository;
        private readonly IGenericRepository<EmployeeContract> _contractRepository;
        private readonly ILogger<CustomerEmployeeAssociationService> _logger;
        private readonly IMapper _mapper;


        public CustomerEmployeeAssociationService(IGenericRepository<CustomerEmployeeAssociation> customerEmployeeAssociationRepository, ILogger<CustomerEmployeeAssociationService> logger, IGenericRepository<EmployeeContract> contractRepository, IMapper mapper)
        {
            _customerEmployeeAssociationRepository = customerEmployeeAssociationRepository;
            _logger = logger;
            _mapper = mapper;
            _contractRepository = contractRepository;
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
                        _mapper.Map(model, existingAssociation);
                        await _customerEmployeeAssociationRepository.UpdateAsync(existingAssociation);
                        mappedEntity = _mapper.Map<CustomerEmployeeAssociationDto>(existingAssociation);
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
                return ApiResult<List<CustomerEmployeeAssociationDto>>.Success(mappedEntities);
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
                if(contractData != null)
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
                var entity = await _customerEmployeeAssociationRepository.FirstOrDefaultAsync(t => t.Id == id , include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.Approver).Include(e => e.Customer).Include(e => e.ApplicationUser), ignoreGlobalFilter: true);
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

    }
}

