using AddOptimization.Contracts.Dto;
using Microsoft.Extensions.Logging;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AddOptimization.Utilities.Extensions;

namespace AddOptimization.Services.Services
{
    public class SavedSearchService : ISavedSearchService
    {
        private readonly IGenericRepository<SavedSearch> _savedSearchRepository;
        private readonly ILogger<SavedSearchService> _logger;
        private readonly IMapper _mapper;

        public SavedSearchService(IGenericRepository<SavedSearch> savedSearchRepository, ILogger<SavedSearchService> logger, IMapper mapper)
        {
            _savedSearchRepository = savedSearchRepository;
            _logger = logger;
            _mapper = mapper;
        }

       
        public async Task<ApiResult<SavedSearchDto>> Create(SavedSearchDto model)
        {
            try
            {
                var currentUserId = _savedSearchRepository.CurrentUserId;

                var isExistingSearch = await _savedSearchRepository.IsExist(s =>
                    s.Name.ToLower() == model.Name.ToLower() &&
                    s.SearchScreen.ToLower() == model.SearchScreen.ToLower() &&
                    s.CreatedByUserId == currentUserId);
                if (isExistingSearch)
                {
                    return ApiResult<SavedSearchDto>.Failure(ValidationCodes.SearchWithNameAlreadyExists);
                }

                if (model.IsDefault ?? false)
                {
                    var existingDefaultSearches = (await _savedSearchRepository.QueryAsync(e =>
                        e.SearchScreen.ToLower() == model.SearchScreen.ToLower() &&
                        e.IsDefault == true &&
                        e.CreatedByUserId == currentUserId)).ToList();

                    if (existingDefaultSearches.Any())
                    {
                        existingDefaultSearches.ForEach(e =>
                        {
                            e.IsDefault = false;
                        });
                        await _savedSearchRepository.BulkUpdateAsync(existingDefaultSearches);
                    }
                }

                var savedSearch = _mapper.Map<SavedSearch>(model);
                savedSearch.CreatedByUserId = currentUserId; 
                await _savedSearchRepository.InsertAsync(savedSearch);
                model.Id = savedSearch.Id;

                return ApiResult<SavedSearchDto>.Success(model);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
      
        public async Task<ApiResult<SavedSearchDto>> Update(Guid id, SavedSearchDto model)
        {
            try
            {
                var currentUserId = _savedSearchRepository.CurrentUserId;

                var isExistingSearch = await _savedSearchRepository.IsExist(s =>
                    s.Id != id &&
                    s.SearchScreen.ToLower() == model.SearchScreen.ToLower() &&
                    s.Name.ToLower() == model.Name.ToLower() &&
                    s.CreatedByUserId == currentUserId);
                if (isExistingSearch)
                {
                    return ApiResult<SavedSearchDto>.Failure(ValidationCodes.SearchWithNameAlreadyExists);
                }

                var savedSearch = await _savedSearchRepository.FirstOrDefaultAsync(s => s.Id == id && s.CreatedByUserId == currentUserId);
                if (savedSearch == null)
                {
                    return ApiResult<SavedSearchDto>.NotFound();
                }

                _mapper.Map(model, savedSearch);
                await _savedSearchRepository.UpdateAsync(savedSearch);

                if (model.IsDefault ?? false)
                {
                    var existingDefaultSearches = (await _savedSearchRepository.QueryAsync(e =>
                        e.SearchScreen.ToLower() == model.SearchScreen.ToLower() &&
                        e.Id != id &&
                        e.IsDefault == true &&
                        e.CreatedByUserId == currentUserId)).ToList();

                    if (existingDefaultSearches.Any())
                    {
                        existingDefaultSearches.ForEach(e =>
                        {
                            e.IsDefault = false;
                        });
                        await _savedSearchRepository.BulkUpdateAsync(existingDefaultSearches);
                    }
                }

                return ApiResult<SavedSearchDto>.Success(model);
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
                var savedSearch = await _savedSearchRepository.SingleOrDefaultAsync(s => s.Id == id);
                if (savedSearch == null)
                {
                    return ApiResult<bool>.NotFound();
                }
                if (savedSearch.CreatedByUserId != _savedSearchRepository.CurrentUserId)
                {
                    return ApiResult<bool>.Failure(ValidationCodes.OtherUserSearchDelete);
                }
                await _savedSearchRepository.DeleteAsync(savedSearch);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<SavedSearchDto>>> GetCurrentUserSearches(string searchScreen)
        {
            try
            {
                var currentUserId = _savedSearchRepository.CurrentUserId;
                var entities = await _savedSearchRepository.QueryAsync(s => (s.SearchScreen.ToLower() == searchScreen.ToLower()) && (s.CreatedByUserId == currentUserId), orderBy: entities => entities.OrderBy(e => e.Name));
                var mappedEntities = _mapper.Map<List<SavedSearchDto>>(entities.ToList());
                return ApiResult<List<SavedSearchDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}
