using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace AddOptimization.Services.Services
{
    public class SecureNoteService : ISecureNoteService
    {
        private readonly ILogger<SecureNoteService> _logger;
        private readonly ITemplatesService _templateService;
        private readonly ITemplateEntryService _templateEntryService;

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SecureNoteService(ILogger<SecureNoteService> logger, ITemplatesService templateService, ITemplateEntryService templateEntryService)
        {
            _logger = logger;
            _templateService = templateService;
            _templateEntryService = templateEntryService;
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
                        model.EntryData.SecureNoteInfo = decryptedEntryData.SecureNoteInfo;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Decryption failed: {Message}", ex.Message);
                    throw new InvalidOperationException("Decryption failed", ex);
                }
            }
        }


        public async Task<ApiResult<bool>> SaveSecureNote(TemplateEntryDto model)
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


        public async Task<ApiResult<List<TemplateEntryDto>>> Search()
        {
            try
            {
                var templates = (await _templateService.GetAllTemplate()).Result;
                var secureNoteId = templates.FirstOrDefault(x => x.TemplateKey == "secure_notes".ToString()).Id;
                var searchResult = (await _templateEntryService.Search(secureNoteId)).Result;
                foreach (var entity in searchResult)
                {
                    if (entity.EntryData != null)
                    {
                        string entryDataJson = JsonSerializer.Serialize(entity.EntryData, jsonOptions);
                        entity.EntryDataEncrypted = DecryptionHelper.Encrypt(entryDataJson);
                        entity.EntryData = null;
                    }
                }
                return ApiResult<List<TemplateEntryDto>>.Success(searchResult);
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

        public async Task<ApiResult<TemplateEntryDto>> GetSecureNoteById(Guid id)
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
                var mappedEntity = (await _templateEntryService.Update(id, model)).Result;
                return ApiResult<TemplateEntryDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

    }

}

