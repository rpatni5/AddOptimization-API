using AddOptimization.Contracts.Constants;
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
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Services;
using AutoMapper;
using iText.Layout.Element;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AddOptimization.Services.Services
{
    public class SchedulerEventService : ISchedulerEventService
    {
        private readonly IGenericRepository<SchedulerEvent> _schedulersRepository;
        private readonly IGenericRepository<SchedulerEventDetails> _schedulersDetailsRepository;
        private readonly IGenericRepository<Customer> _customersRepository;
        private readonly List<string> _currentUserRoles;
        private readonly ILogger<SchedulerEventService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISchedulersStatusService _schedulersStatusService;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly CustomDataProtectionService _protectionService;
        private readonly IGenericRepository<SchedulerEventHistory> _schedulerEventHistoryRepository;
        private readonly IGenericRepository<ApplicationUser> _appUserRepository;
        private readonly ISchedulerEventTypeService _schedulerEventTypeService;
        private readonly IAbsenceRequestService _absenceRequestService;
        private readonly INotificationService _notificationService;
        public SchedulerEventService(IGenericRepository<SchedulerEvent> schedulersRepository, ILogger<SchedulerEventService> logger, IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ISchedulersStatusService schedulersStatusService, IGenericRepository<SchedulerEventDetails> schedulersDetailsRepository, IGenericRepository<Customer> customersRepository, IGenericRepository<SchedulerEventHistory> schedulerEventHistoryRepository, ISchedulerEventTypeService schedulerEventTypeService, IAbsenceRequestService absenceRequestService,
        IConfiguration configuration, IEmailService emailService, ITemplateService templateService, CustomDataProtectionService protectionService, IGenericRepository<ApplicationUser> appUserRepository, INotificationService notificationService)
        {
            _schedulersRepository = schedulersRepository;
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _schedulersStatusService = schedulersStatusService;
            _schedulersDetailsRepository = schedulersDetailsRepository;
            _currentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
            _emailService = emailService;
            _templateService = templateService;
            _configuration = configuration;
            _protectionService = protectionService;
            _customersRepository = customersRepository;
            _schedulerEventHistoryRepository = schedulerEventHistoryRepository;
            _appUserRepository = appUserRepository;
            _schedulerEventTypeService = schedulerEventTypeService;
            _absenceRequestService = absenceRequestService;
            _notificationService = notificationService;
        }



        public async Task<PagedApiResult<SchedulerEventResponseDto>> Search(PageQueryFiterBase filters)
        {
            try
            {

                var entities = await _schedulersRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.Approvar).Include(e => e.Customer).Include(e => e.UserStatus).Include(e => e.AdminStatus).Include(e => e.ApplicationUser));
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);

                var pagedResult = PageHelper<SchedulerEvent, SchedulerEventResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new SchedulerEventResponseDto
                {
                    Id = e.Id,
                    CustomerId = e.CustomerId,
                    ApprovarId = e.ApprovarId,
                    ApprovarName = e.Approvar.FullName,
                    CustomerName = e.Customer.Organizations,
                    UserId = e.UserId,
                    UserStatusId = e.UserStatusId,
                    UserName = e.ApplicationUser.FullName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    AdminStatusId = e.AdminStatusId,
                    AdminStatusName = e.AdminStatus.Name,
                    AdminStatusKey = e.AdminStatus.StatusKey,
                    UserStatusName = e.UserStatus.Name,
                    UserStatusKey = e.UserStatus.StatusKey,
                    WorkDuration = e.EventDetails.Where(x => !x.IsDeleted && x.EventTypes.Name == "Timesheet").Sum(x => x.Duration),
                    Overtime = e.EventDetails.Where(x => !x.IsDeleted && x.EventTypes.Name == "Overtime").Sum(x => x.Duration),
                    IsCustomerApprovalPending = e.AdminStatus.StatusKey.ToString() == SchedulerStatusesEnum.PENDING_CUSTOMER_APPROVAL.ToString(),
                }).ToList());

                filters.GetValue<bool>("includeHoliday", (v) =>
                {
                    if (v)
                    {
                        pagedResult.Result.ForEach(e =>
                        {
                            e.Holiday = GetHolidaysCount(e.StartDate, e.EndDate, e.UserId);
                        });
                    }
                });

                var retVal = pagedResult;
                return PagedApiResult<SchedulerEventResponseDto>.Success(retVal);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private decimal GetHolidaysCount(DateTime startDate, DateTime endDate, int employeeId)
        {
            PageQueryFiterBase filter = new PageQueryFiterBase();
            filter.AddFilter("startDate", OperatorType.equal.ToString(), startDate);
            filter.AddFilter("endDate", OperatorType.equal.ToString(), endDate);
            filter.AddFilter("employeeId", OperatorType.equal.ToString(), employeeId);
            var result = (_absenceRequestService.Search(filter)).Result.Result;
            return result.Sum(x => x.Duration);
        }
        public async Task<ApiResult<bool>> Save(List<SchedulerEventDetailsDto> schedulerEventDetails)
        {
            var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
            var eventDetail = schedulerEventDetails.FirstOrDefault(x => x.SchedulerEventId != Guid.Empty);
            if (eventDetail != null)
            {
                userId = (await _schedulersRepository.FirstOrDefaultAsync(x => x.Id == eventDetail.SchedulerEventId)).UserId;
            }

            schedulerEventDetails.ForEach(x =>
            {
                if (x.UserId == null)
                {
                    x.UserId = userId;
                }
            });
            await _unitOfWork.BeginTransactionAsync();
            var schedluerEventsToUpdate = new List<SchedulerEventDetails>();
            var schedluerEventsToInsert = new List<SchedulerEventDetails>();

            try
            {
                foreach (var item in schedulerEventDetails)
                {
                    var entity = _mapper.Map<SchedulerEventDetails>(item);
                    if (item.Id != Guid.Empty)
                    {
                        entity.Date = entity.Date.Value;
                        schedluerEventsToUpdate.Add(entity);
                    }
                    else
                    {
                        entity.Date = entity.Date.Value;
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
                var entity = await _schedulersDetailsRepository.FirstOrDefaultAsync(t => t.Id == id);
                entity.IsDeleted = true;
                entity.IsActive = true;

                await _schedulersDetailsRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<SchedulerEventResponseDto>> CreateOrViewTimeSheets(SchedulerEventRequestDto model)
        {
            try
            {
                int userId = model.UserId != null ? model.UserId.Value : _httpContextAccessor.HttpContext.GetCurrentUserId().Value;

                SchedulerEvent entity = new SchedulerEvent();
                entity.StartDate = new DateTime(model.DateMonth.Year, model.DateMonth.Month, 1);
                entity.EndDate = entity.StartDate.AddMonths(1).AddDays(-1);

                var response = await _schedulersRepository.FirstOrDefaultAsync(x => x.CustomerId == model.CustomerId && x.ApprovarId == model.ApprovarId && x.StartDate == entity.StartDate && x.EndDate == entity.EndDate && x.UserId == userId);

                if (response != null)
                {
                    entity = response;
                }
                else
                {
                    var eventStatus = (await _schedulersStatusService.Search()).Result;
                    entity.ApprovarId = model.ApprovarId;
                    entity.CustomerId = model.CustomerId;
                    entity.UserId = userId;
                    var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.DRAFT.ToString()).Id;
                    entity.AdminStatusId = statusId;
                    entity.UserStatusId = statusId;
                    entity.IsDraft = true;
                    await _schedulersRepository.InsertAsync(entity);
                }


                entity = await _schedulersRepository.FirstOrDefaultAsync(x => x.Id == entity.Id, include: entities => entities.Include(e => e.Approvar).Include(e => e.UserStatus).Include(e => e.AdminStatus).Include(e => e.ApplicationUser).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.Customer));
                entity.StartDate = entity.StartDate;
                entity.EndDate = entity.EndDate;
                var mappedEntity = _mapper.Map<SchedulerEventResponseDto>(entity);
                return ApiResult<SchedulerEventResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<SchedulerEventResponseDto>> GetSchedulerEvent(Guid id)
        {
            var eventTypes = (await _schedulerEventTypeService.Search()).Result;
            var timesheetEventId = eventTypes.FirstOrDefault(x => x.Name.Equals("timesheet", StringComparison.InvariantCultureIgnoreCase)).Id;
            var overtimeId = eventTypes.FirstOrDefault(x => x.Name.Equals("overtime", StringComparison.InvariantCultureIgnoreCase)).Id;
            var eventStatus = (await _schedulersStatusService.Search()).Result;
            var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.PENDING_CUSTOMER_APPROVAL.ToString()).Id;
            var entity = await _schedulersRepository.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, include: entities => entities.Include(e => e.Approvar).Include(e => e.UserStatus).Include(e => e.AdminStatus).Include(e => e.ApplicationUser).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.Customer).Include(e => e.EventDetails));
            var mappedEntity = _mapper.Map<SchedulerEventResponseDto>(entity);
            mappedEntity.WorkDuration = entity.EventDetails.Where(x => x.EventTypeId == timesheetEventId).Sum(x => x.Duration);
            mappedEntity.Overtime = entity.EventDetails.Where(x => x.EventTypeId == overtimeId).Sum(x => x.Duration);
            mappedEntity.IsCustomerApprovalPending = mappedEntity.AdminStatusId.ToString() == statusId.ToString();
            mappedEntity.StartDate = mappedEntity.StartDate;
            mappedEntity.EndDate = mappedEntity.EndDate;
            return ApiResult<SchedulerEventResponseDto>.Success(mappedEntity);
        }

        public async Task<ApiResult<List<SchedulerEventResponseDto>>> GetSchedulerEventsForEmailReminder(Guid customerId, int userId)
        {
            var entity = await _schedulersRepository.QueryAsync(x => x.CustomerId == customerId && x.UserId == userId && !x.IsDeleted, include: entities => entities
            .Include(e => e.Approvar)
            .Include(e => e.UserStatus).Include(e => e.AdminStatus)
            .Include(e => e.ApplicationUser).Include(e => e.CreatedByUser)
            .Include(e => e.UpdatedByUser).Include(e => e.Customer)
            .Include(e => e.EventDetails));
            var response = new List<SchedulerEventResponseDto>();

            if (entity == null || !entity.Any()) // User with no timesheets will be notified for 3 recent months passed
            {
                var user = (await _appUserRepository.FirstOrDefaultAsync(x => x.Id == userId));
                var months = MonthDateRangeHelper.GetMonthDateRanges();
                foreach (var month in months)
                {
                    var value = entity.FirstOrDefault(c => c.StartDate == month.StartDate && c.EndDate == month.EndDate);
                    if (value == null)
                    {
                        var schedulerEvent = new SchedulerEventResponseDto
                        {
                            UserName = user.FullName,
                            StartDate = value != null ? value.StartDate : month.StartDate,
                            EndDate = value != null ? value.EndDate : month.EndDate,
                            ApplicationUser = _mapper.Map<ApplicationUserDto>(user),
                            EventDetails = null
                        };
                        response.Add(schedulerEvent);
                    }
                }
                return ApiResult<List<SchedulerEventResponseDto>>.Success(response);
            }

            DateTime today = DateTime.Today;
            DateTime endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            if (today == endOfMonth)
            {
                var previousMonthsDateRanges = MonthDateRangeHelper.GetMonthDateRanges(true);
                foreach (var month in previousMonthsDateRanges)
                {
                    var value = entity.FirstOrDefault(c => c.StartDate == month.StartDate && c.EndDate == month.EndDate);
                    if (value == null || value?.IsDraft == true)
                    {
                        var schedulerEvent = new SchedulerEventResponseDto
                        {
                            UserName = value != null ? value.ApplicationUser?.FullName : entity.FirstOrDefault().ApplicationUser.FullName,
                            StartDate = value != null ? value.StartDate : month.StartDate,
                            EndDate = value != null ? value.EndDate : month.EndDate,
                            ApplicationUser = _mapper.Map<ApplicationUserDto>(value != null ? value.ApplicationUser : entity.FirstOrDefault().ApplicationUser),
                            EventDetails = _mapper.Map<List<SchedulerEventDetailsDto>>(value != null ? value.EventDetails : null)
                        };
                        response.Add(schedulerEvent);
                    }
                }
            }
            else
            {
                var months = MonthDateRangeHelper.GetMonthDateRanges();
                foreach (var month in months)
                {
                    var value = entity.FirstOrDefault(c => c.StartDate == month.StartDate && c.EndDate == month.EndDate);
                    if (value == null || value?.IsDraft == true)
                    {
                        var schedulerEvent = new SchedulerEventResponseDto
                        {
                            UserName = value != null ? value.ApplicationUser?.FullName : entity.FirstOrDefault().ApplicationUser.FullName,
                            StartDate = value != null ? value.StartDate : month.StartDate,
                            EndDate = value != null ? value.EndDate : month.EndDate,
                            ApplicationUser = _mapper.Map<ApplicationUserDto>(value != null ? value.ApplicationUser : entity.FirstOrDefault().ApplicationUser),
                            EventDetails = _mapper.Map<List<SchedulerEventDetailsDto>>(value != null ? value.EventDetails : null)
                        };
                        response.Add(schedulerEvent);
                    }
                }
            }
            return ApiResult<List<SchedulerEventResponseDto>>.Success(response);
        }
        public async Task<ApiResult<List<SchedulerEventResponseDto>>> GetSchedulerEventsForApproveEmailReminder()
        {
            var entity = await _schedulersRepository.QueryAsync(x => x.AdminStatus.StatusKey == SchedulerStatusesEnum.PENDING_CUSTOMER_APPROVAL.ToString(), include: entities => entities.Include(e => e.Approvar).Include(e => e.UserStatus).Include(e => e.AdminStatus).Include(e => e.ApplicationUser).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.Customer));
            if (entity == null || !entity.Any())
            {
                return ApiResult<List<SchedulerEventResponseDto>>.Failure(ValidationCodes.SchedulerEventsDoesNotExists);
            }
            var response = _mapper.Map<List<SchedulerEventResponseDto>>(entity);
            return ApiResult<List<SchedulerEventResponseDto>>.Success(response);
        }
        public async Task<ApiResult<List<SchedulerEventDetailsDto>>> GetSchedulerEventDetails(Guid id, bool getRoleBasedData = true)
        {
            try
            {
                bool ignoreGlobalFilter = true;
                if (getRoleBasedData)
                {
                    var superAdminRole = _currentUserRoles.Where(c => c.Contains("Super Admin") || c.Contains("Account Admin")).ToList();
                    ignoreGlobalFilter = superAdminRole.Count != 0;
                }

                var entity = await _schedulersDetailsRepository.QueryAsync(include: entities => entities
                .Include(e => e.CreatedByUser).Include(e => e.ApplicationUser).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.SchedulerEvent), predicate: (o => o.SchedulerEventId == id && !o.IsDeleted), ignoreGlobalFilter: ignoreGlobalFilter);

                if (entity == null)
                {
                    return ApiResult<List<SchedulerEventDetailsDto>>.NotFound("SchedulerEventDetails");
                }
                var mappedEntity = _mapper.Map<List<SchedulerEventDetailsDto>>(entity);
                mappedEntity.ForEach(x => x.Date = x.Date.Value);
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
            var userStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.PENDING_APPROVAL.ToString()).Id;
            var adminStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.PENDING_ACCOUNT_APPROVAL.ToString()).Id;

            eventDetails.UserStatusId = userStatusId;
            eventDetails.AdminStatusId = adminStatusId;

            var result = await _schedulersRepository.UpdateAsync(eventDetails);
            var saveResult = await Save(models);
            if (saveResult.IsSuccess)
            {
                SchedulerEventHistory entity = new SchedulerEventHistory()
                {
                    SchedulerEventId = eventDetails.Id,
                    UserId = eventDetails.UserId,
                    UserStatusId = eventDetails.UserStatusId,
                    AdminStatusId = eventDetails.AdminStatusId,
                };
                await _schedulerEventHistoryRepository.InsertAsync(entity);
            }
            var approver = (await _appUserRepository.FirstOrDefaultAsync(x => x.Id == result.ApprovarId));
            var user = (await _appUserRepository.FirstOrDefaultAsync(x => x.Id == result.UserId));
            var details = (await _schedulersDetailsRepository.QueryAsync(x => x.SchedulerEventId == result.Id)).ToList();
            var duration = await CalculateTimesheetsDaysAndOvertimeHours(result, details);
            await SendRequestTimesheetApprovalEmailToAccountAdmin(approver.Email, result, approver.FullName, user.FullName, duration.Item1, duration.Item2);
            await SendNotificationToAccountAdmin(approver.Id, user, result);
            //Send email on timesheet submission to approvar -> send direct link of approval
            return saveResult;
        }
        private async Task SendNotificationToAccountAdmin(int Id, ApplicationUser user, SchedulerEvent schedulerEvent)
        {
            var subject = $"Timesheet submitted by {user.FullName}";
            var bodyContent = $"{user.FullName} has submitted a timesheet for month {DateTimeFormatInfo.CurrentInfo.GetMonthName(schedulerEvent.StartDate.Month)}";
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            var linkUrl = GetTimesheetLinkForAccountAdmin(schedulerEvent.Id);
            var createdByUser = new NotificationUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
            };
            var model = new NotificationDto
            {
                Subject = subject,
                Content = bodyContent,
                Link = linkUrl,
                AppplicationUserId = Id,
                GroupKey = $"Timesheet submitted #{user.FullName}",
            };

            await _notificationService.CreateAsync(model);
        }

        public async Task<ApiResult<List<SchedulerEventDetailsDto>>> GetSchedulerEventDetails(SchedulerEventRequestDto model)
        {
            var eventDetails = (await CreateOrViewTimeSheets(model)).Result;
            return await GetSchedulerEventDetails(eventDetails.Id);
        }



        private IQueryable<SchedulerEvent> ApplyFilters(IQueryable<SchedulerEvent> entities, PageQueryFiterBase filter)
        {

            filter.GetValue<string>("userId", (v) =>
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                entities = entities.Where(e => e.UserId == userId);
            });
            filter.GetValue<string>("customerName", (v) =>
            {
                entities = entities.Where(e => e.Customer != null && (e.Customer.Organizations.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("customer", (v) =>
            {
                entities = entities.Where(e => e.CustomerId.ToString() == v);
            });
            filter.GetValue<bool>("isDraft", (v) =>
            {
                entities = entities.Where(e => e.IsDraft == v);
            });
            filter.GetValue<string>("approvarName", (v) =>
            {
                entities = entities.Where(e => e.Customer != null && (e.Approvar.FirstName.ToLower().Contains(v.ToLower()) || e.Approvar.LastName.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("userStatusName", (v) =>
            {
                entities = entities.Where(e => e.Customer != null && (e.UserStatus.Name.ToLower().Contains(v.ToLower()) || e.UserStatus.Name.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("userStatusId", (v) =>
            {
                entities = entities.Where(e => e.UserStatusId.ToString() == v);
            });
            filter.GetValue<string>("adminStatusName", (v) =>
            {
                entities = entities.Where(e => e.Customer != null && (e.AdminStatus.Name.ToLower().Contains(v.ToLower()) || e.AdminStatus.Name.ToLower().Contains(v.ToLower())));
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
            filter.GetValue<string>("workDuration", (v) =>
            {
                int workDuration = Convert.ToInt32(v);
                entities = entities.Where(e => e.EventDetails.Where(x => x.EventTypes.Name == "Timesheet").Sum(x => x.Duration) == workDuration);
            });
            filter.GetValue<string>("overtime", (v) =>
            {
                int overtime = Convert.ToInt32(v);
                entities = entities.Where(e => e.EventDetails.Where(x => x.EventTypes.Name == "Overtime").Sum(x => x.Duration) == overtime);
            });
            filter.GetList<DateTime>("duedateRange", (v) =>
            {
                entities = entities.Where(e => e.StartDate.Date <= v.Max().Date);
            }, OperatorType.lessthan, true);

            filter.GetList<DateTime>("duedateRange", (v) =>
            {
                entities = entities.Where(e => e.StartDate.Date >= v.Min().Date);
            }, OperatorType.greaterthan, true);

            filter.GetList<DateTime>("startDate", (v) =>
            {
                var date = new DateTime(v.Max().Year, v.Max().Month, 1);
                entities = entities.Where(e => e.StartDate == date);
            }, OperatorType.lessthan, true);

            filter.GetList<DateTime>("endDate", (v) =>
            {
                var date = (new DateTime(v.Min().Year, v.Min().Month, 1)).AddMonths(1).AddDays(-1);
                entities = entities.Where(e => e.EndDate == date);
            }, OperatorType.lessthan, true);

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
                    if (columnName.ToUpper() == nameof(SchedulerEventResponseDto.CustomerName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Customer.Organizations);
                    }
                    if (columnName.ToUpper() == nameof(SchedulerEventResponseDto.ApprovarName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Approvar.FirstName);
                    }
                    else if (columnName.ToUpper() == nameof(SchedulerEventResponseDto.UserStatusName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.UserStatus.Name);
                    }
                }

                else
                {
                    if (columnName.ToUpper() == nameof(SchedulerEventResponseDto.CustomerName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Customer.Organizations);
                    }
                    if (columnName.ToUpper() == nameof(SchedulerEventResponseDto.ApprovarName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Approvar.FirstName);
                    }
                    else if (columnName.ToUpper() == nameof(SchedulerEventResponseDto.UserStatusName).ToUpper())
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


        public async Task<ApiResult<bool>> ApproveRequest(AccountAdminActionRequestDto model)
        {
            try
            {
                var eventDetails = await _schedulersRepository.FirstOrDefaultAsync(x => x.Id == model.Id);
                var eventStatus = (await _schedulersStatusService.Search()).Result;
                var customerApprovedId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.CUSTOMER_APPROVED.ToString()).Id;
                var pendingCustomerApprovedId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.PENDING_CUSTOMER_APPROVAL.ToString()).Id;
                var userApprovedStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.APPROVED.ToString()).Id;
                var userPendingApprovalStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.PENDING_APPROVAL.ToString()).Id;


                var customerDetails = await _customersRepository.FirstOrDefaultAsync(x => x.Id == model.CustomerId);
                if (customerDetails.IsApprovalRequired)
                {
                    eventDetails.UserStatusId = userPendingApprovalStatusId;
                    eventDetails.AdminStatusId = pendingCustomerApprovedId;
                }
                else
                {
                    eventDetails.UserStatusId = userApprovedStatusId;
                    eventDetails.AdminStatusId = customerApprovedId;
                }
                var result = await _schedulersRepository.UpdateAsync(eventDetails);
                SchedulerEventHistory entity = new SchedulerEventHistory()
                {
                    SchedulerEventId = eventDetails.Id,
                    UserId = eventDetails.UserId,
                    UserStatusId = eventDetails.UserStatusId,
                    AdminStatusId = eventDetails.AdminStatusId,
                    Comment = model.Comment,
                };

                await _schedulerEventHistoryRepository.InsertAsync(entity);
                var user = (await _appUserRepository.FirstOrDefaultAsync(x => x.Id == result.UserId));
                var details = (await _schedulersDetailsRepository.QueryAsync(x => x.SchedulerEventId == result.Id)).ToList();
                var duration = await CalculateTimesheetsDaysAndOvertimeHours(result, details);
                if (customerDetails.IsApprovalRequired)
                {
                    await SendRequestTimesheetApprovalEmailToCustomer(customerDetails.AdministrationContactEmail, result, customerDetails.AdministrationContactName, user.FullName, duration.Item1, duration.Item2);
                }
                else
                {
                    await SendTimesheetApprovedEmailToEmployee(user.Email, result, user.FullName, model.ApprovarName, duration.Item1, duration.Item2);
                    await SendApprovedNotificationToEmployee(model, user, result);
                }

                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        private async Task SendApprovedNotificationToEmployee(AccountAdminActionRequestDto admin, ApplicationUser user, SchedulerEvent schedulerEvent)
        {
            var subject = $"Timesheet approved by {admin.Approvar.FullName}";
            var bodyContent = $"{admin.Approvar.FullName} has approved your timesheet for {DateTimeFormatInfo.CurrentInfo.GetMonthName(schedulerEvent.StartDate.Month)}";
            var linkUrl = GetTimesheetLinkForEmployee(schedulerEvent.Id);
            var createdByUser = new NotificationUserDto
            {
                Id = admin.Approvar.Id,
                FullName = admin.Approvar.FullName,
                Email = admin.Approvar.Email,
            };
            var model = new NotificationDto
            {
                Subject = subject,
                Content = bodyContent,
                Link = linkUrl,
                AppplicationUserId = user.Id,
                GroupKey = $"Timesheet approved #{admin.Approvar.FullName}",
            };

            await _notificationService.CreateAsync(model);
        }

        public async Task<ApiResult<bool>> DeclineRequest(AccountAdminActionRequestDto model)
        {
            try
            {
                var eventDetails = await _schedulersRepository.FirstOrDefaultAsync(x => x.Id == model.Id);
                eventDetails.IsDraft = true;
                var eventStatus = (await _schedulersStatusService.Search()).Result;
                var draftStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.DRAFT.ToString()).Id;
                var declinedStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.DECLINED.ToString()).Id;
                eventDetails.UserStatusId = declinedStatusId;
                eventDetails.AdminStatusId = draftStatusId;
                var result = await _schedulersRepository.UpdateAsync(eventDetails);
                SchedulerEventHistory entity = new SchedulerEventHistory()
                {
                    SchedulerEventId = eventDetails.Id,
                    UserId = eventDetails.UserId,
                    UserStatusId = eventDetails.UserStatusId,
                    AdminStatusId = eventDetails.AdminStatusId,
                    Comment = model.Comment,
                };

                await _schedulerEventHistoryRepository.InsertAsync(entity);
                var user = (await _appUserRepository.FirstOrDefaultAsync(x => x.Id == result.UserId));
                var details = (await _schedulersDetailsRepository.QueryAsync(x => x.SchedulerEventId == result.Id)).ToList();
                var duration = await CalculateTimesheetsDaysAndOvertimeHours(result, details);
                await SendTimesheetDeclinedEmailToEmployee(user.Email, result, user.FullName, model.ApprovarName, duration.Item1, duration.Item2, model.Comment);
                await SendDeclinedNotificationToEmployee(model, user, result);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        private async Task SendDeclinedNotificationToEmployee(AccountAdminActionRequestDto admin, ApplicationUser user, SchedulerEvent schedulerEvent)
        {
            var subject = $"Timesheet declined by {admin.Approvar.FullName}";
            var bodyContent = $"{admin.Approvar.FullName} has declined your timesheet for {DateTimeFormatInfo.CurrentInfo.GetMonthName(schedulerEvent.StartDate.Month)}";
            var linkUrl = GetTimesheetLinkForEmployee(schedulerEvent.Id);

            var createdByUser = new NotificationUserDto
            {
                Id = admin.Approvar.Id,
                FullName = admin.Approvar.FullName,
                Email = admin.Approvar.Email,
            };
            var model = new NotificationDto
            {
                Subject = subject,
                Content = bodyContent,
                Link = linkUrl,
                AppplicationUserId = user.Id,
                GroupKey = $"Timesheet declined #{admin.Approvar.FullName}",
            };

            await _notificationService.CreateAsync(model);
        }
        public async Task<ApiResult<bool>> TimesheetAction(CustomerTimesheetActionDto model)
        {
            try
            {
                var eventDetails = await _schedulersRepository.FirstOrDefaultAsync(x => x.Id == model.Id);
                var eventStatus = (await _schedulersStatusService.Search()).Result;
                var customerApprovedId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.CUSTOMER_APPROVED.ToString()).Id;
                var customerDeclinedId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.CUSTOMER_DECLINED.ToString()).Id;

                var userStatusApprovedId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.APPROVED.ToString()).Id;
                var userDeclinedStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == SchedulerStatusesEnum.DECLINED.ToString()).Id;

                if (model.IsApproved)
                {
                    eventDetails.AdminStatusId = customerApprovedId;
                    eventDetails.UserStatusId = userStatusApprovedId;
                }
                else
                {
                    eventDetails.AdminStatusId = customerDeclinedId;
                    eventDetails.UserStatusId = userDeclinedStatusId;
                }
                var result = await _schedulersRepository.UpdateAsync(eventDetails);
                SchedulerEventHistory entity = new SchedulerEventHistory()
                {
                    SchedulerEventId = eventDetails.Id,
                    UserId = eventDetails.UserId,
                    UserStatusId = eventDetails.UserStatusId,
                    AdminStatusId = eventDetails.AdminStatusId,
                    Comment = model.Comment,
                };

                await _schedulerEventHistoryRepository.InsertAsync(entity);

                var approver = (await _appUserRepository.FirstOrDefaultAsync(x => x.Id == result.ApprovarId));
                var user = (await _appUserRepository.FirstOrDefaultAsync(x => x.Id == result.UserId));
                var customer = (await _customersRepository.FirstOrDefaultAsync(x => x.Id == result.CustomerId));
                var details = (await _schedulersDetailsRepository.QueryAsync(x => x.SchedulerEventId == result.Id)).ToList();
                var duration = await CalculateTimesheetsDaysAndOvertimeHours(result, details);
                await SendTimesheetActionEmailToAccountAdmin(approver, customer, user, eventDetails, model.IsApproved, entity.Comment, duration.Item1, duration.Item2);
                await SendTimesheetActionEmailToEmployee(customer, user, eventDetails, model.IsApproved, entity.Comment, duration.Item1, duration.Item2, approver);
                await SendTimesheetActionNotificationToEmployee(customer, user, eventDetails, model.IsApproved, approver);
                await SendTimesheetActionNotificationToAccountAdmin(approver, user, eventDetails, model.IsApproved, customer);

                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        private async Task SendTimesheetActionNotificationToAccountAdmin(ApplicationUser approver,
         ApplicationUser user, SchedulerEvent schedulerEvent, bool isApprovedEmail,Customer customer)
        {
            var action = isApprovedEmail ? "approved" : "declined";
            var subject = $"Timesheet {action} by {customer.Organizations}";
            var bodyContent = $"{customer.Organizations} has {action} timesheet for {DateTimeFormatInfo.CurrentInfo.GetMonthName(schedulerEvent.StartDate.Month)}";
            var linkUrl = GetTimesheetLinkForAccountAdmin(schedulerEvent.Id);
            var createdByUser = new NotificationUserDto
            {
                FullName = customer.Organizations,
                Email = customer.Email,
            };
            var model = new NotificationDto
            {
                Subject = subject,
                Content = bodyContent,
                Link = linkUrl,
                AppplicationUserId =approver.Id,
                GroupKey = $"Timesheet {action} #{customer.Organizations}",
            };

            await _notificationService.CreateAsync(model);
        }
        private async Task SendTimesheetActionNotificationToEmployee(Customer customer,
            ApplicationUser user, SchedulerEvent schedulerEvent, bool isApprovedEmail,
            ApplicationUser approver)
        {
            var action = isApprovedEmail ? "approved" : "declined";
            var subject = $"Timesheet {action} by {approver.FullName}";
            var bodyContent = $"{approver.FullName} has {action} your timesheet for {DateTimeFormatInfo.CurrentInfo.GetMonthName(schedulerEvent.StartDate.Month)}";
            var linkUrl = GetTimesheetLinkForEmployee(schedulerEvent.Id);
            var createdByUser = new NotificationUserDto
            {
                Id = approver.Id,
                FullName = approver.FullName,
                Email = approver.Email,
            };
            var model = new NotificationDto
            {
                Subject = subject,
                Content = bodyContent,
                Link = linkUrl,
                AppplicationUserId = user.Id,
                GroupKey = $"Timesheet {action} #{approver.FullName}",
            };

            await _notificationService.CreateAsync(model);
        }


        private async Task<bool> SendTimesheetActionEmailToEmployee(Customer customer,
            ApplicationUser user, SchedulerEvent schedulerEvent, bool isApprovedEmail,
            string comment, decimal totalWorkingDays, decimal overtimeHours, ApplicationUser approver)
        {
            try
            {
                var subject = isApprovedEmail ? "Timesheet Approved." : "Timesheet Declined.";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.TimesheetActionsEmployee);
                var link = GetTimesheetLinkForCustomer(schedulerEvent.Id);
                emailTemplate = emailTemplate.Replace("[EmployeeName]", user.FullName)
                                             .Replace("[AdministrationContactName]", customer.AdministrationContactName)
                                             .Replace("[TimesheetAction]", isApprovedEmail ? "approved" : "declined")
                                             .Replace("[Month]", DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(schedulerEvent.StartDate.Month))
                                             .Replace("[Year]", schedulerEvent.StartDate.Year.ToString())
                                             .Replace("[LinkToTimesheet]", link)
                                             .Replace("[Approver]", customer.AdministrationContactName)
                                             .Replace("[WorkDuration]", LocaleHelper.FormatNumber(totalWorkingDays))
                                             .Replace("[Overtime]", LocaleHelper.FormatNumber(overtimeHours))
                                             .Replace("[Comment]", comment);
                if (!isApprovedEmail)
                {
                    var linkSection = $"<p>Please click on the link below to view this timesheet.<br /></p>" +
                                      $"<p><a href=\"{link}\" target=\"_blank\" style=\"background-color: #202A44; color: white; padding: 5px;\">View</a><br /></p>";
                    emailTemplate = emailTemplate.Replace("[LinkSection]", linkSection);
                }
                else
                {
                    var linkSection = $"<p>No further action is required from you.<br/></p>";
                    emailTemplate = emailTemplate.Replace("[LinkSection]", linkSection);
                }
                return await _emailService.SendEmail(user.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }

        private async Task<bool> SendTimesheetActionEmailToAccountAdmin(ApplicationUser approver, Customer customer,
         ApplicationUser user, SchedulerEvent schedulerEvent, bool isApprovedEmail,
         string comment, decimal totalWorkingDays, decimal overtimeHours)
        {
            try
            {
                var subject = isApprovedEmail ? "Timesheet Approved" : "Timesheet Declined";
                var link = GetTimesheetLinkForAccountAdmin(schedulerEvent.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.TimesheetActions);
                emailTemplate = emailTemplate.Replace("[AccountAdmin]", approver.FullName)
                                             .Replace("[EmployeeName]", user.FullName)
                                             .Replace("[AdministrationContactName]", customer.AdministrationContactName)
                                             .Replace("[TimesheetAction]", isApprovedEmail ? "approved" : "declined")
                                             .Replace("[Month]", DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(schedulerEvent.StartDate.Month))
                                              .Replace("[LinkToTimesheet]", link)
                                             .Replace("[Year]", schedulerEvent.StartDate.Year.ToString())
                                             .Replace("[WorkDuration]", LocaleHelper.FormatNumber(totalWorkingDays))
                                             .Replace("[Overtime]", LocaleHelper.FormatNumber(overtimeHours))
                                             .Replace("[Comment]", comment);
                if (!isApprovedEmail)
                {
                    var linkSection = $"<p>Please click on the link below to view this timesheet.<br /></p>" +
                                      $"<p><a href=\"{link}\" target=\"_blank\" style=\"background-color: #202A44; color: white; padding: 5px;\">View</a><br /></p>";
                    emailTemplate = emailTemplate.Replace("[LinkSection]", linkSection);
                }
                else
                {

                    var linkSection = $"<p>No further action is required from you.<br/></p>";
                    emailTemplate = emailTemplate.Replace("[LinkSection]", linkSection);
                }
                return await _emailService.SendEmail(approver.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }
        #region Private Methods
        private async Task<bool> SendTimesheetApprovedEmailToEmployee(string email, SchedulerEvent schedulerEvent,
            string fullName, string approverName, decimal totalWorkingDays, decimal overtimeHours)
        {
            try
            {
                var subject = "Timesheet Approved";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.TimesheetApproved);
                emailTemplate = emailTemplate.Replace("[FullName]", fullName)
                                             .Replace("[Month]", DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(schedulerEvent.StartDate.Month))
                                             .Replace("[Year]", schedulerEvent.StartDate.Year.ToString())
                                             .Replace("[Approver]", approverName)
                                             .Replace("[WorkDuration]", LocaleHelper.FormatNumber(totalWorkingDays))
                                             .Replace("[Overtime]", LocaleHelper.FormatNumber(overtimeHours));
                return await _emailService.SendEmail(email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }

        private async Task<bool> SendTimesheetDeclinedEmailToEmployee(string email, SchedulerEvent schedulerEvent,
           string fullName, string approverName, decimal totalWorkingDays, decimal overtimeHours, string declinedReason)
        {
            try
            {
                var subject = "Timesheet Declined";
                var link = GetTimesheetLinkForCustomer(schedulerEvent.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.TimesheetDeclined);
                emailTemplate = emailTemplate.Replace("[FullName]", fullName)
                                             .Replace("[Month]", DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(schedulerEvent.StartDate.Month))
                                             .Replace("[Year]", schedulerEvent.StartDate.Year.ToString())
                                             .Replace("[Approver]", approverName)
                                             .Replace("[WorkDuration]", LocaleHelper.FormatNumber(totalWorkingDays))
                                             .Replace("[Overtime]", LocaleHelper.FormatNumber(overtimeHours))
                                             .Replace("[LinkToTimesheet]", link)
                                             .Replace("[Comment]", declinedReason);
                return await _emailService.SendEmail(email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }

        private async Task<ApiResult<bool>> SendRequestTimesheetApprovalEmailToCustomer(string email, SchedulerEvent schedulerEvent, string administrationContactName,
             string employeeName, decimal totalWorkingDays, decimal overtimeHours)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogError(" Sender Email is missing.");
                    return ApiResult<bool>.Success(false);
                }
                var subject = $"Timesheet Approval Request from {employeeName}.";
                var link = GetTimesheetLinkForCustomer(schedulerEvent.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.RequestTimesheetApproval);
                emailTemplate = emailTemplate.Replace("[AdministrationContactName]", administrationContactName)
                                             .Replace("[EmployeeName]", employeeName)
                                             .Replace("[LinkToTimesheet]", link)
                                             .Replace("[Month]", DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(schedulerEvent.StartDate.Month))
                                             .Replace("[Year]", schedulerEvent.StartDate.Year.ToString())
                                             .Replace("[WorkDuration]", LocaleHelper.FormatNumber(totalWorkingDays))
                                             .Replace("[Overtime]", LocaleHelper.FormatNumber(overtimeHours));
                var emailResult = await _emailService.SendEmail(email, subject, emailTemplate);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        private async Task<bool> SendRequestTimesheetApprovalEmailToAccountAdmin(string email, SchedulerEvent schedulerEvent, string approverName,
                                    string employeeName, decimal totalWorkingDays, decimal overtimeHours)
        {
            try
            {
                var subject = $"Timesheet Approval Request from {employeeName}.";
                var link = GetTimesheetLinkForAccountAdmin(schedulerEvent.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.RequestTimesheetApproval);
                emailTemplate = emailTemplate.Replace("[AdministrationContactName]", approverName)
                                             .Replace("[EmployeeName]", employeeName)
                                             .Replace("[LinkToTimesheet]", link)
                                             .Replace("[Month]", DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(schedulerEvent.StartDate.Month))
                                             .Replace("[Year]", schedulerEvent.StartDate.Year.ToString())
                                             .Replace("[WorkDuration]", LocaleHelper.FormatNumber(totalWorkingDays))
                                             .Replace("[Overtime]", LocaleHelper.FormatNumber(overtimeHours));
                return await _emailService.SendEmail(email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }
        public string GetTimesheetLinkForCustomer(Guid schedulerEventId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            var encryptedId = _protectionService.Encode(schedulerEventId.ToString());
            return $"{baseUrl}timesheet/approval/{encryptedId}";
        }
        public string GetTimesheetLinkForAccountAdmin(Guid schedulerEventId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            return $"{baseUrl}admin/timesheets/time-sheets-review-calendar/{schedulerEventId}";
        }
        public string GetTimesheetLinkForEmployee(Guid schedulerEventId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl.TrimEnd('/'));
            return $"{baseUrl}/admin/timesheets/time-sheets-calendar/{schedulerEventId}";
        }
        public async Task<ApiResult<bool>> SendTimesheetApprovalEmailToCustomer(Guid schedulerEventId)
        {
            var entity = (await _schedulersRepository.QueryAsync(x => x.Id == schedulerEventId, include: entities => entities.Include(e => e.ApplicationUser).Include(e => e.Customer))).FirstOrDefault();
            var details = (await _schedulersDetailsRepository.QueryAsync(x => x.SchedulerEventId == schedulerEventId)).ToList();
            var duration = await CalculateTimesheetsDaysAndOvertimeHours(entity, details);
            return await SendRequestTimesheetApprovalEmailToCustomer(entity.Customer.AdministrationContactEmail, entity, entity.Customer.AdministrationContactName, entity.ApplicationUser.FullName, duration.Item1
                , duration.Item2);
        }

        private async Task<(decimal, decimal)> CalculateTimesheetsDaysAndOvertimeHours(SchedulerEvent schedulerEvent, List<SchedulerEventDetails> schedulerEventDetails)
        {

            var eventTypes = (await _schedulerEventTypeService.Search()).Result;
            var timesheetEventId = eventTypes.FirstOrDefault(x => x.Name.Equals("timesheet", StringComparison.InvariantCultureIgnoreCase)).Id;
            var overtimeId = eventTypes.FirstOrDefault(x => x.Name.Equals("overtime", StringComparison.InvariantCultureIgnoreCase)).Id;
            var timesheetEvents = schedulerEventDetails.Where(c => c.Date.Value.Month == schedulerEvent.StartDate.Month && c.EventTypeId == timesheetEventId).ToList();
            var overtimeEvents = schedulerEventDetails.Where(c => c.Date.Value.Month == schedulerEvent.StartDate.Month && c.EventTypeId == overtimeId).ToList();
            var totalWorkingDays = timesheetEvents.Sum(item => item.Duration);
            var overtimeHours = overtimeEvents.Sum(item => item.Duration);
            return (totalWorkingDays, overtimeHours);
        }

        public async Task<bool> IsTimesheetApproved(Guid customerId, List<int> employeeIds, MonthDateRange month)
        {
            var result = (await _schedulersRepository.QueryAsync(x => x.CustomerId == customerId
            && x.StartDate.Month == month.StartDate.Month
            && x.StartDate.Year == month.StartDate.Year
            && employeeIds.Contains(x.UserId)
            && x.AdminStatus.StatusKey == SchedulerStatusesEnum.CUSTOMER_APPROVED.ToString()));

            if (result.Count() < employeeIds.Count)
            {
                _logger.LogInformation($"All employees timesheets are not in customer approved status for {customerId}.");
                return false;

            }
            var IsApproved = result.All(x => employeeIds.Contains(x.UserId));
            return IsApproved;

        }
        public async Task<ApiResult<bool>> SendNotificationToEmployee(List<SchedulerEventResponseDto> schedulerEvents)
        {
            var notifications = new List<NotificationDto>();
            foreach (var schedulerEvent in schedulerEvents)
            {
                var subject = $"Timesheet reminder for month {DateTimeFormatInfo.CurrentInfo.GetMonthName(schedulerEvent.StartDate.Month)}";
                var bodyContent = $"Timesheet reminder for month {DateTimeFormatInfo.CurrentInfo.GetMonthName(schedulerEvent.StartDate.Month)}";
                var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
                var linkUrl = GetTimesheetLinkForEmployee(schedulerEvent.Id);
                var model = new NotificationDto
                {
                    Subject = subject,
                    Content = bodyContent,
                    Link = linkUrl,
                    AppplicationUserId = schedulerEvent.ApplicationUser.Id,
                    GroupKey = $"Timesheet reminder",
                };
                notifications.Add(model);
            }
            return await _notificationService.BulkCreateAsync(notifications);
        }

        #endregion
    }
}
