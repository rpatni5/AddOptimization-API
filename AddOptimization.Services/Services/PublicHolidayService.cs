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

        public async Task<ApiResult<List<PublicHolidayResponseDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _publicholidayRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.Country).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.Date));
                var mappedEntities = _mapper.Map<List<PublicHolidayResponseDto>>(entities);
                return ApiResult<List<PublicHolidayResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<PublicHolidayResponseDto>> Create(PublicHolidayRequestDto model)
        {
            try
            {
                var isExisting = await _publicholidayRepository.IsExist(s => s.Title.ToLower() == model.Title.ToLower());
                if (isExisting)
                {
                    return ApiResult<PublicHolidayResponseDto>.Failure(ValidationCodes.FieldNameAlreadyExists);
                }
                var entity = _mapper.Map<PublicHoliday>(model);
                await _publicholidayRepository.InsertAsync(entity);
                var mappedEntity = _mapper.Map<PublicHolidayResponseDto>(entity);
                return ApiResult<PublicHolidayResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<PublicHolidayResponseDto>> Get(Guid id)
        {
            try
            {
                var entity = await _publicholidayRepository.FirstOrDefaultAsync(o => o.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<PublicHolidayResponseDto>.NotFound("Public Holiday");
                }
                var mappedEntity = _mapper.Map<PublicHolidayResponseDto>(entity);
                return ApiResult<PublicHolidayResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<PublicHolidayResponseDto>> Update(Guid id, PublicHolidayRequestDto model)
        {
            try
            {
                var entity = await _publicholidayRepository.FirstOrDefaultAsync(o => o.Id == id);
                _mapper.Map(model, entity);
                await _publicholidayRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<PublicHolidayResponseDto>(entity);
                return ApiResult<PublicHolidayResponseDto>.Success(mappedEntity);
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
