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
public class CompanyInformationService : ICompanyInformationService
{
    private readonly IGenericRepository<CompanyInformation> _companyRepository;
    private readonly ILogger<CompanyInformationService> _logger;
    private readonly IMapper _mapper;

    public CompanyInformationService(IGenericRepository<CompanyInformation> companyRepository, ILogger<CompanyInformationService> logger, IMapper mapper)
    {
        _companyRepository = companyRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ApiResult<CompanyInformationDto>> Create(CompanyInformationDto model)
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
                entity = _mapper.Map<CompanyInformation>(model);
                await _companyRepository.InsertAsync(entity);
            }
       
            var mappedEntity = _mapper.Map<CompanyInformationDto>(entity);
            return ApiResult<CompanyInformationDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<CompanyInformationDto>> GetCompanyInformation()
    {
        try
        {
            var entity = await _companyRepository.FirstOrDefaultAsync();

            if (entity == null)
            {
                return ApiResult<CompanyInformationDto>.NotFound("CompanyInformation");
            }

            var mappedEntity = _mapper.Map<CompanyInformationDto>(entity);
            return ApiResult<CompanyInformationDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }


}
