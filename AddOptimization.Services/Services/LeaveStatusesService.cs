using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Models;
using Microsoft.AspNetCore.Http;


namespace AddOptimization.Services.Services
{

    public class LeaveStatusesService: ILeaveStatusesService
    {
        private readonly IGenericRepository<LeaveStatuses> _leavestatusRepository;

        private readonly ILogger<LeaveStatusesService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public LeaveStatusesService(IGenericRepository<LeaveStatuses> leavestatusRepository, ILogger<LeaveStatusesService> logger, IMapper mapper , IHttpContextAccessor httpContextAccessor)
        {
            _leavestatusRepository = leavestatusRepository;

            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApiResult<List<LeaveStatusesDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _leavestatusRepository.QueryAsync();                

                var mappedEntities = _mapper.Map<List<LeaveStatusesDto>>(entities.ToList());
                return ApiResult<List<LeaveStatusesDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }

}
