using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
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

        public async Task<ApiResult<List<CountryDto>>> GetCountries()
        {
            try
            {
                var entities = await _countryRepository.QueryAsync();
                var mappedEntities = _mapper.Map<List<CountryDto>>(entities.ToList());
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

