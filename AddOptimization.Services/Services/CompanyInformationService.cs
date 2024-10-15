using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AddOptimization.Services.Services
{
    public class CompanyInformationService : ICompanyInformationService
    {
        private readonly ILogger<CompanyInformationService> _logger;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IGenericRepository<Group> _groupRepository;
        private readonly ITemplatesService _templateService;
        private readonly ITemplateEntryService _templateEntryService;
        private readonly ICountryService _countryService;
        private readonly IGenericRepository<SharedEntry> _sharedEntryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<TemplateEntries> _templateEntryRepository;

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CompanyInformationService(ILogger<CompanyInformationService> logger, IMapper mapper, IConfiguration configuration, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository, ITemplatesService templateService, ITemplateEntryService templateEntryService, ICountryService countryService, IGenericRepository<SharedEntry> sharedEntryRepository, IHttpContextAccessor httpContextAccessor, IGenericRepository<TemplateEntries> templateEntryRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _applicationUserRepository = applicationUserRepository;
            _groupRepository = groupRepository;
            _templateService = templateService;
            _templateEntryService = templateEntryService;
            _countryService = countryService;
            _sharedEntryRepository = sharedEntryRepository;
            _httpContextAccessor = httpContextAccessor;
            _templateEntryRepository = templateEntryRepository;
        }

        private void Decrypt(TemplateEntryDto model)
        {
            if (model.EntryDataEncrypted != null)
            {
                try
                {
                    string encryptedData = model.EntryDataEncrypted;
                    string decryptedJson = DecryptionHelper.Decrypt(encryptedData);

                    var decryptedEntryData = JsonSerializer.Deserialize<EntryDataDto>(decryptedJson, jsonOptions);

                    model.EntryData = decryptedEntryData ?? new EntryDataDto();

                    if (decryptedEntryData != null)
                    {
                        model.EntryData.CompanyInfo = decryptedEntryData.CompanyInfo;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Decryption failed: {Message}", ex.Message);
                    throw new InvalidOperationException("Decryption failed", ex);
                }
            }
        }
        public async Task<ApiResult<bool>> SaveCompanyInformationDetails(TemplateEntryDto model)
        {
            try
            {
                Decrypt(model);
                await _templateEntryService.Save(model);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model)
        {
            try
            {
                Decrypt(model);
                var mappedEntity = (await _templateEntryService.Update(id, model)).Result;
                return ApiResult<TemplateEntryDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<TemplateEntryDto>> GetCompanyDetailsById(Guid id)
        {
            try
            {
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value.ToString();
                var sharedEntries = (await _sharedEntryRepository.QueryAsync(x => x.EntryId == id)).ToList();
                var templateEntries = (await _templateEntryRepository.QueryAsync(te => te.Id == id)).ToList();
                bool isAnySharedEntryDeleted = sharedEntries.Any(se => se.IsDeleted);
                bool isAnyTemplateEntryDeleted = templateEntries.Any(te => te.IsDeleted);
                bool hasAccessToSharedEntries = sharedEntries.Any(se => se.SharedWithId == currentUserId);
                bool hasAccessToTemplateEntries = templateEntries.Any(te => te.CreatedByUserId.ToString() == currentUserId);
                bool hasAccess = hasAccessToSharedEntries || hasAccessToTemplateEntries;

                if (isAnySharedEntryDeleted || isAnyTemplateEntryDeleted)
                {
                    return ApiResult<TemplateEntryDto>.Failure(ValidationCodes.DataNoLongerExist);
                }
                else if (hasAccess)
                {
                    var mappedEntity = (await _templateEntryService.Get(id)).Result;
                    if (mappedEntity.EntryData != null)
                    {
                        string entryDataJson = JsonSerializer.Serialize(mappedEntity.EntryData, jsonOptions);
                        mappedEntity.EntryDataEncrypted = DecryptionHelper.Encrypt(entryDataJson);
                        mappedEntity.EntryData = null;
                    }
                    return ApiResult<TemplateEntryDto>.Success(mappedEntity);
                }
                else
                {
                    return ApiResult<TemplateEntryDto>.Failure(ValidationCodes.PermissionDenied);
                }
            }

            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<List<TemplateEntryDto>>> Search()
        {
            try
            {
                var templates = (await _templateService.GetAllTemplate()).Result;
                var companyInfoId = templates.FirstOrDefault(x => x.TemplateKey == "company_information".ToString()).Id;
                var mappedEntities = (await _templateEntryService.Search(companyInfoId)).Result;
                foreach (var entity in mappedEntities)
                {
                    if (entity.EntryData != null)
                    {
                        string entryDataJson = JsonSerializer.Serialize(entity.EntryData, jsonOptions);
                        entity.EntryDataEncrypted = DecryptionHelper.Encrypt(entryDataJson);
                        entity.EntryData = null;
                    }
                }
                return ApiResult<List<TemplateEntryDto>>.Success(mappedEntities);
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
                await _templateEntryService.Delete(id);
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