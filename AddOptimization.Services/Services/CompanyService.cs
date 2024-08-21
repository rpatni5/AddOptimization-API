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
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AddOptimization.Services.Services;
public class CompanyService : ICompanyService
{
    private readonly IGenericRepository<Company> _companyRepository;
    private readonly ILogger<CompanyService> _logger;
    private readonly IMapper _mapper;
    private readonly ICountryService _countryService;

    public CompanyService(IGenericRepository<Company> companyRepository, ILogger<CompanyService> logger, IMapper mapper, ICountryService countryService)
    {
        _companyRepository = companyRepository;
        _logger = logger;
        _mapper = mapper;
        _countryService = countryService;
    }

    public async Task<ApiResult<CompanyDto>> Create(CompanyDto model)
    {
        try
        {
            var entity = await _companyRepository.FirstOrDefaultAsync();
            if (entity != null)
            {
                entity.CompanyName = model.CompanyName;
                entity.Email = model.Email;
                entity.Website = model.Website;
                entity.BankAccountName = model.BankAccountName;
                entity.BankAccountNumber = model.BankAccountNumber;
                entity.BankName = model.BankName;
                entity.MobileNumber = model.MobileNumber;
                entity.BankAddress = model.BankAddress;
                entity.City = model.City;
                entity.Address = model.Address;
                entity.CountryId = model.CountryId;
                entity.ZipCode = model.ZipCode;
                entity.SwiftCode = model.SwiftCode;
                entity.State = model.State;
                entity.TaxNumber = model.TaxNumber;
                entity.DialCodeId = model.DialCodeId;
                entity.AccountingName = model.AccountingName;
                entity.AccountingEmail = model.AccountingEmail;
                entity.AdministrationContactName = model.AdministrationContactName;
                entity.AdministrationContactEmail = model.AdministrationContactEmail;
                entity.SalesContactName = model.SalesContactName;
                entity.SalesContactEmail = model.SalesContactEmail;
                entity.TechnicalContactName = model.TechnicalContactName;
                entity.TechnicalContactEmail = model.TechnicalContactEmail;
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
            var entity = await _companyRepository.FirstOrDefaultAsync(include: entities => entities.Include(e => e.CountryName), ignoreGlobalFilter: true);
            if(entity == null)
            {
                return null;
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
