

using AutoMapper;
using Microsoft.Extensions.Logging;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Services.Services;

public class FieldService : IFieldService
{
    private readonly ILogger<FieldService> _logger;
    private readonly IGenericRepository<Field> _fieldRepository;
    private readonly IMapper _mapper;

    public FieldService(ILogger<FieldService> logger, IGenericRepository<Field> fieldRepository, IMapper mapper)
    {
        _logger = logger;
        _fieldRepository = fieldRepository;
        _mapper = mapper;
    }

    public async Task<ApiResult<List<FieldDto>>> Search(PageQueryFiterBase filter)
    {
        try
        {
            filter ??= new PageQueryFiterBase();
            var entities = await _fieldRepository.QueryAsync(orderBy: (entities) => entities.OrderBy(f => f.Name));
            filter.GetValue<Guid>("screenId", (v) =>
            {
                entities = entities.Where(e => e.ScreenId == v);
            });
            var mappedEntities=_mapper.Map<List<FieldDto>>(entities.ToList());
            return ApiResult<List<FieldDto>>.Success(mappedEntities);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<bool>> Create(FieldCreateDto model)
    {
        try
        {
            var isExisting = await _fieldRepository.IsExist(s => s.Name.ToLower() == model.Name.ToLower() && s.ScreenId == model.ScreenId);
            if (isExisting)
            {
                return ApiResult<bool>.Failure(ValidationCodes.FieldNameAlreadyExists);
            }
            var entity = _mapper.Map<Field>(model);
            await _fieldRepository.InsertAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}
