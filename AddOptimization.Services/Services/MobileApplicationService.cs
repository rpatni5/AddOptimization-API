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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AddOptimization.Services.Services
{
    

    public class MobileApplicationService : IMobileApplicationService
    {
        private readonly ITemplateEntryService _templateEntryService;
        private readonly ILogger<MobileApplicationService> _logger;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly ITemplatesService _templateService;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Group> _groupRepository;
        private readonly IGenericRepository<SharedEntry> _sharedEntryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<TemplateEntries> _templateEntryRepository;

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        public MobileApplicationService(ITemplateEntryService templateEntryService, ILogger<MobileApplicationService> logger, ITemplatesService templateService, IMapper mapper, IGenericRepository<SharedEntry> sharedEntryRepository, IHttpContextAccessor httpContextAccessor, IGenericRepository<TemplateEntries> templateEntryRepository)
        {
            _templateEntryService = templateEntryService;
            _templateService = templateService;
            _logger = logger;
            _mapper = mapper;
            _sharedEntryRepository = sharedEntryRepository;
            _httpContextAccessor = httpContextAccessor;
            _templateEntryRepository = templateEntryRepository;
        }
        private void EncryptMobileAppInfo(TemplateEntryDto model)
        {


            if (model.EntryData?.MobileAppInfo != null)
            {
                var MobileApplicationInfo = model.EntryData.MobileAppInfo;
                if (!string.IsNullOrEmpty(MobileApplicationInfo.Password))
                {
                    MobileApplicationInfo.Password = AesEncryptionDecryptionHelper.Encrypt(MobileApplicationInfo.Password);
                    model.EntryData.IsValueEncrypted = true;
                }
            }

        }

        private void DecryptMobileAppInfo(TemplateEntryDto entity)
        {
            var MobileApplicationInfo = entity.EntryData?.MobileAppInfo;
            if (MobileApplicationInfo != null && entity.EntryData.IsValueEncrypted == true)
            {

                if (!string.IsNullOrEmpty(MobileApplicationInfo.Password))
                {
                    var cipherBytes = Convert.FromBase64String(MobileApplicationInfo.Password);
                    MobileApplicationInfo.Password = AesEncryptionDecryptionHelper.Decrypt(cipherBytes);
                }
            }
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
                        model.EntryData.MobileAppInfo = decryptedEntryData.MobileAppInfo;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Decryption failed: {Message}", ex.Message);
                    throw new InvalidOperationException("Decryption failed", ex);
                }
            }
        }
        public async Task<ApiResult<bool>> SaveMobileAppDetails(TemplateEntryDto model)
        {
            try
            {
                Decrypt(model);
                EncryptMobileAppInfo(model);
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
                EncryptMobileAppInfo(model);
                var mappedEntity = (await _templateEntryService.Update(id, model)).Result;
                return ApiResult<TemplateEntryDto>.Success(mappedEntity);
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
                var mobileAppId = templates.FirstOrDefault(x => x.TemplateKey == "mobile_application".ToString()).Id;
                var mappedEntities = (await _templateEntryService.Search(mobileAppId)).Result;
                foreach (var entity in mappedEntities)
                {
                    DecryptMobileAppInfo(entity);
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
        public async Task<ApiResult<TemplateEntryDto>> GetById(Guid id)
        {
            try
            {
                    var mappedEntity = (await _templateEntryService.Get(id)).Result;
                if (mappedEntity == null)
                {
                    return ApiResult<TemplateEntryDto>.Failure(ValidationCodes.PermissionDenied);
                }

                else
                {
                    DecryptMobileAppInfo(mappedEntity);
                    if (mappedEntity.EntryData != null)
                    {
                        string entryDataJson = JsonSerializer.Serialize(mappedEntity.EntryData, jsonOptions);
                        mappedEntity.EntryDataEncrypted = DecryptionHelper.Encrypt(entryDataJson);
                        mappedEntity.EntryData = null;
                    }
                    return ApiResult<TemplateEntryDto>.Success(mappedEntity);
                }
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
