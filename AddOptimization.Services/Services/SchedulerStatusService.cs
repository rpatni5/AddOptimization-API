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
    internal class SchedulersStatusService : ISchedulersStatusService
    {
        private readonly IGenericRepository<SchedulerStatus> _schedulersStatusRepository;

        private readonly ILogger<SchedulersStatusService> _logger;
        private readonly IMapper _mapper;
        public SchedulersStatusService(IGenericRepository<SchedulerStatus> schedulersStatusRepository, ILogger<SchedulersStatusService> logger, IMapper mapper)
        {
            _schedulersStatusRepository = schedulersStatusRepository;

            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<SchedulerStatusDto>>> Search()
        {
            try
            {
                var entities = await _schedulersStatusRepository.QueryAsync();
                var mappedEntities = _mapper.Map<List<SchedulerStatusDto>>(entities.ToList());
                return ApiResult<List<SchedulerStatusDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}