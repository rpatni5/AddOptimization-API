using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AddOptimization.Services.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly IGenericRepository<TemplateEntries> _templateEntryRepository;
        private readonly ILogger<CreditCardService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreditCardService(IGenericRepository<TemplateEntries> templateEntryRepository, ILogger<CreditCardService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _templateEntryRepository = templateEntryRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<ApiResult<bool>> SaveCreditCardDetails(TemplateEntryDto model)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var entity = _mapper.Map<TemplateEntries>(model);
                entity.UserId = userId;
                entity.FolderId = model.FolderId;
                entity.EntryData = JsonConvert.SerializeObject(model.EntryData);
                await _templateEntryRepository.InsertAsync(entity);
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

