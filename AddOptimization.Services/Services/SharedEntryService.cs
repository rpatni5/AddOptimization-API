using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
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
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace AddOptimization.Services.Services
{
    public class SharedEntryService : ISharedEntryService
    {
        private readonly IGenericRepository<SharedEntry> _sharedEntryRepository;
        private readonly ILogger<SharedEntryService> _logger;
        private readonly IMapper _mapper;
        private readonly ICreditCardService _creditCardService;
        private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
        private readonly IGenericRepository<Group> _groupRepository;
        private readonly INotificationService _notificationService;
        private readonly ITemplatesService _templateService;
        private readonly IEmployeeService _employeeService;
        private readonly ITemplateEntryService _templateEntryService;
        private readonly IConfiguration _configuration;
        public SharedEntryService(IGenericRepository<SharedEntry> sharedEntryRepository, ILogger<SharedEntryService> logger, IMapper mapper, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository, INotificationService notificationService, ITemplatesService templateService, IEmployeeService employeeService, ITemplateEntryService templateEntryService, IConfiguration configuration)
        {
            _sharedEntryRepository = sharedEntryRepository;
            _logger = logger;
            _mapper = mapper;
            _applicationUserRepository = applicationUserRepository;
            _groupRepository = groupRepository;
            _notificationService = notificationService;
            _templateService = templateService;
            _employeeService = employeeService;
            _templateEntryService = templateEntryService;
            _configuration = configuration;
        }

        public async Task<ApiResult<bool>> Create(SharedEntryRequestDto model)
        {
            try
            {

                var sharedEntries = new List<SharedEntry>();
                foreach (var item in model.sharedField)
                {
                    var sharedEntry = new SharedEntry
                    {
                        Id = Guid.NewGuid(),
                        EntryId = model.EntryId,
                        SharedByUserId = model.SharedByUserId,
                        SharedWithId = item.Id,
                        SharedWithType = item.Type,
                        PermissionLevel = model.PermissionLevel,
                        SharedDate = model.SharedDate
                    };

                    sharedEntries.Add(sharedEntry);
                }

                await _sharedEntryRepository.BulkInsertAsync(sharedEntries);
                await SendNotificationToAccountAdmin(sharedEntries);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<List<SharedEntryResponseDto>>> GetSharedDataBySharedId(Guid id)
        {
            try
            {
                var entities = (await _sharedEntryRepository.QueryAsync(o => o.EntryId == id && !o.IsDeleted, include: entities => entities.Include(e => e.ApplicationUser), ignoreGlobalFilter: true));
                if (entities == null)
                {
                    return ApiResult<List<SharedEntryResponseDto>>.NotFound("details");
                }
                var users = await _applicationUserRepository.QueryAsync((e => e.IsActive));
                var groups = await _groupRepository.QueryAsync((e => !e.IsDeleted));
                var mappedEntity = _mapper.Map<List<SharedEntryResponseDto>>(entities);

                foreach (var entry in mappedEntity)
                {
                    if (entry.SharedWithType == "user")
                    {
                        var user = users.FirstOrDefault(u => u.Id.ToString() == entry.SharedWithId);
                        entry.SharedWithName = user?.FullName ?? "Unknown User";
                    }
                    else if (entry.SharedWithType == "group")
                    {
                        var group = groups.FirstOrDefault(g => g.Id.ToString() == entry.SharedWithId);
                        entry.SharedWithName = group?.Name ?? "Unknown Group";
                    }
                }

                return ApiResult<List<SharedEntryResponseDto>>.Success(mappedEntity);
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
                var entity = await _sharedEntryRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Card Details");
                }

                entity.IsDeleted = true;
                await _sharedEntryRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<SharedEntryResponseDto>>> Update(Guid id, PermissionLevelDto model)
        {
            try
            {
                var sharedEntries = await _sharedEntryRepository.QueryAsync(e => e.EntryId == id && !e.IsDeleted, ignoreGlobalFilter: true);
                foreach (var item in model.PermissionLevelEntries)
                {
                    var sharedEntry = sharedEntries.FirstOrDefault(e => e.Id == item.Id);
                    if (sharedEntry != null)
                    {
                        sharedEntry.PermissionLevel = item.PermissionLevel;
                        await _sharedEntryRepository.UpdateAsync(sharedEntry);
                    }
                }
                var mappedEntities = _mapper.Map<List<SharedEntryResponseDto>>(sharedEntries);
                return ApiResult<List<SharedEntryResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<List<SharedEntryResponseDto>>> GetByUserId(int id, string filterType)
        {
            try
            {
                IQueryable<SharedEntry> query = await _sharedEntryRepository.QueryAsync(e => !e.IsDeleted && !e.TemplateEntries.IsDeleted, include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.TemplateEntries), orderBy: x => x.OrderByDescending(x => x.CreatedAt));
                if (filterType == "SharedByMe")
                {
                    query = query.Where(e => e.SharedByUserId == id);
                }
                else if (filterType == "SharedToMe")
                {
                    query = query.Where(e => e.SharedWithId == id.ToString());
                }
                query = query.OrderByDescending(e => e.CreatedAt);
                var entities = await query.ToListAsync();
                var mappedEntities = entities.Select(e => new SharedEntryResponseDto
                {
                    Id = e.Id,
                    EntryId = e.EntryId,
                    SharedByUserId = e.SharedByUserId,
                    SharedWithId = e.SharedWithId,
                    PermissionLevel = e.PermissionLevel,
                    SharedFolderName = e.TemplateEntries?.TemplateFolder?.Name ?? string.Empty,
                    SharedTitleName = e.TemplateEntries.Title,
                    CreatedBy = e.CreatedByUser != null ? e.CreatedByUser.FullName : string.Empty,
                    TemplateId = e.TemplateEntries.TemplateId,
                    CreatedByUserId = e.CreatedByUserId,
                }).ToList();
                return ApiResult<List<SharedEntryResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        private async Task SendNotificationToAccountAdmin(List<SharedEntry> sharedEntries)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            var templateRoutes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase){
            { "credit_cards",  $"{baseUrl}admin/password-vault/credit-cards/view-credit-card/"},
            { "secure_notes", $"{baseUrl}admin/password-vault/secure-notes/view-secure-note/"},
            { "passwords",    $"{baseUrl}admin/password-vault/passwords/view-password/"},
            { "personal_information",$"{baseUrl}admin/password-vault/personal-information/view-personal-information/"},
            { "company_information", $"{baseUrl}admin/password-vault/company-information/view-company-information/"},
            { "mobile_application",$"{baseUrl}admin/password-vault/mobile-application/view-mobile-application/" }
            };
            foreach (var item in sharedEntries)
            {
                var entities = (await _sharedEntryRepository.QueryAsync(o => o.EntryId == item.EntryId && !o.IsDeleted, include: entities => entities.Include(e => e.TemplateEntries).Include(e=>e.ApplicationUser), ignoreGlobalFilter: true)).ToList();
                var tempalteId = entities.Select(e => e.EntryId).FirstOrDefault();
                var template = (await _templateService.GetTemplateById(tempalteId)).Result;
                var sharedEntry = entities.FirstOrDefault();
                var sharedByUser = sharedEntry?.ApplicationUser;
                var notifications = new List<NotificationDto>();
                var subject = $"{template.Name} shared by {sharedByUser.FullName}";
                var bodyContent = $"{template.Name} shared by {sharedByUser.FullName}";
                var link = templateRoutes.TryGetValue(template.TemplateKey, out var route) ? $"{route}{tempalteId}?sidenav=collapsed" : "";
                var model = new NotificationDto
                {
                    Subject = subject,
                    Content = bodyContent,
                    Link = link,
                    AppplicationUserId = Convert.ToInt32(item.SharedWithId),
                    GroupKey = $"{template.Name} shared by #{sharedByUser.FullName}",
                };
                notifications.Add(model);

                await _notificationService.BulkCreateAsync(notifications);
            }
        }

    }
}

