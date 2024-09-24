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
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IGenericRepository<Group> _groupRepository;

        public CreditCardService(IGenericRepository<TemplateEntries> templateEntryRepository, ILogger<CreditCardService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository)
        {
            _templateEntryRepository = templateEntryRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _applicationUserRepository = applicationUserRepository;
            _groupRepository = groupRepository;
        }


        public async Task<ApiResult<bool>> SaveCreditCardDetails(TemplateEntryDto model)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var entity = _mapper.Map<TemplateEntries>(model);
                entity.UserId = userId;
                entity.FolderId = model.FolderId;

                var (key, iv) = GetEncryptionKeyAndIV();

                if (model.EntryData?.CreditCardInfo != null)
                {
                    var creditCardInfo = model.EntryData.CreditCardInfo;
                    if (!string.IsNullOrEmpty(creditCardInfo.Cvv))
                    {
                        creditCardInfo.Cvv = Convert.ToBase64String(AesEncryptionDecryptionHelper.Encrypt(creditCardInfo.Cvv, key, iv));
                        model.EntryData.IsValueEncrypted = true;
                    }

                    if (!string.IsNullOrEmpty(creditCardInfo.CardPin))
                    {
                        creditCardInfo.CardPin = Convert.ToBase64String(AesEncryptionDecryptionHelper.Encrypt(creditCardInfo.CardPin, key, iv));
                        model.EntryData.IsValueEncrypted = true;
                    }
                }

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


        public async Task<ApiResult<List<TemplateEntryDto>>> Search()
        {
            try
            {
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var entities = await _templateEntryRepository.QueryAsync(
                    e => !e.IsDeleted && e.UserId == currentUserId,
                    include: entities => entities
                        .Include(e => e.CreatedByUser).Include(e =>e.TemplateFolder).Include(e=>e.Template)
                        .Include(e => e.UpdatedByUser)
                        .Include(e => e.ApplicationUser),
                    orderBy: x => x.OrderByDescending(x => x.CreatedAt)
                );

                var mappedEntities = _mapper.Map<List<TemplateEntryDto>>(entities.ToList());

                foreach (var entity in mappedEntities)
                {

                    var creditCardInfo = entity.EntryData?.CreditCardInfo;
                    if (creditCardInfo != null && entity.EntryData.IsValueEncrypted == true)
                    {
                        var (key, iv) = GetEncryptionKeyAndIV();

                        if (!string.IsNullOrEmpty(creditCardInfo.Cvv))
                        {
                            var cipherBytes = Convert.FromBase64String(creditCardInfo.Cvv);
                            creditCardInfo.Cvv = AesEncryptionDecryptionHelper.Decrypt(cipherBytes, key, iv);
                        }

                        if (!string.IsNullOrEmpty(creditCardInfo.CardPin))
                        {
                            var cipherBytes = Convert.FromBase64String(creditCardInfo.CardPin);
                            creditCardInfo.CardPin = AesEncryptionDecryptionHelper.Decrypt(cipherBytes, key, iv);
                        }
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
                var entity = (await _templateEntryRepository.QueryAsync(o => o.Id == id && !o.IsDeleted, ignoreGlobalFilter: true)).FirstOrDefault();
                if (entity == null)
                {
                    return ApiResult<TemplateEntryDto>.NotFound("Country");
                }

                var mappedEntity = _mapper.Map<TemplateEntryDto>(entity);
               
                    var creditCardInfo = mappedEntity.EntryData?.CreditCardInfo;
                    if (creditCardInfo != null && mappedEntity.EntryData.IsValueEncrypted == true)
                    {
                        var (key, iv) = GetEncryptionKeyAndIV();

                        if (!string.IsNullOrEmpty(creditCardInfo.Cvv))
                        {
                            var cipherBytes = Convert.FromBase64String(creditCardInfo.Cvv);
                            creditCardInfo.Cvv = AesEncryptionDecryptionHelper.Decrypt(cipherBytes, key, iv);
                        }

                        if (!string.IsNullOrEmpty(creditCardInfo.CardPin))
                        {
                            var cipherBytes = Convert.FromBase64String(creditCardInfo.CardPin);
                            creditCardInfo.CardPin = AesEncryptionDecryptionHelper.Decrypt(cipherBytes, key, iv);
                        }
                    }
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
                var entity = await _templateEntryRepository.FirstOrDefaultAsync(e => e.Id == id);
                _mapper.Map(model, entity);
                entity.FolderId = model.FolderId;

                var (key, iv) = GetEncryptionKeyAndIV();

                if (model.EntryData?.CreditCardInfo != null)
                {
                    var creditCardInfo = model.EntryData.CreditCardInfo;
                    if (!string.IsNullOrEmpty(creditCardInfo.Cvv))
                    {
                        creditCardInfo.Cvv = Convert.ToBase64String(AesEncryptionDecryptionHelper.Encrypt(creditCardInfo.Cvv, key, iv));
                        model.EntryData.IsValueEncrypted = true;
                    }

                    if (!string.IsNullOrEmpty(creditCardInfo.CardPin))
                    {
                        creditCardInfo.CardPin = Convert.ToBase64String(AesEncryptionDecryptionHelper.Encrypt(creditCardInfo.CardPin, key, iv));
                        model.EntryData.IsValueEncrypted = true;
                    }
                }

                entity.EntryData = JsonConvert.SerializeObject(model.EntryData);
                await _templateEntryRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<TemplateEntryDto>(entity);
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
                var entity = await _templateEntryRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Card Details");
                }

                entity.IsDeleted = true;
                await _templateEntryRepository.UpdateAsync(entity);
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


        private (byte[] key, byte[] iv) GetEncryptionKeyAndIV()
        {
            var key = Encoding.ASCII.GetBytes(_configuration["EncryptionSettings:Key"]);
            var iv = Encoding.ASCII.GetBytes(_configuration["EncryptionSettings:IV"]);
            if (key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes for AES-256.");

            if (iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes for AES.");
            return (key, iv);
        }


    }

}

