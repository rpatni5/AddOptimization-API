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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;


namespace AddOptimization.Services.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ILogger<CreditCardService> _logger;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IGenericRepository<Group> _groupRepository;
        private readonly ITemplatesService _templateService;
        private readonly ITemplateEntryService _templateEntryService;
        private readonly IGenericRepository<SharedEntry> _sharedEntryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<TemplateEntries> _templateEntryRepository;
        private readonly IGenericRepository<SharedFolder> _sharedFolderRepository;


        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CreditCardService(ILogger<CreditCardService> logger, IMapper mapper, IConfiguration configuration, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository, ITemplatesService templateService, ITemplateEntryService templateEntryService, IGenericRepository<SharedEntry> sharedEntryRepository, IHttpContextAccessor httpContextAccessor, IGenericRepository<TemplateEntries> templateEntryRepository, IGenericRepository<SharedFolder> sharedFolderRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _applicationUserRepository = applicationUserRepository;
            _groupRepository = groupRepository;
            _templateService = templateService;
            _templateEntryService = templateEntryService;
            _sharedEntryRepository = sharedEntryRepository;
            _httpContextAccessor = httpContextAccessor;
            _templateEntryRepository = templateEntryRepository;
            _sharedFolderRepository = sharedFolderRepository;
        }


        private void EncryptCreditCardInfo(TemplateEntryDto model)
        {


            if (model.EntryData?.CreditCardInfo != null)
            {
                var creditCardInfo = model.EntryData.CreditCardInfo;
                if (!string.IsNullOrEmpty(creditCardInfo.Cvv))
                {
                    creditCardInfo.Cvv = AesEncryptionDecryptionHelper.Encrypt(creditCardInfo.Cvv);
                    model.EntryData.IsValueEncrypted = true;
                }

                if (!string.IsNullOrEmpty(creditCardInfo.CardPin))
                {
                    creditCardInfo.CardPin = AesEncryptionDecryptionHelper.Encrypt(creditCardInfo.CardPin);
                    model.EntryData.IsValueEncrypted = true;
                }
            }

        }

        private void DecryptCreditCardInfo(TemplateEntryDto entity)
        {
            var creditCardInfo = entity.EntryData?.CreditCardInfo;
            if (creditCardInfo != null && entity.EntryData.IsValueEncrypted == true)
            {

                if (!string.IsNullOrEmpty(creditCardInfo.Cvv))
                {
                    var cipherBytes = Convert.FromBase64String(creditCardInfo.Cvv);
                    creditCardInfo.Cvv = AesEncryptionDecryptionHelper.Decrypt(cipherBytes);
                }

                if (!string.IsNullOrEmpty(creditCardInfo.CardPin))
                {
                    var cipherBytes = Convert.FromBase64String(creditCardInfo.CardPin);
                    creditCardInfo.CardPin = AesEncryptionDecryptionHelper.Decrypt(cipherBytes);
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
                        model.EntryData.CreditCardInfo = decryptedEntryData.CreditCardInfo;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Decryption failed: {Message}", ex.Message);
                    throw new InvalidOperationException("Decryption failed", ex);
                }
            }
        }

        public async Task<ApiResult<bool>> SaveCreditCardDetails(TemplateEntryDto model)
        {
            try
            {
                Decrypt(model);
                EncryptCreditCardInfo(model);
                var res = await _templateEntryService.Save(model);
                if (!res.IsSuccess && res.Code == "TemplateAlreadyExist")
                {
                    var errorMessage = "Template already exists.";
                    return ApiResult<bool>.Failure(ValidationCodes.FolderAlreadyExists, errorMessage);
                }
                return ApiResult<bool>.Success(true);
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
                var creditCardId = templates.FirstOrDefault(x => x.TemplateKey == "credit_cards".ToString()).Id;
                var mappedEntities = (await _templateEntryService.Search(creditCardId)).Result;
                foreach (var entity in mappedEntities)
                {
                    DecryptCreditCardInfo(entity);
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


        public async Task<ApiResult<TemplateEntryDto>> GetCardDetailsById(Guid id)
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
                    DecryptCreditCardInfo(mappedEntity);
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

        public async Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model)
        {
            try
            {
                Decrypt(model);
                EncryptCreditCardInfo(model);
                var result = await _templateEntryService.Update(id, model);
                if (!result.IsSuccess && result.Code == "TemplateAlreadyExist")
                {
                    var errorMessage = "Template already exists.";
                    return ApiResult<TemplateEntryDto>.Failure(ValidationCodes.TemplateAlreadyExists, errorMessage);
                }
                return ApiResult<TemplateEntryDto>.Success(result.Result);
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


        public async Task<ApiResult<List<ApplicationUserDto>>> GetAllUsers()
        {
            try
            {
                var entities = await _applicationUserRepository.QueryAsync((e => e.IsActive && e.FullName != null), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: entities => entities.OrderBy(x => x.FullName));
                var mappedEntities = _mapper.Map<List<ApplicationUserDto>>(entities);
                return ApiResult<List<ApplicationUserDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<List<GroupDto>>> GetAllGroups()
        {
            try
            {
                var entities = await _groupRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: entities => entities.OrderBy(x => x.Name));
                var mappedEntities = _mapper.Map<List<GroupDto>>(entities);
                return ApiResult<List<GroupDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


    }

}

