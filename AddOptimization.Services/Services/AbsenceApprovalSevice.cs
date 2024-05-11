using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
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

namespace AddOptimization.Services.Services
{
    public class AbsenceApprovalSevice : IAbsenceApprovalService
    {
        private readonly IGenericRepository<AbsenceRequest> _absenceApprovalRepository;
        private readonly ILogger<AbsenceApprovalSevice> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILeaveStatusesService _leaveStatusesService;
        private readonly IGenericRepository<LeaveStatuses> _leavestatusRepository;



        public AbsenceApprovalSevice(IGenericRepository<AbsenceRequest> absenceApprovalRepository, IGenericRepository<LeaveStatuses> leavestatusRepository, ILogger<AbsenceApprovalSevice> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILeaveStatusesService leaveStatusesService)
        {
            _absenceApprovalRepository = absenceApprovalRepository;
            _leavestatusRepository = leavestatusRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _leaveStatusesService = leaveStatusesService;
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
                    Date = e.Date,
                    UserId = e.UserId,
                    LeaveStatusName = e.LeaveStatuses.Name,
                    UpdatedBy = e.CreatedByUser.FullName,
                    Duration = e.Duration,
                    UserName = e.ApplicationUser.FullName,

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
                    var approvedStatusId = (await _leavestatusRepository.FirstOrDefaultAsync(x => x.Name.ToLower() == "approved")).Id;

                    entity.LeaveStatusId = approvedStatusId;
                }
                else
                {
                    var rejectedStatusId = (await _leavestatusRepository.FirstOrDefaultAsync(x => x.Name.ToLower() == "rejected")).Id;

                    entity.LeaveStatusId = rejectedStatusId;
                }
                entity.Comment = model.Comment;

                await _absenceApprovalRepository.UpdateAsync(entity);
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
                var date = new DateTime(v.Max().Year, v.Max().Month, 1);
                entities = entities.Where(e => e.Date < date);
            }, OperatorType.lessthan, true);

            filter.GetList<DateTime>("duedateRange", (v) =>
            {
                var date = (new DateTime(v.Min().Year, v.Min().Month, 1)).AddMonths(1).AddDays(-1);
                entities = entities.Where(e => e.Date > date);
            }, OperatorType.greaterthan, true);

            filter.GetValue<DateTime>("Date", (v) =>
            {
                entities = entities.Where(e => e.Date != null && e.Date < v);
            }, OperatorType.lessthan, true);
            filter.GetValue<DateTime>("Date", (v) =>
            {
                entities = entities.Where(e => e.Date != null && e.Date > v);
            }, OperatorType.greaterthan, true);
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

