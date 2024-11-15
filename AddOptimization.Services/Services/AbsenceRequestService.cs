﻿using AddOptimization.Contracts.Dto;
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
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.Contracts.Constants;
using System.Globalization;
using AddOptimization.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using AddOptimization.Data.Repositories;
using System.Reflection.Metadata.Ecma335;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Enums;
using Org.BouncyCastle.Bcpg;


namespace AddOptimization.Services.Services
{
    public class AbsenceRequestService : IAbsenceRequestService
    {
        private readonly IGenericRepository<AbsenceRequest> _absenceRequestRepository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILogger<AbsenceRequestService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILeaveStatusesService _leaveStatusesService;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IHolidayAllocationService _holidayAllocationService;
        private readonly ICustomerEmployeeAssociationService _customerEmployeeAssociationService;
        private readonly IPublicHolidayService _publicHolidayService;
        private readonly INotificationService _notificationService;
        private readonly IGenericRepository<HolidayAllocation> _holidayAllocationRepository;
        private readonly IAbsenceApprovalService _absenceApprovalService;
        public AbsenceRequestService(IGenericRepository<AbsenceRequest> absenceRequestRepository,
            ILogger<AbsenceRequestService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILeaveStatusesService leaveStatusesService,
            IApplicationUserService applicationUserService,
            IConfiguration configuration,
            IEmailService emailService,
            IGenericRepository<HolidayAllocation> holidayAllocationRepository,
            ITemplateService templateService,
            ICustomerEmployeeAssociationService customerEmployeeAssociationService,
            IPublicHolidayService publicHolidayService,
            IHolidayAllocationService holidayAllocationService,
            INotificationService notificationService,
            IAbsenceApprovalService absenceApprovalService,
            IGenericRepository<ApplicationUser> applicationUserRepository)
        {
            _absenceRequestRepository = absenceRequestRepository;
            _logger = logger;
            _mapper = mapper;
            _holidayAllocationRepository = holidayAllocationRepository;
            _httpContextAccessor = httpContextAccessor;
            _leaveStatusesService = leaveStatusesService;
            _applicationUserService = applicationUserService;
            _holidayAllocationService = holidayAllocationService;
            _emailService = emailService;
            _templateService = templateService;
            _configuration = configuration;
            _applicationUserRepository = applicationUserRepository;
            _customerEmployeeAssociationService = customerEmployeeAssociationService;
            _publicHolidayService = publicHolidayService;
            _absenceApprovalService = absenceApprovalService;
            _notificationService = notificationService;
        }



        public async Task<ApiResult<AbsenceRequestResponseDto>> Create(AbsenceRequestRequestDto model)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var leaveStatuses = (await _leaveStatusesService.Search(null)).Result;

                var requestedStatusId = leaveStatuses.First(x => x.Name.Equals(LeaveStatusesEnum.Requested.ToString(), StringComparison.InvariantCultureIgnoreCase)).Id;
                var approvedStatusId = leaveStatuses.First(x => x.Name.Equals(LeaveStatusesEnum.Approved.ToString(), StringComparison.InvariantCultureIgnoreCase)).Id;


                var isExisting = await _absenceRequestRepository.IsExist(s => s.UserId == userId && (!s.IsDeleted) && (s.LeaveStatusId == requestedStatusId || s.LeaveStatusId == approvedStatusId) && ((s.StartDate <= model.StartDate && model.StartDate <= s.EndDate) || (s.StartDate <= model.EndDate && model.EndDate <= s.EndDate) || (s.StartDate == model.StartDate) || (model.StartDate <= s.StartDate && model.EndDate >= s.StartDate)));

                if (isExisting)
                {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "You have already submitted requested for this date.");
                }
                var association = (await _customerEmployeeAssociationService.GetAssociatedCustomers(userId)).Result.ToList();
                var publicHoliday = (await _publicHolidayService.SearchAllPublicHoliday()).Result.ToList();
                var durationExcludingWeekends = (await GetDurationExcludingWeekends(model.StartDate, model.EndDate, association, publicHoliday, userId, model?.Duration)).Result;
                if (durationExcludingWeekends == 0)
                {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "Cannot raise absence request as there is a public holiday.");
                }
                var currentYear = DateTime.UtcNow.Year;
                var leaveBalanceResult = await _holidayAllocationService.GetEmployeeLeaveBalance(userId);
                var leaveBalance = leaveBalanceResult.Result;

                var requestedAbsence = await GetAllAbsenseRequested(userId);
                var requestedAbsenceDuration = requestedAbsence.Result;
                var totalDurations = durationExcludingWeekends + requestedAbsenceDuration;

                if (model.StartDate.Value.Year != currentYear)
                {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "Holiday not allocated for this year");
                }
                else if (leaveBalance.leavesLeft < totalDurations)
                {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "You don’t have enough balance to request holiday");
                }

                model.UserId = userId;
                model.Duration = durationExcludingWeekends;
                model.LeaveStatusId = requestedStatusId;
                model.IsActive = true;
                var entity = _mapper.Map<AbsenceRequest>(model);
                await _absenceRequestRepository.InsertAsync(entity);
                var mappedEntity = _mapper.Map<AbsenceRequestResponseDto>(entity);
                var accountAdminResult = await _applicationUserService.GetAccountAdmins();
                var user = (await _applicationUserRepository.FirstOrDefaultAsync(x => x.Id == mappedEntity.UserId));
                var accountAdmins = accountAdminResult.Result.ToList();
                foreach (var accountAdmin in accountAdminResult.Result.ToList())
                {
                    await SendAbsenceRequestEmailToAccountAdmin(accountAdmin, user, mappedEntity);
                }
                await SendNotificationToAccountAdmin(accountAdmins, user, mappedEntity);
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


        public async Task<PagedApiResult<AbsenceRequestResponseDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _absenceRequestRepository.QueryAsync(
                    e => !e.IsDeleted,
                    include: entities => entities
                        .Include(e => e.LeaveStatuses)
                        .Include(e => e.CreatedByUser)
                        .Include(e => e.UpdatedByUser)
                        .Include(e => e.ApplicationUser),
                    orderBy: x => x.OrderByDescending(x => x.CreatedAt)
                );

                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);

                var pagedResult = PageHelper<AbsenceRequest, AbsenceRequestResponseDto>.ApplyPaging(
                    entities,
                    filters,
                    entities => entities.Select(e => new AbsenceRequestResponseDto
                    {
                        Id = e.Id,
                        UserId = e.UserId,
                        LeaveStatusId = e.LeaveStatusId,
                        LeaveStatusName = e.LeaveStatuses.Name,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt,
                        Comment = e.Comment,
                        Duration = e.Duration,
                        UserName = e.ApplicationUser.FullName,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                    }).ToList()
                );

                return PagedApiResult<AbsenceRequestResponseDto>.Success(pagedResult);
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
                var isExisting = await _absenceRequestRepository.IsExist(s => s.Id != model.Id && s.UserId == userId && (!s.IsDeleted) && (s.LeaveStatusId == requestedStatusId || s.LeaveStatusId == approvedStatusId) && ((s.StartDate <= model.StartDate && model.StartDate <= s.EndDate) || (s.StartDate <= model.EndDate && model.EndDate <= s.EndDate) || (s.StartDate == model.StartDate) || (model.StartDate <= s.StartDate && model.EndDate >= s.StartDate)));

                if (isExisting)
                {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "You have already submitted requested for this date.");
                }

                var association = (await _customerEmployeeAssociationService.GetAssociatedCustomers(userId)).Result.ToList();
                var publicHoliday = (await _publicHolidayService.SearchAllPublicHoliday()).Result.ToList();
                var durationExcludingWeekends = (await GetDurationExcludingWeekends(model.StartDate, model.EndDate, association, publicHoliday, userId, model?.Duration)).Result;
                if (durationExcludingWeekends == 0)
                {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "Cannot raise absence request as there is a public holiday.");
                }

                var leaveBalanceResult = (await _holidayAllocationService.GetEmployeeLeaveBalance(userId)).Result;
                var currentYear = DateTime.UtcNow.Year;
                var associations = await _absenceRequestRepository.QueryAsync(e => e.UserId == userId && !e.IsDeleted && e.Id != model.Id, include: entities => entities.Include(e => e.ApplicationUser).Include(e => e.LeaveStatuses));
                var requestedAssociation = associations.Where(e => e.LeaveStatuses.Name.ToLower() == LeaveStatusesEnum.Requested.ToString().ToLower() && e.StartDate.HasValue && e.StartDate.Value.Year == currentYear && !e.IsDeleted);
                var totalRequestedDuration = requestedAssociation.Sum(e => e.Duration);


                var totalDurations = durationExcludingWeekends + totalRequestedDuration;
                if (model.StartDate.Value.Year != currentYear)
                {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "Holiday not allocated for this year");
                }
                else if (leaveBalanceResult.leavesLeft < totalDurations)
                {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "You don’t have enough balance to request holiday");
                }


                var oldComment = entity.Comment;
                var oldDuration = durationExcludingWeekends;

                model.UserId = userId;
                model.LeaveStatusId = requestedStatusId;
                model.Duration = durationExcludingWeekends;
                _mapper.Map(model, entity);
                entity = await _absenceRequestRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<AbsenceRequestResponseDto>(entity);
                var accountAdminResult = await _applicationUserService.GetAccountAdmins();
                var user = (await _applicationUserRepository.FirstOrDefaultAsync(x => x.Id == mappedEntity.UserId));
                var accountAdmins = accountAdminResult.Result.ToList();
                foreach (var accountAdmin in accountAdminResult.Result.ToList())
                {
                    await SendAbsenceRequestEmailToAccountAdmin(accountAdmin, user, mappedEntity, oldComment, oldDuration, true);
                }
                await SendNotificationToAccountAdmin(accountAdmins, user, mappedEntity);
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
                entity.IsActive = false;
                await _absenceRequestRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private async Task<bool> SendAbsenceRequestEmailToAccountAdmin(ApplicationUserDto accountAdmin,
            ApplicationUser user,
            AbsenceRequestResponseDto absenceRequest,
            string? oldComment = null,
            decimal? oldDuration = null,
            bool isUpdated = false)
        {
            try
            {
                var subject = !isUpdated ? $"Absence Request from {user.FullName}." : $"Absence Request from {user.FullName} Updated.";
                var link = GetAbsenceApprovalLinkForAccountAdmin(absenceRequest.Id);
                var action = !isUpdated ? "submitted" : "updated";
                var duration = !isUpdated ? LocaleHelper.FormatNumber(absenceRequest.Duration) : LocaleHelper.FormatNumber(absenceRequest.Duration);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.AbsenceRequestApproval);
                var startDate = absenceRequest.StartDate.HasValue ? LocaleHelper.FormatDate(absenceRequest.StartDate.Value) : "";
                var endDate = absenceRequest.EndDate.HasValue ? LocaleHelper.FormatDate(absenceRequest.EndDate.Value) : "";
                var dateRange = absenceRequest.StartDate.HasValue ? absenceRequest.EndDate.HasValue ? $"{LocaleHelper.FormatDate(absenceRequest.StartDate.Value)} - {LocaleHelper.FormatDate(absenceRequest.EndDate.Value)}" : LocaleHelper.FormatDate(absenceRequest.StartDate.Value) : "";

                emailTemplate = emailTemplate.Replace("[AccountAdminName]", accountAdmin.FullName)
                                             .Replace("[EmployeeName]", user.FullName)
                                             .Replace("[Action]", action)
                                             .Replace("[LinkToAbsenceRequests]", link)
                                             .Replace("[Date]", dateRange)
                                             .Replace("[Duration]", duration)
                                             .Replace("[Comment]", absenceRequest.Comment);
                return await _emailService.SendEmail(accountAdmin.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }
        private async Task SendNotificationToAccountAdmin(List<ApplicationUserDto> accountAdmins, ApplicationUser user, AbsenceRequestResponseDto absenceRequest)
        {
            var subject = "New absence request submitted";
            var startDate = LocaleHelper.FormatDate(absenceRequest.StartDate.Value);
            var endDate = absenceRequest.EndDate.HasValue ? LocaleHelper.FormatDate(absenceRequest.EndDate.Value) : string.Empty;
            var bodyContent = $"{user.FullName} has requested absence request " + (endDate == null? $"for {startDate}" : $"from {startDate} to {endDate}");
            var linkUrl = GetAbsenceApprovalLinkForAccountAdmin(absenceRequest.Id);
            var notifications = new List<NotificationDto>();
            var createdByUser = new NotificationUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
            };
            foreach (var accountAdmin in accountAdmins)
            {
                var model = new NotificationDto
                {
                    Subject = subject,
                    Content = bodyContent,
                    Link = linkUrl,
                    AppplicationUserId = accountAdmin.Id,
                    GroupKey = $"Absence request #{user.FirstName}",
                };
                notifications.Add(model);
            }

            await _notificationService.BulkCreateAsync(notifications);
        }


        public string GetAbsenceApprovalLinkForAccountAdmin(Guid schedulerEventId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            return $"{baseUrl}admin/timesheets/absence-approval?sidenav=collapsed";
        }


        private IQueryable<AbsenceRequest> ApplyFilters(IQueryable<AbsenceRequest> entities, PageQueryFiterBase filter)
        {

            filter.GetValue<string>("UserId", (v) =>
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                entities = entities.Where(e => e.UserId == userId);
            });
            filter.GetValue<string>("Comment", (v) =>
            {
                entities = entities.Where(e => e.Comment != null && e.Comment.ToLower().Contains(v.ToLower()));
            });

            filter.GetValue<string>("Duration", (v) =>
            {
                if (!string.IsNullOrEmpty(v))
                {
                    entities = entities.Where(e => e.Duration.ToString().StartsWith(v));
                }
            });

            filter.GetValue<string>("leaveStatusName", (v) =>
            {
                entities = entities.Where(e => e.LeaveStatuses.Name != null && e.LeaveStatuses.Name.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<int>("leaveStatusId", (v) =>
            {
                entities = entities.Where(e => e.LeaveStatusId == v);
            });

            filter.GetValue<string>("UserName", (v) =>
            {
                entities = entities.Where(e => e.ApplicationUser.FullName != null && e.ApplicationUser.FullName.ToLower().Contains(v.ToLower()));
            });

            filter.GetValue<DateTime>("startDate", (v) =>
            {
                entities = entities.Where(e => e.StartDate < v);
            }, OperatorType.lessthan, true);

            filter.GetValue<DateTime>("startDate", (v) =>
            {
                entities = entities.Where(e => e.StartDate > v);
            }, OperatorType.greaterthan, true);

            filter.GetValue<DateTime>("endDate", (v) =>
            {
                entities = entities.Where(e => e.EndDate < v);
            }, OperatorType.lessthan, true);

            filter.GetValue<DateTime>("endDate", (v) =>
            {
                entities = entities.Where(e => e.EndDate > v);
            }, OperatorType.greaterthan, true);

            filter.GetValue<int>("employeeId", (v) =>
            {
                entities = entities.Where(e => e.UserId == v);
            });

            return entities;
        }

        private IQueryable<AbsenceRequest> ApplySorting(IQueryable<AbsenceRequest> orders, SortModel sort)
        {
            try
            {
                if (sort?.Name == null)
                {
                    orders = orders.OrderByDescending(o => o.StartDate);
                    return orders;
                }
                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.Comment).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Comment);
                    }
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.StartDate).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.StartDate);
                    }
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.EndDate).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.EndDate);
                    }
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.LeaveStatusName).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.LeaveStatuses.Name);
                    }
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.UpdatedBy).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.CreatedByUser.FullName);
                    }

                }
                else
                {
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.Comment).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.Comment);
                    }
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.StartDate).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.StartDate);
                    }
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.EndDate).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.EndDate);
                    }
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.LeaveStatusName).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.LeaveStatuses.Name);
                    }
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.UpdatedBy).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.CreatedByUser.FullName);
                    }
                }
                return orders;

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return orders;
            }
        }

        public async Task<ApiResult<decimal>> GetAllAbsenseRequested(int employeeId)
        {
            try
            {
                var currentYear = DateTime.UtcNow.Year;
                var associations = await _absenceRequestRepository.QueryAsync(e => e.UserId == employeeId && !e.IsDeleted, include: entities => entities.Include(e => e.ApplicationUser).Include(e => e.LeaveStatuses));
                var requestedAssociation = associations.Where(e => e.LeaveStatuses.Name.ToLower() == LeaveStatusesEnum.Requested.ToString().ToLower() && e.StartDate.HasValue && e.StartDate.Value.Year == currentYear && !e.IsDeleted);
                var totalRequestedDuration = requestedAssociation.Sum(e => e.Duration);
                return ApiResult<decimal>.Success(totalRequestedDuration);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<decimal>> GetDurationExcludingWeekends(DateTime? startDate, DateTime? endDate, List<CustomerEmployeeAssociationDto> association, List<PublicHolidayResponseDto> publicHoliday, int userId, decimal? duration)
        {
            try
            {
                bool hasAssociations = association.Any(a => a.EmployeeId == userId);
                var associatedCountries = association.Where(a => a.EmployeeId == userId).Select(a => a.PublicHolidayCountryId).Distinct().ToList();

                if (!startDate.HasValue)
                {
                    return ApiResult<decimal>.Success(0);
                }

                int totalDays = 0;
                var currentDate = startDate.Value;
                DateTime finalDate = endDate ?? startDate.Value;
                while (currentDate <= finalDate)
                {
                    if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        bool allCountriesHaveHoliday = associatedCountries.All(countryId => publicHoliday.Any(ph => ph.CountryId == countryId && ph.Date.Date == currentDate.Date));

                        if (duration == null)
                        {
                            if (!allCountriesHaveHoliday || !hasAssociations)
                            {
                                totalDays++;
                            }
                        }
                        else
                        {
                            if (!allCountriesHaveHoliday || !hasAssociations)
                            {
                                return ApiResult<decimal>.Success((decimal)duration);
                            }
                            else
                            {
                                return ApiResult<decimal>.Success(0);
                            }

                        }
                    }
                    currentDate = currentDate.AddDays(1);
                }

                return ApiResult<decimal>.Success(totalDays);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return ApiResult<decimal>.Failure("An error occurred while calculating the duration.");
            }
        }


        public async Task<ApiResult<decimal>> GetDurations(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var association = (await _customerEmployeeAssociationService.GetAssociatedCustomers(userId)).Result.ToList();
                var publicHoliday = (await _publicHolidayService.SearchAllPublicHoliday()).Result.ToList();
                var durationExcludingWeekends = (await GetDurationExcludingWeekends(startDate, endDate, association, publicHoliday, userId, null)).Result;

                return ApiResult<decimal>.Success(durationExcludingWeekends);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<LeaveHistoryResponseDto>> GetLeaveHistory(int employeeId)
        {
            try
            {
                var currentYear = DateTime.UtcNow.Year;

                var entities = await _absenceRequestRepository.QueryAsync(
                    e => e.UserId == employeeId && !e.IsDeleted && e.CreatedAt.HasValue && e.CreatedAt.Value.Year == currentYear,
                    include: entities => entities.Include(e => e.ApplicationUser)
                );

                var holidayAllocationResult = await _holidayAllocationService.GetAllocatedHolidays(employeeId);
                var totalHolidayAllocated = holidayAllocationResult.Result?.Holidays ?? 0;

                var leaveTakenResult = await _absenceApprovalService.GetAllAbsenseApproval(employeeId);
                var leaveTaken = leaveTakenResult?.Result ?? 0;
                var remainingLeaves = totalHolidayAllocated - leaveTaken;

                var mappedEntities = entities.OrderByDescending(e => e.CreatedAt)
                    .Select(e => new AbsenceRequestResponseDto
                    {
                        Comment = e.Comment,
                        UserId = e.UserId,
                        LeaveStatusId = e.LeaveStatusId,
                        LeaveStatusName = e.LeaveStatuses.Name,
                        Duration = e.Duration,
                        StartDate = e.StartDate,
                        CreatedAt = e.CreatedAt,
                        EndDate = e.EndDate
                    }).ToList();

                var response = new LeaveHistoryResponseDto
                {
                    TotalAllocatedHoliday = totalHolidayAllocated,
                    LeaveTaken = leaveTaken,
                    LeavesLeft = remainingLeaves,
                    AbsenceRequests = mappedEntities
                };

                return ApiResult<LeaveHistoryResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

    }
}
