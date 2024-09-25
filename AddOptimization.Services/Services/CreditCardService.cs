using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
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

        public CreditCardService( ILogger<CreditCardService> logger, IMapper mapper, IConfiguration configuration, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository, ITemplatesService templateService, ITemplateEntryService templateEntryService)
        {
            _logger = logger;
            _mapper = mapper;
            _applicationUserRepository = applicationUserRepository;
            _groupRepository = groupRepository;
            _templateService = templateService;
            _templateEntryService = templateEntryService;
        }

       
        private void EncryptCreditCardInfo(TemplateEntryDto model)       {
           

            if (model.EntryData?.CreditCardInfo != null)
            {
                var creditCardInfo = model.EntryData.CreditCardInfo;
                if (!string.IsNullOrEmpty(creditCardInfo.Cvv))
                {
                    creditCardInfo.Cvv =AesEncryptionDecryptionHelper.Encrypt(creditCardInfo.Cvv);
                    model.EntryData.IsValueEncrypted = true;
                }

                if (!string.IsNullOrEmpty(creditCardInfo.CardPin))
                {
                    creditCardInfo.CardPin =AesEncryptionDecryptionHelper.Encrypt(creditCardInfo.CardPin);
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

        public async Task<ApiResult<bool>> SaveCreditCardDetails(TemplateEntryDto model)
        {
            try
            {
                EncryptCreditCardInfo(model);
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
                var creditCardId = templates.FirstOrDefault(x => x.Name == "Credit Cards".ToString()).Id;
                var mappedEntities = (await _templateEntryService.Search(creditCardId)).Result;
                foreach (var entity in mappedEntities)
                {
                    DecryptCreditCardInfo(entity);
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
                DecryptCreditCardInfo(mappedEntity);
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
                EncryptCreditCardInfo(model);
                var mappedEntity = (await _templateEntryService.Update(id, model)).Result;
                return ApiResult<TemplateEntryDto>.Success(mappedEntity);
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
                var entities = await _applicationUserRepository.QueryAsync((e => e.IsActive), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser));
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
                var entities = await _groupRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser));
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

