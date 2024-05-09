using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AddOptimization.Services.Services
{
    public class SchedulerEventService : ISchedulerEventService
    {
        private readonly IGenericRepository<SchedulerEvent> _schedulersRepository;
        private readonly IGenericRepository<SchedulerEventDetails> _schedulersDetailsRepository;
        private readonly List<string> _currentUserRoles;
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
            _currentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
        }



        public async Task<PagedApiResult<CreateViewTimesheetResponseDto>> Search(PageQueryFiterBase filters)
        {
            try
            {

                var entities = await _schedulersRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.Approvar).Include(e => e.Client).Include(e => e.UserStatus).Include(e => e.AdminStatus).Include(e => e.ApplicationUser));
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);

                var pagedResult = PageHelper<SchedulerEvent, CreateViewTimesheetResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new CreateViewTimesheetResponseDto
                {
                    Id = e.Id,
                    ClientId = e.ClientId,
                    ApprovarId = e.ApprovarId,
                    ApprovarName = e.Approvar.FullName,
                    ClientName = $"{e.Client.FirstName} {e.Client.LastName}",
                    UserId = e.UserId,
                    UserStatusId = e.UserStatusId,
                    UserName = e.ApplicationUser.FullName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    AdminStatusId = e.AdminStatusId,
                    AdminStatusName = e.AdminStatus.Name,
                    UserStatusName = e.UserStatus.Name,
                    WorkDuration = e.EventDetails.Where(x => x.EventTypes.Name == "Timesheet").Sum(x => x.Duration),
                    Overtime = e.EventDetails.Where(x => x.EventTypes.Name == "Overtime").Sum(x => x.Duration),
                    Holiday = 0,
                }).ToList());
                var retVal = pagedResult;
                return PagedApiResult<CreateViewTimesheetResponseDto>.Success(retVal);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<bool>> Save(List<SchedulerEventDetailsDto> model)
        {
            var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
            model.ForEach(x => x.UserId = userId);
            await _unitOfWork.BeginTransactionAsync();
            var schedluerEventsToUpdate = new List<SchedulerEventDetails>();
            var schedluerEventsToInsert = new List<SchedulerEventDetails>();

            try
            {
                foreach (var item in model)
                {
                    var entity = _mapper.Map<SchedulerEventDetails>(item);
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
                    await _schedulersDetailsRepository.BulkUpdateAsync(schedluerEventsToUpdate);

                }
                if (schedluerEventsToInsert.Count() > 0)
                {
                    await _schedulersDetailsRepository.BulkInsertAsync(schedluerEventsToInsert);
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
                int userId = model.UserId != null ? model.UserId.Value : _httpContextAccessor.HttpContext.GetCurrentUserId().Value;

                SchedulerEvent entity = new SchedulerEvent();
                entity.StartDate = new DateTime(model.DateMonth.Year, model.DateMonth.Month, 1);
                entity.EndDate = entity.StartDate.AddMonths(1).AddDays(-1);

                var response = await _schedulersRepository.FirstOrDefaultAsync(x => x.ClientId == model.ClientId && x.ApprovarId == model.ApprovarId && x.StartDate == entity.StartDate && x.EndDate == entity.EndDate && x.UserId == userId);

                if (response != null)
                {
                    entity = response;
                }
                else
                {
                    var eventStatus = (await _schedulersStatusService.Search()).Result;
                    entity.ApprovarId = model.ApprovarId;
                    entity.ClientId = model.ClientId;
                    entity.UserId = userId;
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
                var superAdminRole = _currentUserRoles.Where(c => c.Contains("Super Admin") || c.Contains("Account Admin")).ToList();

                var entity = await _schedulersDetailsRepository.QueryAsync(include: entities => entities
                .Include(e => e.CreatedByUser).Include(e => e.ApplicationUser).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.SchedulerEvent), predicate: o => o.SchedulerEventId == id, ignoreGlobalFilter: superAdminRole.Count != 0);

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

        public async Task<ApiResult<bool>> SubmitEventDetails(List<SchedulerEventDetailsDto> models)
        {
            var eventDetails = await _schedulersRepository.FirstOrDefaultAsync(x => x.Id == models.First().SchedulerEventId);
            eventDetails.IsDraft = false;
            var eventStatus = (await _schedulersStatusService.Search()).Result;
            var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.PENDING_ACCOUNT_ADMIN_APPROVAL.ToString()).Id;

            eventDetails.UserStatusId = statusId;
            eventDetails.AdminStatusId = statusId;

            var result = await _schedulersRepository.UpdateAsync(eventDetails);

            return await Save(models);
        }

        public async Task<ApiResult<List<SchedulerEventDetailsDto>>> GetSchedularEventDetails(CreateViewTimesheetRequestDto model)
        {
            var eventDetails = (await CreateOrViewTimeSheets(model)).Result;
            return await GetSchedularEventDetails(eventDetails.Id);
        }



        private IQueryable<SchedulerEvent> ApplyFilters(IQueryable<SchedulerEvent> entities, PageQueryFiterBase filter)
        {

            filter.GetValue<string>("userId", (v) =>
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                entities = entities.Where(e => e.UserId == userId);
            });
            filter.GetValue<string>("clientName", (v) =>
            {
                entities = entities.Where(e => e.Client != null && (e.Client.FirstName.ToLower().Contains(v.ToLower()) || e.Client.LastName.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("client", (v) =>
            {
                entities = entities.Where(e => e.ClientId.ToString() == v);
            });
            filter.GetValue<bool>("isDraft", (v) =>
            {
                entities = entities.Where(e => e.IsDraft == v);
            });
            filter.GetValue<string>("approvarName", (v) =>
            {
                entities = entities.Where(e => e.Client != null && (e.Approvar.FirstName.ToLower().Contains(v.ToLower()) || e.Approvar.LastName.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("userStatusName", (v) =>
            {
                entities = entities.Where(e => e.Client != null && (e.UserStatus.Name.ToLower().Contains(v.ToLower()) || e.UserStatus.Name.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("adminStatusName", (v) =>
            {
                entities = entities.Where(e => e.Client != null && (e.AdminStatus.Name.ToLower().Contains(v.ToLower()) || e.AdminStatus.Name.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("adminStatusId", (v) =>
            {
                entities = entities.Where(e => e.AdminStatusId.ToString() == v);
            });
            filter.GetValue<string>("approvarId", (v) =>
            {
                entities = entities.Where(e => e.ApprovarId.ToString() == v);
            });
            filter.GetValue<string>("userName", (v) =>
            {
                entities = entities.Where(e => e.ApplicationUser != null && (e.ApplicationUser.FullName.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("employeeId", (v) =>
            {
                int userId = Convert.ToInt32(v);
                entities = entities.Where(e => e.UserId == userId);
            });

            filter.GetList<DateTime>("duedateRange", (v) =>
            {
                var date = new DateTime(v.Max().Year, v.Max().Month, 1);
                entities = entities.Where(e => e.EndDate < date);
            }, OperatorType.lessthan, true);

            filter.GetList<DateTime>("duedateRange", (v) =>
            {
                var date = (new DateTime(v.Min().Year, v.Min().Month, 1)).AddMonths(1).AddDays(-1);
                entities = entities.Where(e => e.StartDate > date);
            }, OperatorType.greaterthan, true);

            filter.GetList<DateTime>("startDate", (v) =>
            {
                var date = new DateTime(v.Max().Year, v.Max().Month, 1);
                entities = entities.Where(e => e.StartDate == date);
            }, OperatorType.lessthan, true);

            filter.GetList<DateTime>("startDate", (v) =>
            {
                var date = new DateTime(v.Max().Year, v.Max().Month, 1);
                entities = entities.Where(e => e.StartDate == date);
            }, OperatorType.greaterthan, true);

            filter.GetList<DateTime>("endDate", (v) =>
            {
                var date = (new DateTime(v.Min().Year, v.Min().Month, 1)).AddMonths(1).AddDays(-1);
                entities = entities.Where(e => e.EndDate == date);
            }, OperatorType.lessthan, true);

            filter.GetList<DateTime>("endDate", (v) =>
            {
                var date = (new DateTime(v.Min().Year, v.Min().Month, 1)).AddMonths(1).AddDays(-1);
                entities = entities.Where(e => e.EndDate == date);
            }, OperatorType.greaterthan, true);



            return entities;
        }
        private IQueryable<SchedulerEvent> ApplySorting(IQueryable<SchedulerEvent> entities, SortModel sort)
        {
            try
            {
                if (sort?.Name == null)
                {
                    entities = entities.OrderByDescending(o => o.StartDate);
                    return entities;
                }

                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {
                    if (columnName.ToUpper() == nameof(CreateViewTimesheetResponseDto.ClientName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Client.FirstName);
                    }
                    else if (columnName.ToUpper() == nameof(CreateViewTimesheetResponseDto.ApprovarName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Approvar.FirstName);
                    }
                    else if (columnName.ToUpper() == nameof(CreateViewTimesheetResponseDto.UserStatusName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.UserStatus.Name);
                    }
                }

                else
                {
                    if (columnName.ToUpper() == nameof(CreateViewTimesheetResponseDto.ClientName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Client.FirstName);
                    }
                    else if (columnName.ToUpper() == nameof(CreateViewTimesheetResponseDto.ApprovarName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Approvar.FirstName);
                    }
                    else if (columnName.ToUpper() == nameof(CreateViewTimesheetResponseDto.UserStatusName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.UserStatus.Name);
                    }

                }
                return entities;

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return entities;
            }

        }

        public async Task<ApiResult<bool>> ApproveRequest(CreateViewTimesheetResponseDto model)
        {
            try
            {

                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<bool>> RejectRequest(CreateViewTimesheetResponseDto model)
        {
            try
            {
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
