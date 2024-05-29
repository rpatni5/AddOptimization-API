using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Utilities.Constants;
using AutoMapper;
using NPOI.SS.Formula.Functions;

namespace AddOptimization.Services.Services;
public class CompanyService : ICompanyService
{
    private readonly IGenericRepository<Company> _companyRepository;
    private readonly ILogger<CompanyService> _logger;
    private readonly IMapper _mapper;

    public CompanyService(IGenericRepository<Company> companyRepository, ILogger<CompanyService> logger, IMapper mapper)
    {
        _companyRepository = companyRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ApiResult<CompanyDto>> Create(CompanyDto model)
    {
        try
        {
            var entity = await _companyRepository.FirstOrDefaultAsync();
            if (entity != null)
            {
                entity.AccountName = model.AccountName;
                entity.AccountNumber = model.AccountNumber;
                entity.Email = model.Email;
                entity.Website = model.Website;
                entity.BankAccountName = model.BankAccountName;
                entity.BankAccountNumber = model.BankAccountNumber;
                entity.BankName = model.BankName;
                entity.MobileNumber = model.MobileNumber;
                entity.BillingAddress = model.BillingAddress;

                await _companyRepository.UpdateAsync(entity);
            }
            else
            {
                entity = _mapper.Map<Company>(model);
                await _companyRepository.InsertAsync(entity);
            }
       
            var mappedEntity = _mapper.Map<CompanyDto>(entity);
            return ApiResult<CompanyDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<CompanyDto>> GetCompanyInformation()
    {
        try
        {
            var entity = await _companyRepository.FirstOrDefaultAsync();

            if (entity == null)
            {
                return ApiResult<CompanyDto>.NotFound("CompanyInformation");
            }

            var mappedEntity = _mapper.Map<CompanyDto>(entity);
            return ApiResult<CompanyDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }


}
