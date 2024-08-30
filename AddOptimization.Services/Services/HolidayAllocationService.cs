using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Utilities.Constants;
using AddOptimization.Services.Constants;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using AddOptimization.Utilities.Enums;

namespace AddOptimization.Services.Services
{
    public class HolidayAllocationService : IHolidayAllocationService
    {
        private readonly IGenericRepository<HolidayAllocation> _holidayAllocationRepository;
        private readonly ILogger<HolidayAllocationService> _logger;
        private readonly IMapper _mapper;
        private readonly List<string> _currentUserRoles;
        private readonly IAbsenceApprovalService _absenceApprovalService;
        

        public HolidayAllocationService(IGenericRepository<HolidayAllocation> holidayAllocationRepository, ILogger<HolidayAllocationService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IAbsenceApprovalService absenceApprovalService)
        {
            _holidayAllocationRepository = holidayAllocationRepository;
            _logger = logger;
            _mapper = mapper;
            _currentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
            _absenceApprovalService = absenceApprovalService;
        }


        public async Task<PagedApiResult<HolidayAllocationResponseDto>> Search(PageQueryFiterBase filter)
        {
            try
            {
                var entities = await _holidayAllocationRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));

                entities = ApplySorting(entities, filter?.Sorted?.FirstOrDefault());

                entities = ApplyFilters(entities, filter);


                var pagedResult = PageHelper<HolidayAllocation, HolidayAllocationResponseDto>.ApplyPaging(entities, filter, entities => entities.Select(e => new HolidayAllocationResponseDto
                {
                    Id = e.Id,
                    UserName = e.ApplicationUser.FullName,
                    CreatedBy = e.CreatedByUser.FullName,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    Holidays = e.Holidays,
                    UserId = e.UserId,
                }).ToList());

                return PagedApiResult<HolidayAllocationResponseDto>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<HolidayAllocationResponseDto>> Create(HolidayAllocationRequestDto model)
        {
            try
            {
                var isExisting = await _holidayAllocationRepository.IsExist(s => s.UserId == model.UserId && !s.IsDeleted && s.Id != model.Id);
                if (isExisting)
                {
                    return ApiResult<HolidayAllocationResponseDto>.Failure(ValidationCodes.FieldNameAlreadyExists);
                }
                HolidayAllocation entity;
                if (model.Id != Guid.Empty)
                {
                    entity = await _holidayAllocationRepository.FirstOrDefaultAsync(o => o.Id == model.Id);
                    if (entity == null)
                    {
                        return ApiResult<HolidayAllocationResponseDto>.Failure(ValidationCodes.NotFound);
                    }
                    var createdAt = entity.CreatedAt;
                    _mapper.Map(model, entity);
                    entity.CreatedAt = createdAt;
                    await _holidayAllocationRepository.UpdateAsync(entity);
                }
                else
                {
                    entity = _mapper.Map<HolidayAllocation>(model);
                    await _holidayAllocationRepository.InsertAsync(entity);
                }
                var mappedEntity = _mapper.Map<HolidayAllocationResponseDto>(entity);
                return ApiResult<HolidayAllocationResponseDto>.Success(mappedEntity);
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
                var entity = await _holidayAllocationRepository.FirstOrDefaultAsync(t => t.Id == id);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Holiday Allocation");
                }
                entity.IsDeleted = true;
                await _holidayAllocationRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<HolidayAllocationResponseDto>> GetAllocatedHolidays(int employeeId)
        {
            try
            {
                var associations = await _holidayAllocationRepository.FirstOrDefaultAsync(e => e.UserId == employeeId && !e.IsDeleted, include: entities => entities.Include(e => e.ApplicationUser));
                var mappedEntity = _mapper.Map<HolidayAllocationResponseDto>(associations);
                return ApiResult<HolidayAllocationResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<LeaveBalanceDto>> GetEmployeeLeaveBalance(int employeeId)
        {
            try
            {
              var totalHolidayAllocated = (await GetAllocatedHolidays(employeeId)).Result?.Holidays ?? 0 ;
                var leaveTaken = (await _absenceApprovalService.GetAllAbsenseApproval(employeeId)).Result?.Count ?? 0;
                var remainingLeaves = totalHolidayAllocated - leaveTaken;

                var leaveBalanceDto = new LeaveBalanceDto
                {
                    EmployeeId = employeeId,
                    TotalAllocatedHoliday = totalHolidayAllocated,
                    LeaveTaken = leaveTaken,
                    leavesLeft = remainingLeaves
                };

                return ApiResult<LeaveBalanceDto>.Success(leaveBalanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return ApiResult<LeaveBalanceDto>.Failure("An error occurred while fetching  balance.");
            }
        }


        private IQueryable<HolidayAllocation> ApplyFilters(IQueryable<HolidayAllocation> entities, PageQueryFiterBase filter)
        {
            filter.GetValue<string>("holidays", (v) =>
            {
                entities = entities.Where(e => e.Holidays == Convert.ToInt32(v));
            });

            filter.GetValue<string>("userName", (v) =>
            {
                entities = entities.Where(e => e.ApplicationUser.FullName.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<string>("createdBy", (v) =>
            {
                entities = entities.Where(e => e.CreatedByUser.FullName.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<DateTime>("createdAt", (v) =>
            {
                entities = entities.Where(e => e.CreatedAt < v);
            }, OperatorType.lessthan, true);

            filter.GetValue<DateTime>("createdAt", (v) =>
            {
                entities = entities.Where(e => e.CreatedAt > v);
            }, OperatorType.greaterthan, true);

            filter.GetValue<DateTime>("updatedAt", (v) =>
            {
                entities = entities.Where(e => e.UpdatedAt < v);
            }, OperatorType.lessthan, true);

            filter.GetValue<DateTime>("updatedAt", (v) =>
            {
                entities = entities.Where(e => e.UpdatedAt > v);
            }, OperatorType.greaterthan, true);

            return entities;
        }
        private IQueryable<HolidayAllocation> ApplySorting(IQueryable<HolidayAllocation> entities, SortModel sort)
        {
            try
            {
                if (sort?.Name == null)
                {
                    entities = entities.OrderByDescending(o => o.CreatedAt);
                    return entities;
                }

                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {
                    if (columnName == nameof(HolidayAllocationResponseDto.UserName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.ApplicationUser.FullName);
                    }
                    if (columnName == nameof(HolidayAllocationResponseDto.Holidays).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Holidays);
                    }
                    if (columnName == nameof(HolidayAllocationResponseDto.CreatedBy).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.CreatedByUser.FullName);
                    }
                    if (columnName == nameof(HolidayAllocationResponseDto.CreatedAt).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.CreatedAt);
                    }
                }
                else
                {
                    if (columnName == nameof(HolidayAllocationResponseDto.UserName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.ApplicationUser.FullName);
                    }
                    if (columnName == nameof(HolidayAllocationResponseDto.Holidays).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Holidays);
                    }
                    if (columnName == nameof(HolidayAllocationResponseDto.CreatedBy).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.CreatedByUser.FullName);
                    }
                    if (columnName == nameof(HolidayAllocationResponseDto.CreatedAt).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.CreatedAt);
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

    }

}
