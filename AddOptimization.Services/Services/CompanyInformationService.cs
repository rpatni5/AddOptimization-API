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
            var entity = _mapper.Map<CompanyInformation>(model);
            await _companyRepository.InsertAsync(entity);
            var mappedEntity = _mapper.Map<CompanyInformationDto>(entity);
            return ApiResult<CompanyInformationDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<CompanyInformationDto>> GetCompanyInformation(Guid id)
    {
        try
        {
            var entity = await _companyRepository.FirstOrDefaultAsync(t => t.Id == id, include: entities => entities.Include(e => e.CreatedByUser), orderBy: (entities) => entities.OrderByDescending(c => c.CreatedAt));
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

    //public async Task<ApiResult<CompanyInformationDto>> Update(Guid id, CompanyInformationDto model)
    //{
    //    try
    //    {
    //        var entity = await _companyRepository.FirstOrDefaultAsync(o => o.Id == id);
    //        _mapper.Map(model, entity);
    //        await _companyRepository.UpdateAsync(entity);
    //        var mappedEntity = _mapper.Map<CompanyInformationDto>(entity);
    //        return ApiResult<CompanyInformationDto>.Success(mappedEntity);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogException(ex);
    //        throw;
    //    }
    //}

}
