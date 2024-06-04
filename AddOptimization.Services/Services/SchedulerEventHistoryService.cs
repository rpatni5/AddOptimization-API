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
    public class SchedulerEventHistoryService : ISchedulerEventHistoryService
    {
        private readonly IGenericRepository<SchedulerEventHistory> _schedulerEventHistoryRepository;
        private readonly ILogger<SchedulerEventHistoryService> _logger;
        private readonly IMapper _mapper;

        public SchedulerEventHistoryService(IGenericRepository<SchedulerEventHistory> schedulerEventHistoryRepository, ILogger<SchedulerEventHistoryService> logger, IMapper mapper)
        {
            _schedulerEventHistoryRepository = schedulerEventHistoryRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<SchedulerEventHistoryDto>>> GetSchedulerEventHistory(Guid id)
        {
            try
            {
                var entity = await _schedulerEventHistoryRepository.QueryAsync(x => x.SchedulerEventId == id && !x.IsDeleted, orderBy: q => q.OrderByDescending(e => e.CreatedAt), include: entities => entities.Include(e => e.SchedulerEvent));
                
                if (entity == null)
                {
                    return ApiResult<List<SchedulerEventHistoryDto>>.NotFound("Scheduler History");
                }
                var mappedEntity = _mapper.Map<List<SchedulerEventHistoryDto>>(entity);
                return ApiResult<List<SchedulerEventHistoryDto>>.Success(mappedEntity);
            }

            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }

}

