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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AddOptimization.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly ILogger<ProductService> _logger;
        private readonly IMapper _mapper;

        public ProductService(IGenericRepository<Product> productRepository, ILogger<ProductService> logger, IMapper mapper)
        {
            _productRepository = productRepository;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<ApiResult<ProductResponseDto>> Upsert(ProductRequestDto model)
        {
            try
            {
                var isExisting = await _productRepository.IsExist(s => s.Name.ToLower() == model.Name.ToLower() && !s.IsDeleted && s.Id != model.Id);
                if (isExisting)
                {
                    return ApiResult<ProductResponseDto>.Failure(ValidationCodes.FieldNameAlreadyExists);
                }

                Product entity;
                if (model.Id != Guid.Empty)
                {
                    entity = await _productRepository.FirstOrDefaultAsync(o => o.Id == model.Id);
                    _mapper.Map(model, entity);
                    await _productRepository.UpdateAsync(entity);
                }
                else
                {
                    entity = _mapper.Map<Product>(model);
                    await _productRepository.InsertAsync(entity);
                }
                var mappedEntity = _mapper.Map<ProductResponseDto>(entity);
                return ApiResult<ProductResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<PagedApiResult<ProductResponseDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _productRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);
                var pagedResult = PageHelper<Product, ProductResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new ProductResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    SalesPrice = e.SalesPrice,
                    PurchasePrice = e.PurchasePrice,
                    Quantity = e.Quantity,
                    ProfitMargin = e.ProfitMargin,
                    CreatedAt   = e.CreatedAt,
                    CreatedBy = e.CreatedByUser.FullName

                }).ToList());

                var result = pagedResult;
                return PagedApiResult<ProductResponseDto>.Success(result);
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
                var entity = await _productRepository.FirstOrDefaultAsync(t => t.Id == id);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Product");
                }
                entity.IsDeleted = true;
                entity.IsActive = false;
                await _productRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private IQueryable<Product> ApplyFilters(IQueryable<Product> entities, PageQueryFiterBase filter)
        {

            filter.GetValue<string>("name", (v) =>
            {
                entities = entities.Where(e => e.Name != null && (e.Name.ToLower().Contains(v.ToLower())));
            });
            filter.GetValue<string>("description", (v) =>
            {
                entities = entities.Where(e => e.Description == v);
            });
            filter.GetValue<string>("createdBy", (v) =>
            {
                entities = entities.Where(e => e.CreatedByUser.FullName == v);
            });
            filter.GetValue<string>("salesPrice", (v) =>
            {
                int salesPrice = Convert.ToInt32(v);
                entities = entities.Where(e => e.SalesPrice == salesPrice);
            });
           
            filter.GetValue<string>("purchasePrice", (v) =>
            {
                int purchasePrice = Convert.ToInt32(v);
                entities = entities.Where(e => e.PurchasePrice == purchasePrice);
            });
            filter.GetValue<string>("quantity", (v) =>
            {
                int quantity = Convert.ToInt32(v);
                entities = entities.Where(e => e.Quantity == quantity);
            });
            filter.GetValue<string>("profitMargin", (v) =>
            {
                int profitMargin = Convert.ToInt32(v);
                entities = entities.Where(e => e.ProfitMargin == profitMargin);
            });
            filter.GetValue<DateTime>("createdAt", (v) =>
            {
                entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt < v);
            }, OperatorType.lessthan, true);

            filter.GetValue<DateTime>("createdAt", (v) =>
            {
                entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt > v);
            }, OperatorType.greaterthan, true);


            return entities;
        }


        private IQueryable<Product> ApplySorting(IQueryable<Product> entities, SortModel sort)
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
                    if (columnName.ToUpper() == nameof(ProductResponseDto.Name).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Name);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.Description).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Description); ;
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.SalesPrice).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.SalesPrice);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.PurchasePrice).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.PurchasePrice);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.Quantity).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Quantity);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.ProfitMargin).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.ProfitMargin);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.CreatedAt).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.CreatedAt);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.CreatedBy).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.CreatedByUser.FullName);
                    }

                }

                else
                {
                    if (columnName.ToUpper() == nameof(ProductResponseDto.Name).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Name);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.Description).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Description); ;
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.SalesPrice).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.SalesPrice);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.PurchasePrice).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.PurchasePrice);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.Quantity).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Quantity);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.ProfitMargin).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.ProfitMargin);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.CreatedAt).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.CreatedAt);
                    }
                    if (columnName.ToUpper() == nameof(ProductResponseDto.CreatedBy).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.CreatedByUser.FullName);
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
