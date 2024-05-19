using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Utilities.Constants;
using AddOptimization.Services.Constants;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace AddOptimization.Services.Services
{
    public class HolidayAllocationService : IHolidayAllocationService
    {
        private readonly IGenericRepository<HolidayAllocation> _holidayAllocationRepository;
        private readonly ILogger<HolidayAllocationService> _logger;
        private readonly IMapper _mapper;  

        public HolidayAllocationService(IGenericRepository<HolidayAllocation> holidayAllocationRepository, ILogger<HolidayAllocationService> logger, IMapper mapper)
        {
            _holidayAllocationRepository = holidayAllocationRepository;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<ApiResult<List<HolidayAllocationResponseDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _holidayAllocationRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));


                var mappedEntities = _mapper.Map<List<HolidayAllocationResponseDto>>(entities);
                return ApiResult<List<HolidayAllocationResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<HolidayAllocationResponseDto>> Create(HolidayAllocationRequestDto model)
        {
            try
            {
                var isExisting = await _holidayAllocationRepository.IsExist(s => s.UserId == model.UserId && !s.IsDeleted && s.Id != model.Id);
                if (isExisting)
                {
                    return ApiResult<HolidayAllocationResponseDto>.Failure(ValidationCodes.FieldNameAlreadyExists);
                }
                HolidayAllocation entity;
                if (model.Id != Guid.Empty)
                {
                    entity = await _holidayAllocationRepository.FirstOrDefaultAsync(o => o.Id == model.Id);
                    _mapper.Map(model, entity);
                    await _holidayAllocationRepository.UpdateAsync(entity);
                }
                else
                {
                    entity = _mapper.Map<HolidayAllocation>(model);
                    await _holidayAllocationRepository.InsertAsync(entity);
                }
                var mappedEntity = _mapper.Map<HolidayAllocationResponseDto>(entity);
                return ApiResult<HolidayAllocationResponseDto>.Success(mappedEntity);
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
                var entity = await _holidayAllocationRepository.FirstOrDefaultAsync(t => t.Id == id);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Holiday Allocation");
                }
                entity.IsDeleted = true;
                await _holidayAllocationRepository.UpdateAsync(entity);
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
