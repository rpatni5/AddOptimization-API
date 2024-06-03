using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using AddOptimization.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using AddOptimization.Utilities.Models;
using AddOptimization.Services.Constants;


namespace AddOptimization.Services.Services
{
    public class AbsenceRequestService : IAbsenceRequestService
    {
        private readonly IGenericRepository<AbsenceRequest> _absenceRequestRepository;
        private readonly ILogger<AbsenceRequestService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILeaveStatusesService _leaveStatusesService;


        public AbsenceRequestService(IGenericRepository<AbsenceRequest> absenceRequestRepository, ILogger<AbsenceRequestService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILeaveStatusesService leaveStatusesService)
        {
            _absenceRequestRepository = absenceRequestRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _leaveStatusesService = leaveStatusesService;
        }



        public async Task<ApiResult<AbsenceRequestResponseDto>> Create(AbsenceRequestRequestDto model)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var leaveStatuses = (await _leaveStatusesService.Search(null)).Result;

                var requestedStatusId = leaveStatuses.First(x => x.Name.Equals(LeaveStatusesEnum.Requested.ToString(), StringComparison.InvariantCultureIgnoreCase)).Id;
                var approvedStatusId = leaveStatuses.First(x => x.Name.Equals(LeaveStatusesEnum.Approved.ToString(), StringComparison.InvariantCultureIgnoreCase)).Id;
                var isExisting = await _absenceRequestRepository.IsExist(s => s.Date == model.Date && s.UserId == userId && (s.LeaveStatusId == requestedStatusId || s.LeaveStatusId == approvedStatusId));
                if (isExisting)
                {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "You have already submitted requested for this date.");
                }

                model.UserId = userId;
                model.LeaveStatusId = requestedStatusId;
                var entity = _mapper.Map<AbsenceRequest>(model);
                await _absenceRequestRepository.InsertAsync(entity);
                var mappedEntity = _mapper.Map<AbsenceRequestResponseDto>(entity);
                return ApiResult<AbsenceRequestResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<AbsenceRequestResponseDto>> Get(Guid id)
        {
            try
            {
                var entity = await _absenceRequestRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<AbsenceRequestResponseDto>.NotFound("Absence Request");
                }
                var mappedEntity = _mapper.Map<AbsenceRequestResponseDto>(entity);

                return ApiResult<AbsenceRequestResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<AbsenceRequestResponseDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _absenceRequestRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.LeaveStatuses).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));
                filters.GetValue<string>("userId", (v) =>
                {
                    var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                    entities = entities.Where(e => e.UserId == userId);
                });

                filters.GetValue<int>("employeeId", (v) =>
                {
                    entities = entities.Where(e => e.UserId == v);
                });

                filters.GetValue<DateTime>("startDate", (v) =>
                {
                    entities = entities.Where(e => e.Date >= v);
                });

                filters.GetValue<DateTime>("endDate", (v) =>
                {
                    entities = entities.Where(e => e.Date <= v);
                });

                var mappedEntities = _mapper.Map<List<AbsenceRequestResponseDto>>(entities);
                return ApiResult<List<AbsenceRequestResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<AbsenceRequestResponseDto>> Update(Guid id, AbsenceRequestRequestDto model)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var leaveStatuses = (await _leaveStatusesService.Search(null)).Result;

                var requestedStatusId = leaveStatuses.First(x => x.Name.Equals(LeaveStatusesEnum.Requested.ToString(), StringComparison.InvariantCultureIgnoreCase)).Id;
                var approvedStatusId = leaveStatuses.First(x => x.Name.Equals(LeaveStatusesEnum.Approved.ToString(), StringComparison.InvariantCultureIgnoreCase)).Id;

                var entity = await _absenceRequestRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<AbsenceRequestResponseDto>.NotFound("Absence Request");
                }
                model.UserId = userId;
                model.LeaveStatusId = requestedStatusId;
                _mapper.Map(model, entity);
                entity = await _absenceRequestRepository.UpdateAsync(entity);

                var mappedEntity = _mapper.Map<AbsenceRequestResponseDto>(entity);
                return ApiResult<AbsenceRequestResponseDto>.Success(mappedEntity);
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
                var entity = await _absenceRequestRepository.FirstOrDefaultAsync(t => t.Id == id);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Customer");
                }
                entity.IsDeleted = true;
                await _absenceRequestRepository.UpdateAsync(entity);
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
