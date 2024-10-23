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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AddOptimization.Services.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ITemplateEntryService _templateEntryService;
        private readonly ILogger<PasswordService> _logger;
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
        public PasswordService(ITemplateEntryService templateEntryService, ILogger<PasswordService> logger, ITemplatesService templateService, IMapper mapper, IGenericRepository<SharedEntry> sharedEntryRepository, IHttpContextAccessor httpContextAccessor, IGenericRepository<TemplateEntries> templateEntryRepository)
        {
            _templateEntryService = templateEntryService;
            _templateService = templateService;
            _logger = logger;
            _mapper = mapper;
            _sharedEntryRepository = sharedEntryRepository;
            _httpContextAccessor = httpContextAccessor;
            _templateEntryRepository = templateEntryRepository;
        }
        private void EncryptPasswordInfo(TemplateEntryDto model)
        {


            if (model.EntryData?.PasswordInfo != null)
            {
                var passwordInfo = model.EntryData.PasswordInfo;
                if (!string.IsNullOrEmpty(passwordInfo.Password))
                {
                    passwordInfo.Password = AesEncryptionDecryptionHelper.Encrypt(passwordInfo.Password);
                    model.EntryData.IsValueEncrypted = true;
                }
            }

        }

        private void DecryptPasswordInfo(TemplateEntryDto entity)
        {
            var passwordInfo = entity.EntryData?.PasswordInfo;
            if (passwordInfo != null && entity.EntryData.IsValueEncrypted == true)
            {

                if (!string.IsNullOrEmpty(passwordInfo.Password))
                {
                    var cipherBytes = Convert.FromBase64String(passwordInfo.Password);
                    passwordInfo.Password = AesEncryptionDecryptionHelper.Decrypt(cipherBytes);
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
                        model.EntryData.PasswordInfo = decryptedEntryData.PasswordInfo;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Decryption failed: {Message}", ex.Message);
                    throw new InvalidOperationException("Decryption failed", ex);
                }
            }
        }
        public async Task<ApiResult<bool>> SavePasswordDetails(TemplateEntryDto model)
        {
            try
            {
                Decrypt(model);
                EncryptPasswordInfo(model);
                return await _templateEntryService.Save(model);
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
                EncryptPasswordInfo(model);
                return await _templateEntryService.Update(id, model);
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
                var passwordId = templates.FirstOrDefault(x => x.TemplateKey == "passwords".ToString()).Id;
                var mappedEntities = (await _templateEntryService.Search(passwordId)).Result;
                foreach (var entity in mappedEntities)
                {
                    DecryptPasswordInfo(entity);
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
        public async Task<ApiResult<TemplateEntryDto>> GetPasswordById(Guid id)
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
                    DecryptPasswordInfo(mappedEntity);
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
