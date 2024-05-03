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
                var entities = await _publicholidayRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.Country).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.Date));
                var mappedEntities = _mapper.Map<List<PublicHolidayDto>>(entities);
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
                var entities = _mapper.Map<PublicHoliday>(model);
                await _publicholidayRepository.InsertAsync(entities);
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
                entity.Description = model.Description;
               

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
       

    }
}
