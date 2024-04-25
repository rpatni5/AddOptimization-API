using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AddOptimization.Services.Services
{
    public class PublicHolidayService : IPublicHolidayService
    {
        private readonly IGenericRepository<PublicHoliday> _publicholidayRepository;
        private readonly IGenericRepository<Country> _countryRepository;

        private readonly ILogger<PublicHolidayService> _logger;
        private readonly IMapper _mapper;

        public PublicHolidayService(IGenericRepository<PublicHoliday> publicholidayRepository, IGenericRepository<Country> countryRepository, ILogger<PublicHolidayService> logger, IMapper mapper)
        {
            _publicholidayRepository = publicholidayRepository;
            _countryRepository = countryRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<PublicHolidayDto>>> Search( PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _publicholidayRepository.QueryAsync(include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(o => o.Country), orderBy: x => x.OrderBy(x => x.Date));
                entities = entities.Where(x => !x.IsDeleted);

                var response = entities.Select(e => new PublicHolidayDto
                {
                    Title = e.Title,
                    Id = e.Id,
                    Info = e.Info,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    UpdatedBy = e.UpdatedByUser.FullName,
                    Date = e.Date,
                    CountryName = e.Country.CountryName,
                    CountryId = e.CountryId,
                    CreatedBy = e.CreatedByUser.FullName,


                }).ToList();


                //Country=entities.Country.CountryName;

                //response = (List<PublicHolidayDto>)response.Where(x => !x.IsDeleted);



                var mappedEntities = _mapper.Map<List<PublicHolidayDto>>(response);
                return ApiResult<List<PublicHolidayDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }





        public async Task<ApiResult<bool>> Create(PublicHolidayDto model)
        {
            try
            {
                var isExisting = await _publicholidayRepository.IsExist(s => s.Title.ToLower() == model.Title.ToLower());
                if (isExisting)
                {
                    return ApiResult<bool>.Failure(ValidationCodes.FieldNameAlreadyExists);
                }
             
                //var entities = await _publicholidayRepository.QueryAsync(include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.Date));
                var entities = _mapper.Map<PublicHoliday>(model);
               //model.CreatedBy = entities.CreatedByUser != null ? entities.CreatedByUser.FullName : string.Empty;
               // model.UpdatedBy = entities.UpdatedByUser != null ? entities.UpdatedByUser.FullName : string.Empty;
              
                await _publicholidayRepository.InsertAsync(entities);
                //var mappedEntities = _mapper.Map<PublicHolidayDto>(entities);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


      



        public async Task<ApiResult<PublicHolidayDto>> Get(Guid id)
        {
            try
            {
                var entity = await _publicholidayRepository.FirstOrDefaultAsync(o => o.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<PublicHolidayDto>.NotFound("Public Holiday");
                }
                var mappedEntity = _mapper.Map<PublicHolidayDto>(entity);
                return ApiResult<PublicHolidayDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }




        public async Task<ApiResult<PublicHolidayDto>> Update(Guid id, PublicHolidayDto model)
        {
            try
            {
                var entity = await _publicholidayRepository.FirstOrDefaultAsync(o => o.Id == id);


                entity.Title = model.Title;
                entity.Info = model.Info;
               

                await _publicholidayRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<PublicHolidayDto>(entity);
                return ApiResult<PublicHolidayDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }







        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            try
            {
                var entity = await _publicholidayRepository.FirstOrDefaultAsync(t => t.Id == id);
                entity.IsDeleted = true;
                await _publicholidayRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }




         

        public async Task<ApiResult<List<PublicHolidayDto>>> GetByCountryId(Guid countryid)
        {
            try
            {
                var entity = await _publicholidayRepository.QueryAsync(o => o.CountryId == countryid, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<List<PublicHolidayDto>>.NotFound("Country");
                }


                var mappedEntity = _mapper.Map<List<PublicHolidayDto>>(entity);
                return ApiResult<List<PublicHolidayDto>>.Success(mappedEntity);
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
                var entities = await _countryRepository.QueryAsync(include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.Id));
                var mappedEntities = _mapper.Map<List<CountryDto>>(entities);
                return ApiResult<List<CountryDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
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
