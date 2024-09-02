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


        public AbsenceRequestService(IGenericRepository<AbsenceRequest> absenceRequestRepository,
            ILogger<AbsenceRequestService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILeaveStatusesService leaveStatusesService,
            IApplicationUserService applicationUserService,
            IConfiguration configuration,
            IEmailService emailService,
            ITemplateService templateService,
            IHolidayAllocationService holidayAllocationService,
            IGenericRepository<ApplicationUser> applicationUserRepository)
        {
            _absenceRequestRepository = absenceRequestRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _leaveStatusesService = leaveStatusesService;
            _applicationUserService = applicationUserService;
            _holidayAllocationService = holidayAllocationService;
            _emailService = emailService;
            _templateService = templateService;
            _configuration = configuration;
            _applicationUserRepository = applicationUserRepository;
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
                var leaveBalanceResult = await _holidayAllocationService.GetEmployeeLeaveBalance(userId);
                var leaveBalance = leaveBalanceResult.Result;
                if (leaveBalance.leavesLeft<=model.Duration) {
                    return ApiResult<AbsenceRequestResponseDto>.Failure(ValidationCodes.AbsenceRequestedProhibited, "You don’t have enough balance to request holiday");
                }
                model.UserId = userId;
                model.LeaveStatusId = requestedStatusId;
                var entity = _mapper.Map<AbsenceRequest>(model);
                await _absenceRequestRepository.InsertAsync(entity);
                var mappedEntity = _mapper.Map<AbsenceRequestResponseDto>(entity);
                var accountAdminResult = await _applicationUserService.GetAccountAdmins();
                var user = (await _applicationUserRepository.FirstOrDefaultAsync(x => x.Id == mappedEntity.UserId));
                foreach (var accountAdmin in accountAdminResult.Result.ToList())
                {
                   await SendAbsenceRequestEmailToAccountAdmin(accountAdmin, user, mappedEntity);
                }
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
                        Date = e.Date,
                        LeaveStatusId = e.LeaveStatusId,
                        LeaveStatusName=e.LeaveStatuses.Name,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt,
                        Comment=e.Comment,
                        Duration=e.Duration,
                        UserName=e.ApplicationUser.FullName,
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
                var oldComment = entity.Comment;
                var oldDuration = entity.Duration;

                model.UserId = userId;
                model.LeaveStatusId = requestedStatusId;
                _mapper.Map(model, entity);
                entity = await _absenceRequestRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<AbsenceRequestResponseDto>(entity);
                var accountAdminResult = await _applicationUserService.GetAccountAdmins();
                var user = (await _applicationUserRepository.FirstOrDefaultAsync(x => x.Id == mappedEntity.UserId));
                foreach (var accountAdmin in accountAdminResult.Result.ToList())
                {
                    await SendAbsenceRequestEmailToAccountAdmin(accountAdmin, user, mappedEntity, oldComment, oldDuration, true);
                }
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

        private async Task<bool> SendAbsenceRequestEmailToAccountAdmin(ApplicationUserDto accountAdmin,
            ApplicationUser user,
            AbsenceRequestResponseDto absenceRequest,
            string? oldComment = null,
            decimal? oldDuration = null,
            bool isUpdated = false)
        {
            try
            {
                var subject = !isUpdated ? "Absence Request" : "Absence Request Updated";
                var link = GetAbsenceApprovalLinkForAccountAdmin(absenceRequest.Id);
                var action = !isUpdated ? "submitted" : "updated";
                var duration = !isUpdated ? LocaleHelper.FormatNumber(absenceRequest.Duration) : $"{oldDuration} is updated to {LocaleHelper.FormatNumber(absenceRequest.Duration)}";
                var comment = !isUpdated ? absenceRequest.Comment : $"{oldComment} is updated to {absenceRequest.Comment}";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.AbsenceRequestApproval);
                emailTemplate = emailTemplate.Replace("[AccountAdminName]", accountAdmin.FullName)
                                             .Replace("[EmployeeName]", user.FullName)
                                             .Replace("[Action]", action)
                                             .Replace("[LinkToAbsenceRequests]", link)
                                             .Replace("[Date]", LocaleHelper.FormatDate(absenceRequest.Date))
                                             .Replace("[Duration]", duration)
                                             .Replace("[Comment]", comment);
                return await _emailService.SendEmail(accountAdmin.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }

        public string GetAbsenceApprovalLinkForAccountAdmin(Guid schedulerEventId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            return $"{baseUrl}admin/timesheets/absence-approval";
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

            filter.GetValue<DateTime>("Date", (v) =>
            {
                entities = entities.Where(e => e.Date < v);
            }, OperatorType.lessthan, true);

            filter.GetValue<DateTime>("Date", (v) =>
            {
                entities = entities.Where(e => e.Date > v);
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
                    orders = orders.OrderByDescending(o => o.Date);
                    return orders;
                }
                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.Comment).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Comment);
                    }
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.Date).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Date);
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
                    if (columnName.ToUpper() == nameof(AbsenceRequestResponseDto.Date).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.Date);
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


    }
}
