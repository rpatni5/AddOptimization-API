using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AddOptimization.Services.Services
{

    internal class SchedulerEventTypeService : ISchedulerEventTypeService
    {
        private readonly IGenericRepository<SchedulerEventType> _schedulersEventRepository;

        private readonly ILogger<SchedulerEventTypeService> _logger;
        private readonly IMapper _mapper;
        public SchedulerEventTypeService(IGenericRepository<SchedulerEventType> schedulersEventRepository, ILogger<SchedulerEventTypeService> logger, IMapper mapper)
        {
            _schedulersEventRepository = schedulersEventRepository;

            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<SchedulerEventTypeDto>>> Search()
        {
            try
            {
                var entities = await _schedulersEventRepository.QueryAsync();
                var mappedEntities = _mapper.Map<List<SchedulerEventTypeDto>>(entities.ToList());
                return ApiResult<List<SchedulerEventTypeDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}
