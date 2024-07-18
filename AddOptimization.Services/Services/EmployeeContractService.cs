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
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Helpers;

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
            var entity = await _contractRepository.FirstOrDefaultAsync(t => t.EmployeeAssociationId == id && !t.IsDeleted , include: entity => entity.Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser), ignoreGlobalFilter: true);
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
            var entity = await _contractRepository.FirstOrDefaultAsync(t => t.EmployeeId == id && !t.IsContractSigned && !t.IsDeleted, include: entity => entity.Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser), ignoreGlobalFilter: true);
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

    public async Task<PagedApiResult<EmployeeContractResponseDto>> Search(PageQueryFiterBase filters)
    {
        try
        {

            var entities = await _contractRepository.QueryAsync((t => t.IsContractSigned && !t.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));
            entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
            entities = ApplyFilters(entities, filters);

            var pagedResult = PageHelper<EmployeeContract, EmployeeContractResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new EmployeeContractResponseDto
            {
                Id = e.Id,
                CustomerId = e.CustomerId,
                CustomerName = e.Customer.Organizations,
                EmployeeId = e.EmployeeId,
                EmployeeName = e.ApplicationUser.FullName,
                SignedDate = e.SignedDate,
                IsContractSigned = e.IsContractSigned,
                EmployeeAssociationId = e.EmployeeAssociationId,
                JobTitle = e.JobTitle,
                Address = e.Address,


            }).ToList());

            var result = pagedResult;
            return PagedApiResult<EmployeeContractResponseDto>.Success(result);

        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    private IQueryable<EmployeeContract> ApplyFilters(IQueryable<EmployeeContract> entities, PageQueryFiterBase filter)
    {
        filter.GetValue<string>("employeeId", (v) =>
        {
            var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
            entities = entities.Where(e => e.EmployeeId == userId);
        });
        filter.GetValue<string>("employeeName", (v) =>
        {
            entities = entities.Where(e => e.ApplicationUser != null && (e.ApplicationUser.FullName.ToLower().Contains(v.ToLower())));
        });
        filter.GetValue<string>("customerName", (v) =>
        {
            entities = entities.Where(e => e.Customer != null && (e.Customer.Organizations.ToLower().Contains(v.ToLower())));
        });
       
        filter.GetValue<DateTime>("signedDate", (v) =>
        {
            entities = entities.Where(e => e.SignedDate != null && e.SignedDate < v);
        }, OperatorType.lessthan, true);
        filter.GetValue<DateTime>("signedDate", (v) =>
        {
            entities = entities.Where(e => e.SignedDate != null && e.SignedDate > v);
        }, OperatorType.greaterthan, true);

        return entities;
    }


    private IQueryable<EmployeeContract> ApplySorting(IQueryable<EmployeeContract> entities, SortModel sort)
    {
        try
        {
            if (sort?.Name == null)
            {
                entities = entities.OrderByDescending(o => o.CreatedAt);
                return entities;
            }
            var columnName = sort.Name.ToUpper();
            if (sort.Direction == SortDirection.ascending.ToString())
            {
                if (columnName.ToUpper() == nameof(EmployeeContractResponseDto.CustomerName).ToUpper())
                {
                    entities = entities.OrderBy(o => o.Customer.Organizations);
                }
                if (columnName.ToUpper() == nameof(EmployeeContractResponseDto.EmployeeName).ToUpper())
                {
                    entities = entities.OrderBy(o => o.ApplicationUser.FullName); ;
                }
                if (columnName.ToUpper() == nameof(EmployeeContractResponseDto.SignedDate).ToUpper())
                {
                    entities = entities.OrderBy(o => o.SignedDate);
                }

            }

            else
            {
                if (columnName.ToUpper() == nameof(InvoiceResponseDto.CustomerName).ToUpper())
                {
                    entities = entities.OrderByDescending(o => o.Customer.Organizations);
                }
                if (columnName.ToUpper() == nameof(EmployeeContractResponseDto.EmployeeName).ToUpper())
                {
                    entities = entities.OrderByDescending(o => o.ApplicationUser.FullName); ;
                }
                if (columnName.ToUpper() == nameof(EmployeeContractResponseDto.SignedDate).ToUpper())
                {
                    entities = entities.OrderByDescending(o => o.SignedDate);
                }

            }
            return entities;

        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return entities;
        }

    }

}
