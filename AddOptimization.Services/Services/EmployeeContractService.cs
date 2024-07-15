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

namespace AddOptimization.Services.Services;

public class EmployeeContractService : IEmployeeContractService
{
    private readonly ILogger<EmployeeContractService> _logger;
    private readonly IGenericRepository<EmployeeContract> _contractRepository;
    private readonly IMapper _mapper;

    public EmployeeContractService(ILogger<EmployeeContractService> logger, IGenericRepository<EmployeeContract> contractRepository, IMapper mapper)
    {
        _logger = logger;
        _contractRepository = contractRepository;
        _mapper = mapper;
    }


    public async Task<ApiResult<EmployeeContractResponseDto>> Create(EmployeeContractRequestDto model)
    {
        try
        {
            var entity = _mapper.Map<EmployeeContract>(model);
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



}
