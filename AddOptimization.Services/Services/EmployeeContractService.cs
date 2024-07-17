using AutoMapper;
using Microsoft.Extensions.Logging;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Services.Services;

public class EmployeeContractService : IEmployeeContractService
{
    private readonly ILogger<EmployeeContractService> _logger;
    private readonly IGenericRepository<EmployeeContract> _contractRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public EmployeeContractService(ILogger<EmployeeContractService> logger, IGenericRepository<EmployeeContract> contractRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _contractRepository = contractRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task<ApiResult<EmployeeContractResponseDto>> Create(EmployeeContractRequestDto model)
    {
        try
        {
            var entity = _mapper.Map<EmployeeContract>(model);
            entity.IsContractSigned = false;
            await _contractRepository.InsertAsync(entity);
            var mappedEntity = _mapper.Map<EmployeeContractResponseDto>(entity);
            return ApiResult<EmployeeContractResponseDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<EmployeeContractResponseDto>> GetEmployeeContractById(Guid id)
    {
        try
        {
            var entity = await _contractRepository.FirstOrDefaultAsync(t => t.EmployeeAssociationId == id, include: entity => entity.Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser), ignoreGlobalFilter: true);
            if (entity == null)
            {
                return null;
            }
            var mappedEntity = _mapper.Map<EmployeeContractResponseDto>(entity);

            return ApiResult<EmployeeContractResponseDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<EmployeeContractResponseDto>> Update(Guid id, EmployeeContractRequestDto model)
    {
        try
        {
            var entity = await _contractRepository.FirstOrDefaultAsync(e => e.Id == id);
            _mapper.Map(model, entity);
            entity.IsContractSigned = false;
            await _contractRepository.UpdateAsync(entity);
            var mappedEntity = _mapper.Map<EmployeeContractResponseDto>(entity);
            return ApiResult<EmployeeContractResponseDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> SignContract(Guid contractId)
    {
        try
        {
            
            var entity = await _contractRepository.FirstOrDefaultAsync(o => o.Id == contractId, disableTracking: false, ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<bool>.Failure("Employee");
            }
            entity.IsContractSigned = true;
            entity.SignedDate = DateTime.UtcNow;
            await _contractRepository.UpdateAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<EmployeeContractResponseDto>> GetEmployeeContractByEmployeeId(int id)
    {
        try
        {
            var entity = await _contractRepository.FirstOrDefaultAsync(t => t.EmployeeId == id && !t.IsContractSigned, include: entity => entity.Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser), ignoreGlobalFilter: true);
            if (entity == null)
            {
                return null;
            }
            var mappedEntity = _mapper.Map<EmployeeContractResponseDto>(entity);

            return ApiResult<EmployeeContractResponseDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<List<EmployeeContractResponseDto>>> Search()
    {
        try
        {

            var entities = await _contractRepository.QueryAsync(include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));

            var mappedEntities = _mapper.Map<List<EmployeeContractResponseDto>>(entities);

            return ApiResult<List<EmployeeContractResponseDto>>.Success(mappedEntities);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

}
