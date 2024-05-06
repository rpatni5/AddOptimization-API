using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AddOptimization.Services.Services
{
    public class SchedulerEventService : ISchedulerEventService
    {
        private readonly IGenericRepository<SchedulerEvent> _schedulersRepository;
        private readonly IGenericRepository<SchedulerEventDetails> _schedulersDetailsRepository;

        private readonly ILogger<SchedulerEventService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISchedulersStatusService _schedulersStatusService;

        public SchedulerEventService(IGenericRepository<SchedulerEvent> schedulersRepository, ILogger<SchedulerEventService> logger, IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ISchedulersStatusService schedulersStatusService, IGenericRepository<SchedulerEventDetails> schedulersDetailsRepository)
        {
            _schedulersRepository = schedulersRepository;

            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _schedulersStatusService = schedulersStatusService;
            _schedulersDetailsRepository = schedulersDetailsRepository;
        }
        public async Task<PagedApiResult<SchedulerEventDetailsDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                //var entities = await _schedulersRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.AdminStatus).Include(e => e.ApplicationUser).Include(e => e.Client), orderBy: x => x.OrderBy(x => x.Date));

                //entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                //entities = ApplyFilters(entities, filters);


                //var pagedResult = PageHelper<SchedulerEvent, SchedulersDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new SchedulersDto
                //{
                //    Id = e.Id,
                //    Duration = e.Duration,
                //    Date = e.Date,
                //    Summary = e.Summary,
                //    CreatedAt = e.CreatedAt,
                //    CreatedBy = e.CreatedByUser.FullName,
                //    StatusID = e.AdminStatus.Id,
                //    UserID = e.ApplicationUser.Id,
                //    ClientID = e.Client.Id,

                //}).ToList());
                //var retVal = pagedResult;
                return PagedApiResult<SchedulerEventDetailsDto>.Success(null);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<bool>> Save(List<SchedulerEventDetailsDto> model)
        {
            await _unitOfWork.BeginTransactionAsync();
            var schedluerEventsToUpdate = new List<SchedulerEvent>();
            var schedluerEventsToInsert = new List<SchedulerEvent>();

            try
            {
                foreach (var item in model)
                {
                    var entity = _mapper.Map<SchedulerEvent>(item);
                    if (item.Id != Guid.Empty)
                    {
                        schedluerEventsToUpdate.Add(entity);
                    }
                    else
                    {
                        schedluerEventsToInsert.Add(entity);
                    }
                }

                if (schedluerEventsToUpdate.Count() > 0)
                {
                    await _schedulersRepository.BulkUpdateAsync(schedluerEventsToUpdate);

                }
                if (schedluerEventsToInsert.Count() > 0)
                {
                    await _schedulersRepository.BulkInsertAsync(schedluerEventsToInsert);
                }
                await _unitOfWork.CommitTransactionAsync();
                return ApiResult<bool>.Success(true);

            }

            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            try
            {
                var entity = await _schedulersRepository.FirstOrDefaultAsync(t => t.Id == id);
                entity.IsDeleted = true;
                entity.IsActive = false;

                await _schedulersRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<CreateViewTimesheetResponseDto>> CreateOrViewTimeSheets(CreateViewTimesheetRequestDto model)
        {
            try
            {
                SchedulerEvent entity = new SchedulerEvent();
                entity.StartDate = new DateTime(model.DateMonth.Year, model.DateMonth.Month, 1);
                entity.EndDate = entity.StartDate.AddMonths(1).AddDays(-1);

                var response = await _schedulersRepository.FirstOrDefaultAsync(x => x.ClientId == model.ClientId && x.ApprovarId == model.ApprovarId && x.StartDate == entity.StartDate && x.EndDate == entity.EndDate);

                if (response != null)
                {
                    entity = response;
                }
                else
                {
                    var eventStatus = (await _schedulersStatusService.Search()).Result;
                    entity.ApprovarId = model.ApprovarId;
                    entity.ClientId = model.ClientId;
                    entity.UserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                    var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.DRAFT.ToString()).Id;
                    entity.AdminStatusId = statusId;
                    entity.UserStatusId = statusId;
                    entity.IsDraft = true;
                    await _schedulersRepository.InsertAsync(entity);
                }


                entity = await _schedulersRepository.FirstOrDefaultAsync(x => x.Id == entity.Id, include: entities => entities.Include(e => e.Approvar).Include(e => e.UserStatus).Include(e => e.AdminStatus).Include(e => e.ApplicationUser).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.Client));

                var mappedEntity = _mapper.Map<CreateViewTimesheetResponseDto>(entity);
                return ApiResult<CreateViewTimesheetResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<CreateViewTimesheetResponseDto>> GetSchedulerEvent(Guid id)
        {
            var entity = await _schedulersRepository.FirstOrDefaultAsync(x => x.Id == id, include: entities => entities.Include(e => e.Approvar).Include(e => e.UserStatus).Include(e => e.AdminStatus).Include(e => e.ApplicationUser).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.Client));

            var mappedEntity = _mapper.Map<CreateViewTimesheetResponseDto>(entity);
            return ApiResult<CreateViewTimesheetResponseDto>.Success(mappedEntity);
        }


        public async Task<ApiResult<List<SchedulerEventDetailsDto>>> GetSchedularEventDetails(Guid id)
        {
            try
            {
                var entity = await _schedulersDetailsRepository.QueryAsync(include: entities => entities
                .Include(e => e.CreatedByUser).Include(e => e.ApplicationUser).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.SchedulerEvent), predicate: o => o.SchedulerEventId == id, ignoreGlobalFilter: true);

                if (entity == null)
                {
                    return ApiResult<List<SchedulerEventDetailsDto>>.NotFound("SchedularEventDetails");
                }
                var mappedEntity = _mapper.Map<List<SchedulerEventDetailsDto>>(entity);
                return ApiResult<List<SchedulerEventDetailsDto>>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}
