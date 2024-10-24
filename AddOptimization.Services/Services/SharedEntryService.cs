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
        private readonly IGroupService _groupService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<GroupMember> _groupMemberRepository;
        private readonly IGenericRepository<SharedFolder> _sharedFolderRepository;
        private readonly IGenericRepository<TemplateEntries> _templateEntryRepository;

        public SharedEntryService(IGenericRepository<SharedEntry> sharedEntryRepository, ILogger<SharedEntryService> logger, IMapper mapper, IGenericRepository<ApplicationUser> applicationUserRepository, IGenericRepository<Group> groupRepository, INotificationService notificationService, ITemplatesService templateService, IEmployeeService employeeService, ITemplateEntryService templateEntryService, IConfiguration configuration, IGroupService groupService, IHttpContextAccessor httpContextAccessor, IGenericRepository<GroupMember> groupMemberRepository, IGenericRepository<SharedFolder> sharedFolderRepository, IGenericRepository<TemplateEntries> templateEntryRepository)
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
            _groupService = groupService;
            _httpContextAccessor = httpContextAccessor;
            _groupMemberRepository = groupMemberRepository;
            _sharedFolderRepository = sharedFolderRepository;
            _templateEntryRepository = templateEntryRepository;
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
                    if (entry.SharedWithType == SharedWithTypeEnum.USER)
                    {
                        var user = users.FirstOrDefault(u => u.Id.ToString() == entry.SharedWithId);
                        entry.SharedWithName = user?.FullName ?? "Unknown User";
                    }
                    else if (entry.SharedWithType == SharedWithTypeEnum.GROUP)
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


        public async Task<ApiResult<List<TemplateEntryDto>>> GetByUserId(int id, string filterType)
        {
            try
            {
                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value.ToString();
                var groupIds = (await _groupMemberRepository.QueryAsync(x => !x.IsDeleted && x.UserId.ToString() == currentUserId)).Select(x => x.GroupId.ToString().ToLower()).Distinct().ToList();

                var sharedEntries = (await _sharedEntryRepository.QueryAsync(x => !x.IsDeleted && !x.TemplateEntries.IsDeleted && (x.SharedWithId == currentUserId.ToString() || x.SharedByUserId.ToString() == currentUserId || groupIds.Contains(x.SharedWithId)), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser))).ToList();

                var entryIds = sharedEntries.Select(x => x.EntryId).Distinct().ToList();

                var sharedFolders = (await _sharedFolderRepository.QueryAsync(x => !x.IsDeleted && (x.SharedWithId == currentUserId.ToString() || x.SharedByUserId.ToString() == currentUserId || groupIds.Contains(x.SharedWithId)) , include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.TemplateFolder))).ToList();

                var folderIds = sharedFolders.Select(x => x.FolderId).Distinct().ToList();

                var templateEntriesByFolder = await _templateEntryRepository.QueryAsync(te => te.FolderId.HasValue && folderIds.Contains(te.FolderId.Value), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.TemplateFolder).Include(e => e.Template).Include(e => e.UpdatedByUser).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));

                var templateEntriesByEntryId = await _templateEntryRepository.QueryAsync(te => entryIds.Contains(te.Id), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.TemplateFolder).Include(e => e.Template).Include(e => e.UpdatedByUser).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));

                var combinedTemplateEntries = templateEntriesByFolder.ToList().Union(templateEntriesByEntryId.ToList()).DistinctBy(e => e.Id).ToList();

                if (filterType == "SharedByMe")
                {
                    combinedTemplateEntries = combinedTemplateEntries.Where(te =>sharedEntries.Any(se => !se.IsDeleted && se.SharedByUserId == id && se.EntryId == te.Id) || sharedFolders.Any(sf => !sf.IsDeleted && sf.FolderId == te.FolderId && sf.SharedByUserId == id)).ToList();
                }
                else if (filterType == "SharedToMe")
                {
                    combinedTemplateEntries = combinedTemplateEntries.Where(te =>sharedEntries.Any(se => !se.IsDeleted && se.EntryId == te.Id  && (se.SharedWithId == id.ToString() || groupIds.Contains(se.SharedWithId))) || sharedFolders.Any(sf => !sf.IsDeleted && sf.FolderId == te.FolderId && (sf.SharedWithId == id.ToString() || groupIds.Contains(sf.SharedWithId)))).ToList();
                }

                var mappedEntities = combinedTemplateEntries.Select(x => SelectTemplate(x, sharedEntries, sharedFolders, currentUserId)).ToList();
                return ApiResult<List<TemplateEntryDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private static TemplateEntryDto SelectTemplate(TemplateEntries x, List<SharedEntry> sharedEntries, List<SharedFolder> sharedFolders, string currentUserId)
        {
            var entry = sharedEntries.FirstOrDefault(e => e.EntryId == x.Id);
            var sharedFolder = x.FolderId != null ? sharedFolders.FirstOrDefault(f => f.FolderId == x.FolderId) : null;
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string permission = DeterminePermission(x.UserId, currentUserId, entry, sharedFolder);

            return new TemplateEntryDto
            {
                Id = x.Id == null ? Guid.Empty : x.Id,
                Title = x.Title,
                FolderId = x.FolderId,
                UserId = x.UserId,
                TemplateId = x.TemplateId,
                IsDeleted = x.IsDeleted,
                CreatedBy = x.CreatedByUser != null ? x.CreatedByUser.FullName : string.Empty,
                CreatedAt = x.CreatedAt,
                EntryData = x.EntryData == null ? new EntryDataDto() : System.Text.Json.JsonSerializer.Deserialize<EntryDataDto>(x.EntryData, options),
                Permission = permission,
                FolderName = x.TemplateFolder?.Name ?? string.Empty,
            };
        }


        private static string DeterminePermission(int? userId, string currentUserId, SharedEntry entry, SharedFolder sharedFolder)
        {
            if (userId.ToString() == currentUserId)
                return PermissionLevel.FullAccess.ToString();

            var permissions = new List<string>
            {
                entry?.PermissionLevel,sharedFolder?.PermissionLevel
            }.Where(p => p != null).ToList();

            if (permissions.Contains(PermissionLevel.FullAccess.ToString()))
                return PermissionLevel.FullAccess.ToString();
            if (permissions.Contains(PermissionLevel.Edit.ToString()))
                return PermissionLevel.Edit.ToString();
            return PermissionLevel.Read.ToString();
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
                var entities = (await _sharedEntryRepository.QueryAsync(o => o.EntryId == item.EntryId && !o.IsDeleted, include: entities => entities.Include(e => e.TemplateEntries).Include(e => e.ApplicationUser), ignoreGlobalFilter: true)).ToList();
                var tempalteId = entities.Select(e => e.EntryId).FirstOrDefault();
                var template = (await _templateService.GetTemplateById(tempalteId)).Result;
                var sharedEntry = entities.FirstOrDefault();
                var sharedByUser = sharedEntry?.ApplicationUser;
                var notifications = new List<NotificationDto>();
                var subject = $"{template.Name} shared by {sharedByUser.FullName}";
                var bodyContent = $"{template.Name} shared by {sharedByUser.FullName}";
                var link = templateRoutes.TryGetValue(template.TemplateKey, out var route) ? $"{route}{tempalteId}?sidenav=collapsed" : "";
                if (item.SharedWithType == SharedWithTypeEnum.GROUP)
                {
                    Guid groupId;
                    if (Guid.TryParse(item.SharedWithId, out groupId))
                    {
                        var groupResult = await _groupService.GetGroupAndMembersByGroupId(groupId);
                        if (groupResult != null && groupResult.Result != null && groupResult.Result.groupMembers != null)
                        {
                            foreach (var member in groupResult.Result.groupMembers)
                            {
                                var model = new NotificationDto
                                {
                                    Subject = subject,
                                    Content = bodyContent,
                                    Link = link,
                                    AppplicationUserId = member.UserId,
                                    GroupKey = $"{template.Name} shared by #{sharedByUser?.FullName}",
                                };
                                notifications.Add(model);
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid Group.");
                        }
                    }
                }
                else if (item.SharedWithType == SharedWithTypeEnum.USER)
                {
                    var model = new NotificationDto
                    {
                        Subject = subject,
                        Content = bodyContent,
                        Link = link,
                        AppplicationUserId = Convert.ToInt32(item.SharedWithId),
                        GroupKey = $"{template.Name} shared by #{sharedByUser.FullName}",
                    };
                    notifications.Add(model);
                }
                await _notificationService.BulkCreateAsync(notifications);
            }
        }

    }
}

