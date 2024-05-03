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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AddOptimization.Services.Services
{
    public class SchedulerEventService : ISchedulerEventService
    {
        private readonly IGenericRepository<SchedulerEvent> _schedulersRepository;

        private readonly ILogger<SchedulerEventService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public SchedulerEventService(IGenericRepository<SchedulerEvent> schedulersRepository, ILogger<SchedulerEventService> logger, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _schedulersRepository = schedulersRepository;

            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;

        }
        public async Task<PagedApiResult<SchedulersDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _schedulersRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.SchedulerStatus).Include(e => e.SchedulerEventType).Include(e => e.ApplicationUser).Include(e => e.Client), orderBy: x => x.OrderBy(x => x.Date));

                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);


                var pagedResult = PageHelper<SchedulerEvent, SchedulersDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new SchedulersDto
                {
                    Id = e.Id,
                    Duration = e.Duration,
                    Date = e.Date,
                    Summary = e.Summary,
                    CreatedAt = e.CreatedAt,
                    CreatedBy = e.CreatedByUser.FullName,
                    StatusID = e.SchedulerStatus.Id,
                    EventTypeID = e.SchedulerEventType.Id,
                    UserID = e.ApplicationUser.Id,
                    ClientID = e.Client.Id,

                }).ToList());
                var retVal = pagedResult;
                return PagedApiResult<SchedulersDto>.Success(retVal);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<bool>> Save(List<SchedulersDto> model)
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


        private IQueryable<SchedulerEvent> ApplyFilters(IQueryable<SchedulerEvent> entities, PageQueryFiterBase filter)
        {
            filter.GetValue<string>("Date", (v) =>
            {
                if (!string.IsNullOrEmpty(v))
                {
                    var createdDate = DateTime.Parse(v).Date;
                    entities = entities.Where(e => e.Date != null && e.Date.Value.Date == createdDate);
                }
            });

            filter.GetValue<string>("Duration", (v) =>
            {
                if (!string.IsNullOrEmpty(v))
                {
                    entities = entities.Where(e => e.Duration == Convert.ToInt32(v));
                }
            });

            filter.GetValue<string>("Summary", (v) =>
            {
                entities = entities.Where(e => e.Summary != null && e.Summary.ToLower().Contains(v.ToLower()));
            });


            return entities;
        }


        private IQueryable<SchedulerEvent> ApplySorting(IQueryable<SchedulerEvent> orders, SortModel sort)
        {
            try
            {
                if (sort?.Name == null)
                {
                    orders = orders.OrderByDescending(o => o.CreatedAt);
                    return orders;
                }
                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {
                    if (columnName.ToUpper() == nameof(SchedulersDto.Duration).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Duration);
                    }
                    if (columnName.ToUpper() == nameof(SchedulersDto.Date).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Date);
                    }
                    if (columnName.ToUpper() == nameof(SchedulersDto.Summary).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Summary);
                    }

                }
                else
                {
                    if (columnName.ToUpper() == nameof(SchedulersDto.Duration).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.Duration);
                    }
                    if (columnName.ToUpper() == nameof(SchedulersDto.Date).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.Date);
                    }
                    if (columnName.ToUpper() == nameof(SchedulersDto.Summary).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.Summary);
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
