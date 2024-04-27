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
    public class SchedulersService:ISchedulersService
    {
        private readonly IGenericRepository<Schedulers> _schedulersRepository;

        private readonly ILogger<SchedulersService> _logger;
        private readonly IMapper _mapper;
        public SchedulersService(IGenericRepository<Schedulers> schedulersRepository, ILogger<SchedulersService> logger, IMapper mapper)
        {
            _schedulersRepository = schedulersRepository;
          
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<PagedApiResult<SchedulersDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _schedulersRepository.QueryAsync((e => !e.IsDeleted),  include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e=>e.SchedulerStatus).Include(e=>e.SchedulerEventType), orderBy: x => x.OrderBy(x => x.Date));

                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);


                var pagedResult = PageHelper<Schedulers, SchedulersDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new SchedulersDto
                {
                    Id = e.Id,
                    Duration = e.Duration,
                    Date = e.Date,
                    Summary = e.Summary,
                    CreatedAt = e.CreatedAt,
                    CreatedBy = e.CreatedByUser.FullName,
               
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

        public async Task<ApiResult<bool>> Create(SchedulersDto model)
        {
            try
            {
                
                var entities = _mapper.Map<Schedulers>(model);
                await _schedulersRepository.InsertAsync(entities);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<SchedulersDto>> Update(Guid id, SchedulersDto model)
        {
            try
            {
                var entity = await _schedulersRepository.FirstOrDefaultAsync(o => o.Id == id);


                entity.Duration = model.Duration;
                entity.Date = model.Date;
                entity.Summary = model.Summary;
                entity.IsDeleted   = model.IsDeleted;


                await _schedulersRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<SchedulersDto>(entity);
                return ApiResult<SchedulersDto>.Success(mappedEntity);
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






        private IQueryable<Schedulers> ApplyFilters(IQueryable<Schedulers> entities, PageQueryFiterBase filter)
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




        private IQueryable<Schedulers> ApplySorting(IQueryable<Schedulers> orders, SortModel sort)
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
