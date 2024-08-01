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
using NPOI.SS.Formula.Functions;
using AddOptimization.Contracts.Constants;
using AddOptimization.Utilities.Interface;
using System.Diagnostics.Contracts;

namespace AddOptimization.Services.Services;

public class EmployeeContractService : IEmployeeContractService
{
    private readonly ILogger<EmployeeContractService> _logger;
    private readonly IGenericRepository<EmployeeContract> _contractRepository;
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly IGenericRepository<Employee> _employeeRepository;
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
            var activeContracts = (await _contractRepository.QueryAsync(e => e.EmployeeAssociationId == model.EmployeeAssociationId && !e.IsDeleted)).ToList();
            if (activeContracts != null)
            {
                foreach (var contract in activeContracts)
                {
                    contract.IsDeleted = true;
                    
                }
                await _contractRepository.BulkUpdateAsync(activeContracts);
            }

            var datepart = DateTime.UtcNow.ToString("yyyymmdd");
            var guidpart = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            var contractNumber = (await _contractRepository.QueryAsync()).Count()+1;
            var contractName = $"{model.CustomerName}_{model.EmployeeName}_{contractNumber}";

            var entity = _mapper.Map<EmployeeContract>(model);
            entity.IsContractSigned = false;
            entity.ContractName = contractName;
            entity.IsExternal = true;
           
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
            var entity = await _contractRepository.FirstOrDefaultAsync(t => t.Id == id , include: entity => entity.Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser).Include(e => e.ProjectFeePaymentMode), ignoreGlobalFilter: true);
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
            entity.IsExternal = model.IsExternal;
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
            var entity = await _contractRepository.FirstOrDefaultAsync(t => t.EmployeeId == id && !t.IsContractSigned && !t.IsDeleted, include: entity => entity.Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser).Include(e => e.ProjectFeePaymentMode), ignoreGlobalFilter: true);
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
                CreatedAt = e.CreatedAt,
                IsExternal = e.IsExternal,

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

    public async Task<ApiResult<List<EmployeeContractResponseDto>>> GetContractsByAsscociationId(Guid id)
    {
        try
        {
            var entity = await _contractRepository.QueryAsync(o => o.EmployeeAssociationId == id && !o.IsDeleted, include: entities => entities
            .Include(e => e.CreatedByUser),  ignoreGlobalFilter: true);

            if (entity == null)
            {
                return ApiResult<List<EmployeeContractResponseDto>>.NotFound("Contracts");
            }
            var mappedEntity = _mapper.Map<List<EmployeeContractResponseDto>>(entity);
            return ApiResult<List<EmployeeContractResponseDto>>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<PagedApiResult<EmployeeContractResponseDto>> SearchAllContracts(PageQueryFiterBase filters)
    {
        try
        {

            var entities = await _contractRepository.QueryAsync( include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));
            filters.GetValue<Guid>("contractId", (v) =>
            {
                entities = entities.Where(e => e.Id == v);
            });
            filters.GetValue<int>("employeeId", (v) =>
            {
                entities = entities.Where(e => e.EmployeeId == v);
            });
            filters.GetValue<Guid>("customerId", (v) =>
            {
                entities = entities.Where(e => e.CustomerId == v);
            });
            filters.GetValue<string>("employeeName", (v) =>
            {
                entities = entities.Where(e => e.ApplicationUser != null && (e.ApplicationUser.FullName.ToLower().Contains(v.ToLower())));
            });
            filters.GetValue<string>("customerName", (v) =>
            {
                entities = entities.Where(e => e.Customer != null && (e.Customer.Organizations.ToLower().Contains(v.ToLower())));
            });
            filters.GetValue<string>("contractName", (v) =>
            {
                entities = entities.Where(e => e.ContractName != null && (e.ContractName.ToLower().Contains(v.ToLower())));
            });


            filters.GetValue<DateTime>("createdAt", (v) =>
            {
                entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt < v);
            }, OperatorType.lessthan, true);
            filters.GetValue<DateTime>("contractDate", (v) =>
            {
                entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt > v);
            }, OperatorType.greaterthan, true);
            entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
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
                CreatedAt = e.CreatedAt,
                IsActive = e.IsActive,
                IsDeleted = e.IsDeleted,
                ContractName = e.ContractName,
                IsExternal = e.IsExternal,
                PublicHoliday =e.PublicHoliday,
                Salary = e.Salary,
                Hours = e.Hours,
                NIENumber = e.NIENumber,


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

    public async Task<ApiResult<bool>> Delete(Guid id)
    {
        try
        {
            var entity = await _contractRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<bool>.NotFound("Contract");
            }
          
            entity.IsDeleted = true;
            await _contractRepository.UpdateAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }



 //------------------------Internal Contract----------------------------------

    public async Task<ApiResult<EmployeeContractResponseDto>> CreateInternalEmployeeContract(EmployeeContractRequestDto model)
    {
        try
        {
            var activeContracts = (await _contractRepository.QueryAsync(e => e.EmployeeId == model.EmployeeId && !e.IsDeleted)).ToList();
            if (activeContracts != null)
            {
                foreach (var contract in activeContracts)
                {
                    contract.IsDeleted = true;

                }
                await _contractRepository.BulkUpdateAsync(activeContracts);
            }
            var datepart = DateTime.UtcNow.ToString("yyyymmdd");
            var guidpart = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            var contractNumber = (await _contractRepository.QueryAsync()).Count() + 1;
            var contractName = $"{model.EmployeeName}_{contractNumber}";

            var entity = _mapper.Map<EmployeeContract>(model);
            entity.IsContractSigned = false;
            entity.ContractName = contractName;
            entity.IsExternal = false;

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

    public async Task<ApiResult<List<EmployeeContractResponseDto>>> GetInternalContractByEmployeeId(int id)
    {
        try
        {
            var entity = await _contractRepository.QueryAsync(o => o.EmployeeId == id && !o.IsDeleted, include: entities => entities
            .Include(e => e.CreatedByUser), ignoreGlobalFilter: true);

            if (entity == null)
            {
                return ApiResult<List<EmployeeContractResponseDto>>.NotFound("Contracts");
            }
            var mappedEntity = _mapper.Map<List<EmployeeContractResponseDto>>(entity);
            return ApiResult<List<EmployeeContractResponseDto>>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }


    public async Task<ApiResult<EmployeeContractResponseDto>> GetContractByEmployeeId(int id)
    {
        try
        {
            var entity = await _contractRepository.FirstOrDefaultAsync(t => t.EmployeeId == id , include: entity => entity.Include(e => e.InvoicingPaymentMode).Include(e => e.Customer).Include(e => e.CustomerEmployeeAssociation).Include(e => e.ApplicationUser).Include(e => e.ProjectFeePaymentMode), ignoreGlobalFilter: true);
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


}
