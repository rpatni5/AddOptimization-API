using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
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



        public SecureNoteService(ILogger<SecureNoteService> logger, ITemplatesService templateService, ITemplateEntryService templateEntryService)
        {
            _logger = logger;
            _templateService = templateService;
            _templateEntryService = templateEntryService;
        }


        public async Task<ApiResult<bool>> SaveSecureNote(TemplateEntryDto model)
        {
            try
            {
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
                var secureNoteId = templates.FirstOrDefault(x => x.Name == "Secure Notes".ToString()).Id;
                var searchResult = (await _templateEntryService.Search(secureNoteId)).Result;
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
                return ApiResult<TemplateEntryDto>.Success(mappedEntity);
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

