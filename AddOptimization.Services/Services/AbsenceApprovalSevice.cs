﻿using AddOptimization.Contracts.Constants;
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
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace AddOptimization.Services.Services
{
    public class AbsenceApprovalSevice : IAbsenceApprovalService
    {
        private readonly IGenericRepository<AbsenceRequest> _absenceApprovalRepository;
        private readonly ILogger<AbsenceApprovalSevice> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILeaveStatusesService _leaveStatusesService;
        private readonly IGenericRepository<LeaveStatuses> _leaveStatusesRepository;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly INotificationService _notificationService;
        

        public AbsenceApprovalSevice(IGenericRepository<AbsenceRequest> absenceApprovalRepository,
            IGenericRepository<LeaveStatuses> leaveStatusesRepository,
            ILogger<AbsenceApprovalSevice> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILeaveStatusesService leaveStatusesService,
            IConfiguration configuration,
            IEmailService emailService,
            ITemplateService templateService,
            IGenericRepository<ApplicationUser> applicationUserRepository,
            IApplicationUserService applicationUserService,
            INotificationService notificationService
            )
        {
            _absenceApprovalRepository = absenceApprovalRepository;
            _leaveStatusesRepository = leaveStatusesRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _leaveStatusesService = leaveStatusesService;
            _emailService = emailService;
            _templateService = templateService;
            _configuration = configuration;
            _applicationUserRepository = applicationUserRepository;
            _applicationUserService = applicationUserService;
            _notificationService = notificationService;

        }
        public async Task<PagedApiResult<AbsenceRequestResponseDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _absenceApprovalRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.LeaveStatuses).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.ApplicationUser));
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);
                var pagedResult = PageHelper<AbsenceRequest, AbsenceRequestResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new AbsenceRequestResponseDto
                {
                    Id = e.Id,
                    Comment = e.Comment,
                    UserId = e.UserId,
                    LeaveStatusId = e.LeaveStatusId,
                    LeaveStatusName = e.LeaveStatuses.Name,
                    UpdatedBy = e.CreatedByUser.FullName,
                    Duration = e.Duration,
                    UserName = e.ApplicationUser.FullName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,

                }).ToList());
                var retVal = pagedResult;
                return PagedApiResult<AbsenceRequestResponseDto>.Success(retVal);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<bool>> AbsenceAction(AdminApprovalRequestActionDto model)
        {
            try
            {
                var entity = await _absenceApprovalRepository.FirstOrDefaultAsync(t => t.Id == model.Id);

                if (model.IsApproved)
                {
                    var approvedStatusId = (await _leaveStatusesRepository.FirstOrDefaultAsync(x => x.Name.ToLower() == LeaveStatusesEnum.Approved.ToString())).Id;
                    entity.LeaveStatusId = approvedStatusId;
                }
                else
                {
                    var rejectedStatusId = (await _leaveStatusesRepository.FirstOrDefaultAsync(x => x.Name.ToLower() == LeaveStatusesEnum.Rejected.ToString())).Id;
                    entity.LeaveStatusId = rejectedStatusId;
                }
                entity.Comment = model.Comment;
                var result = await _absenceApprovalRepository.UpdateAsync(entity);
                var accountAdminResult = await _applicationUserService.GetAccountAdmins();
                var accountAdmin = accountAdminResult.Result.FirstOrDefault(admin => admin.Id == result.UpdatedByUserId);
                var user = await _applicationUserRepository.FirstOrDefaultAsync(x => x.Id == entity.UserId);
                if (model.IsApproved)
                {
                    await SendAbsenceRequestActionEmailEmployee(model.IsApproved, accountAdmin, user, result);
                    await SendNotificationToEmployee(model.IsApproved, accountAdmin, user, result);
                }
                else
                {                 
                    await SendAbsenceRequestActionEmailEmployee(model.IsApproved, accountAdmin, user, result);
                    await SendNotificationToEmployee(model.IsApproved, accountAdmin, user, result);
                }

                return ApiResult<bool>.Success(true);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        private IQueryable<AbsenceRequest> ApplyFilters(IQueryable<AbsenceRequest> entities, PageQueryFiterBase filter)
        {

            filter.GetValue<string>("Comment", (v) =>
            {
                entities = entities.Where(e => e.Comment != null && e.Comment.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<string>("Duration", (v) =>
            {
                if (!string.IsNullOrEmpty(v))
                {
                    entities = entities.Where(e => e.Duration == Convert.ToDecimal(v));
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

            filter.GetList<DateTime>("duedateRange", (v) =>
            {
                entities = entities.Where(e => e.StartDate <= v.Max().Date);
            }, OperatorType.lessthan, true);

            filter.GetList<DateTime>("duedateRange", (v) =>
            {
                entities = entities.Where(e => e.StartDate >= v.Min().Date);
            }, OperatorType.greaterthan, true);

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

        public async Task<ApiResult<decimal>> GetAllAbsenseApproval(int employeeId)
        {
            try
            {
                var associations = await _absenceApprovalRepository.QueryAsync(e => e.UserId == employeeId && !e.IsDeleted, include: entities => entities.Include(e => e.ApplicationUser).Include(e => e.LeaveStatuses));
                var currentYear = DateTime.UtcNow.Year;
                var approvedAssociations = associations.Where(e => e.LeaveStatuses.Name.ToLower() == LeaveStatusesEnum.Approved.ToString().ToLower() && e.StartDate.HasValue && e.StartDate.Value.Year == currentYear && !e.IsDeleted);
                var totalApprovedDuration = approvedAssociations.Sum(e => e.Duration);
                return ApiResult<decimal>.Success(totalApprovedDuration);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private async Task<bool> SendAbsenceRequestActionEmailEmployee(bool isApproved, ApplicationUserDto accountAdmin, ApplicationUser user, AbsenceRequest absenceRequest)
        {
            try
            {
                var subject = isApproved ? "Absence Request Approved." : "Absence Request Declined.";
                var action = isApproved ? "Approved" : "Declined";
                var link = GetAbsenceRequestLinkForAccountAdmin(absenceRequest.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.AbsenceRequestActions);
                var dateRange = absenceRequest.StartDate.HasValue ? absenceRequest.EndDate.HasValue ? $"{LocaleHelper.FormatDate(absenceRequest.StartDate.Value)} - {LocaleHelper.FormatDate(absenceRequest.EndDate.Value)}" : LocaleHelper.FormatDate(absenceRequest.StartDate.Value) : "";
                emailTemplate = emailTemplate.Replace("[AccountAdminName]", accountAdmin.FullName)
                                             .Replace("[EmployeeName]", user.FullName)
                                             .Replace("[Action]", action)
                                             .Replace("[Date]", dateRange)
                                             .Replace("[Link]", link)
                                             .Replace("[Duration]", LocaleHelper.FormatNumber(absenceRequest.Duration))
                                             .Replace("[Comment]", absenceRequest.Comment);
                if (!isApproved)
                {
                    var linkSection = $"<p>Please click on the link below to view absence request.<br /></p>" +
                                      $"<p><a href=\"{link}\" target=\"_blank\" style=\"background-color: #202A44; color: white; padding: 5px;\">View</a><br /></p>";
                    emailTemplate = emailTemplate.Replace("[LinkSection]", linkSection);
                }
                else
                {
                    emailTemplate = emailTemplate.Replace("[LinkSection]",string.Empty);
                }
                return await _emailService.SendEmail(user.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }
        private async Task SendNotificationToEmployee(bool isApproved,ApplicationUserDto accountAdmin, ApplicationUser user, AbsenceRequest absenceRequest)
        {
            var startDate = LocaleHelper.FormatDate(absenceRequest.StartDate.Value);
            var endDate =absenceRequest.EndDate.HasValue ? LocaleHelper.FormatDate(absenceRequest.EndDate.Value) : string.Empty;
            var action = isApproved ? "accepted" : "declined";
            var subject = $"Absence request {action} by {accountAdmin.FullName}";
            var bodyContent = $"Absence request {action} " + (endDate == null ? $"for {startDate}" : $"from {startDate} to {endDate}");
            var linkUrl = GetAbsenceRequestLinkForAccountAdmin(absenceRequest.Id);
            var createdByUser = new NotificationUserDto
            {
                Id = accountAdmin.Id,
                FullName = accountAdmin.FullName,
                Email = accountAdmin.Email,
            };
            var model = new NotificationDto
            {
                Subject = subject,
                Content = bodyContent,
                Link = linkUrl,
                AppplicationUserId = user.Id,
                GroupKey = $"Absence request #{action}",
            };
            await _notificationService.CreateAsync(model);
        }


        public string GetAbsenceRequestLinkForAccountAdmin(Guid schedulerEventId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            return $"{baseUrl}admin/timesheets/absence-request?sidenav=collapsed";
        }
    }
}

