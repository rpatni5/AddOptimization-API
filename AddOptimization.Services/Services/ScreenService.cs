using AutoMapper;
using Microsoft.Extensions.Logging;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;

namespace AddOptimization.Services.Services;

public class ScreenService : IScreenService
{
    private readonly ILogger<ScreenService> _logger;
    private readonly IGenericRepository<Screen> _screenRepository;
    private readonly IMapper _mapper;

    public ScreenService(ILogger<ScreenService> logger, IGenericRepository<Screen> screenRepository, IMapper mapper)
    {
        _logger = logger;
        _screenRepository = screenRepository;
        _mapper = mapper;
    }

    public async Task<ApiResult<List<ScreenDto>>> Search()
    {
        try
        {
            var entities = await _screenRepository.QueryAsync(orderBy: (entities) => entities.OrderBy(f => f.Name));
            var mappedEntities = _mapper.Map<List<ScreenDto>>(entities.ToList());
            return ApiResult<List<ScreenDto>>.Success(mappedEntities);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
    public async Task<ApiResult<ScreenDto>> Create(ScreenCreateDto model)
    {
        try
        {
            var isExisting=await _screenRepository.IsExist(s=> s.Name.ToLower()==model.Name.ToLower());
            if(isExisting)
            {
                return ApiResult<ScreenDto>.Failure(ValidationCodes.ScreenWithNameAlreadyExists);
            }
            var entity = _mapper.Map<Screen>(model);
            entity=await _screenRepository.InsertAsync(entity);
            var mappedEtntiy=_mapper.Map<ScreenDto>(entity);
            return ApiResult<ScreenDto>.Success(mappedEtntiy);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<ScreenDto>> Update(Guid id, ScreenCreateDto model)
    {
        try
        {
            var entity = await _screenRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                return ApiResult<ScreenDto>.NotFound("Screen");
            }
            var isExisting = await _screenRepository.IsExist(s =>s.Id != id && s.Name.ToLower() == model.Name.ToLower());
            if (isExisting)
            {
                return ApiResult<ScreenDto>.Failure(ValidationCodes.ScreenWithNameAlreadyExists);
            }
            entity.Route=model.Route;
            entity.Name=model.Name;
            entity = await _screenRepository.UpdateAsync(entity);
            var mappedEtntiy = _mapper.Map<ScreenDto>(entity);
            return ApiResult<ScreenDto>.Success(mappedEtntiy);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}
