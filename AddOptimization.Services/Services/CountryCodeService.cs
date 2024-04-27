using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AddOptimization.Services.Services
{
    public class CountryCodeService : ICountryCodeService
    {
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly ILogger<CountryCodeService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CountryCodeService(IGenericRepository<Country> countryRepository, ILogger<CountryCodeService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _countryRepository = countryRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResult<List<CountryDto>>> GetByCountryId(Guid countryid)
        {
            try
            {
                var entity = await _countryRepository.QueryAsync(o => o.Id == countryid, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<List<CountryDto>>.NotFound("Country");
                }


                var mappedEntity = _mapper.Map<List<CountryDto>>(entity);
                return ApiResult<List<CountryDto>>.Success(mappedEntity);
            }

            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<List<CountryDto>>> GetAllCountry()
        {
            try
            {
                var entities = await _countryRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.Id));
                var mappedEntities = _mapper.Map<List<CountryDto>>(entities);
                return ApiResult<List<CountryDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

    }

}

