using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
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
        public async Task<ApiResult<ProductResponseDto>> UpSert(ProductRequestDto model)
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

        public async Task<ApiResult<List<ProductResponseDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _productRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));


                var mappedEntities = _mapper.Map<List<ProductResponseDto>>(entities);
                return ApiResult<List<ProductResponseDto>>.Success(mappedEntities);
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

    }
}
