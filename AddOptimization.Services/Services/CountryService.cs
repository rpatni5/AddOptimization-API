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
    public class CountryService : ICountryService
    {
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly ILogger<CountryService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CountryService(IGenericRepository<Country> countryRepository, ILogger<CountryService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _countryRepository = countryRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResult<CountryDto>> GetCountriesById(Guid countryId)
        {
            try
            {
                var entity = (await _countryRepository.QueryAsync(o => o.Id == countryId && !o.IsDeleted, ignoreGlobalFilter: true)).FirstOrDefault();
                if (entity == null)
                {
                    return ApiResult<CountryDto>.NotFound("Country");
                }

                var mappedEntity = _mapper.Map<CountryDto>(entity);
                return ApiResult<CountryDto>.Success(mappedEntity);
            }

            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<List<CountryDto>>> GetAllCountries()
        {
            try
            {
                var entities = await _countryRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.CountryName));
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

