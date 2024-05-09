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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AddOptimization.Services.Services
{
    public class AbsenceApprovalSevice:IAbsenceApprovalService
    {
        private readonly IGenericRepository<AbsenceRequest> _absenceApprovalRepository;
        private readonly ILogger<AbsenceApprovalSevice> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILeaveStatusesService _leaveStatusesService;


        public AbsenceApprovalSevice(IGenericRepository<AbsenceRequest> absenceApprovalRepository, ILogger<AbsenceApprovalSevice> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILeaveStatusesService leaveStatusesService)
        {
            _absenceApprovalRepository = absenceApprovalRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _leaveStatusesService = leaveStatusesService;
        }

      


        public async Task<PagedApiResult<AbsenceRequestResponseDto>> Search(PageQueryFiterBase filters)
        {
            try
            {

                var entities = await _absenceApprovalRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.LeaveStatuses).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser));
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);

                var pagedResult = PageHelper<AbsenceRequest, AbsenceRequestResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new AbsenceRequestResponseDto
                {
                    Id = e.Id,
                    Comment = e.Comment,
                    Date=e.Date,
                    UserId = e.UserId,
                    LeaveStatusName=e.LeaveStatuses.Name,
                    UpdatedBy=e.CreatedByUser.FullName,
                    Duration=e.Duration
                  
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
                    entities = entities.Where(e => e.Duration == Convert.ToInt32(v));
                }
            });
            filter.GetValue<string>("LeaveStatuses.Name", (v) =>
            {
                entities = entities.Where(e => e.LeaveStatuses.Name != null && e.LeaveStatuses.Name.ToLower().Contains(v.ToLower()));
            });

            filter.GetValue<string>("CreatedByUser.FullName", (v) =>
            {
                entities = entities.Where(e => e.CreatedByUser.FullName != null && e.CreatedByUser.FullName.ToLower().Contains(v.ToLower()));
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
