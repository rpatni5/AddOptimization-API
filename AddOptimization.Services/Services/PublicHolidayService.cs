using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AddOptimization.Services.Services
{
    public class PublicHolidayService : IPublicHolidayService
    {
        private readonly IGenericRepository<PublicHoliday> _publicholidayRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly ICustomerEmployeeAssociationService _customerEmployeeAssociationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PublicHolidayService> _logger;
        private readonly IMapper _mapper;

        public PublicHolidayService(IGenericRepository<PublicHoliday> publicholidayRepository, IGenericRepository<Country> countryRepository, ILogger<PublicHolidayService> logger, IMapper mapper, ICustomerEmployeeAssociationService customerEmployeeAssociationService, IHttpContextAccessor httpContextAccessor)
        {
            _publicholidayRepository = publicholidayRepository;
            _countryRepository = countryRepository;
            _logger = logger;
            _mapper = mapper;
            _customerEmployeeAssociationService = customerEmployeeAssociationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PagedApiResult<PublicHolidayResponseDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _publicholidayRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.Country).Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.Date));

                PageQueryFiterBase associationFilter = new PageQueryFiterBase();
                associationFilter.GetValue<int>("employeeId", employeeId =>
                {
                    associationFilter.AddFilter("employeeId", OperatorType.equal.ToString(), employeeId);
                });

                var association = (await _customerEmployeeAssociationService.SearchAllAssociations(associationFilter)).Result.ToList();
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters, association);

                var pagedResult = PageHelper<PublicHoliday, PublicHolidayResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new PublicHolidayResponseDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    CountryId = e.CountryId,
                    CountryName = e.Country.CountryName ?? string.Empty,
                    Date = e.Date,


                }).ToList());


                var result = pagedResult;
                return PagedApiResult<PublicHolidayResponseDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<PublicHolidayResponseDto>> Create(PublicHolidayRequestDto model)
        {
            try
            {
                var isExisting = await _publicholidayRepository.IsExist(s => s.Title.ToLower() == model.Title.ToLower() && s.CountryId == model.CountryId);
                if (isExisting)
                {
                    return ApiResult<PublicHolidayResponseDto>.Failure(ValidationCodes.FieldNameAlreadyExists);
                }
                var entity = _mapper.Map<PublicHoliday>(model);
                entity.Date = entity.Date.Date;
                await _publicholidayRepository.InsertAsync(entity);
                var mappedEntity = _mapper.Map<PublicHolidayResponseDto>(entity);
                return ApiResult<PublicHolidayResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<PublicHolidayResponseDto>> Get(Guid id)
        {
            try
            {
                var entity = await _publicholidayRepository.FirstOrDefaultAsync(o => o.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<PublicHolidayResponseDto>.NotFound("Public Holiday");
                }
                var mappedEntity = _mapper.Map<PublicHolidayResponseDto>(entity);
                return ApiResult<PublicHolidayResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<PublicHolidayResponseDto>> Update(Guid id, PublicHolidayRequestDto model)
        {
            try
            {
                var entity = await _publicholidayRepository.FirstOrDefaultAsync(o => o.Id == id);
                _mapper.Map(model, entity);
                await _publicholidayRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<PublicHolidayResponseDto>(entity);
                return ApiResult<PublicHolidayResponseDto>.Success(mappedEntity);
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
                var entity = await _publicholidayRepository.FirstOrDefaultAsync(t => t.Id == id);
                entity.IsDeleted = true;
                await _publicholidayRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private IQueryable<PublicHoliday> ApplyFilters(IQueryable<PublicHoliday> entities, PageQueryFiterBase filter, List<CustomerEmployeeAssociationDto> association)
        {
            filter.GetValue<string>("countryId", (v) =>
            {
                entities = entities.Where(e => e.CountryId.ToString() == v);
            });
            filter.GetValue<int>("employeeId", employeeId =>
            {
                var associatedCountries = association
                    .Where(a => a.EmployeeId == employeeId)
                    .Select(a => a.PublicHolidayCountryId)
                    .Distinct()
                    .ToList();

                entities = entities.Where(e => associatedCountries.Contains(e.CountryId));
            });

            filter.GetValue<string>("countryName", (v) =>
            {
                entities = entities.Where(e => e.Country != null && (e.Country.CountryName.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("title", (v) =>
            {
                entities = entities.Where(e => e.Title != null && (e.Title.ToLower().Contains(v.ToLower())));
            });

            filter.GetValue<DateTime>("date", (v) =>
            {
                entities = entities.Where(e => e.Date != null && e.Date < v);
            }, OperatorType.lessthan, true);

            filter.GetValue<DateTime>("date", (v) =>
            {
                entities = entities.Where(e => e.Date != null && e.Date > v);
            }, OperatorType.greaterthan, true);

            return entities;
        }


        private IQueryable<PublicHoliday> ApplySorting(IQueryable<PublicHoliday> entities, SortModel sort)
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
                    if (columnName.ToUpper() == nameof(PublicHolidayResponseDto.CountryName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Country.CountryName);
                    }
                    if (columnName.ToUpper() == nameof(PublicHolidayResponseDto.Title).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Title); ;
                    }
                    if (columnName.ToUpper() == nameof(PublicHolidayResponseDto.Date).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Date);
                    }

                }

                else
                {
                    if (columnName.ToUpper() == nameof(PublicHolidayResponseDto.CountryName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Country.CountryName);
                    }
                    if (columnName.ToUpper() == nameof(PublicHolidayResponseDto.Title).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Title); ;
                    }
                    if (columnName.ToUpper() == nameof(PublicHolidayResponseDto.Date).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Date);
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
