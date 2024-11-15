﻿using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Stripe;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Services;
using AddOptimization.Contracts.Constants;
using AddOptimization.Utilities.Interface;
using System.Globalization;


namespace AddOptimization.Services.Services
{
    public class CvManagementService : ICvManagementService
    {
        private readonly IGenericRepository<CvEntry> _cvEntryRepository;
        private readonly IGenericRepository<CvEntryHistory> _cvEntryHistoryRepository;
        private readonly ILogger<CvManagementService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly CustomDataProtectionService _protectionService;
        private readonly ITemplateService _templateService;
        private readonly IEmailService _emailService;

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CvManagementService(IGenericRepository<CvEntry> cvEntryRepository, ILogger<CvManagementService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IConfiguration configuration, IGenericRepository<CvEntryHistory> cvEntryHistoryRepository, CustomDataProtectionService protectionService, ITemplateService templateService, IEmailService emailService)
        {
            _cvEntryRepository = cvEntryRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _cvEntryHistoryRepository = cvEntryHistoryRepository;
            _protectionService = protectionService;
            _templateService = templateService;
            _emailService = emailService;
        }

        private async Task<List<string>> SaveVersionAndGenerateDownloadUrls(CvEntryDataDto request)
        {
            string path = Path.Combine(_hostingEnvironment.WebRootPath, "services", "certificates");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var downloadUrls = new List<string>();

            var existingCertificateFilenames = new HashSet<string>(
                request.Certificate
                    .Where(c => !string.IsNullOrEmpty(c.DownloadUrl))
                    .Select(c => Path.GetFileName(c.DownloadUrl))
            );

            foreach (var certificate in request.Certificate)
            {
                if (!string.IsNullOrEmpty(certificate.DownloadUrl) && existingCertificateFilenames.Contains(Path.GetFileName(certificate.DownloadUrl)))
                {
                    downloadUrls.Add(certificate.DownloadUrl);
                    continue;
                }

                if (certificate.File != null && certificate.File.Length > 0)
                {
                    string fileName = $"CERT_{Guid.NewGuid()}{Path.GetExtension(certificate.File.FileName)}";
                    string filePath = Path.Combine(path, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await certificate.File.CopyToAsync(stream);
                    }

                    var downloadUrl = new UriBuilder
                    {
                        Scheme = _httpContextAccessor.HttpContext.Request.Scheme,
                        Host = _httpContextAccessor.HttpContext.Request.Host.Host,
                        Port = _httpContextAccessor.HttpContext.Request.Host.Port ?? -1,
                        Path = $"/services/certificates/{fileName}"
                    }.ToString();

                    certificate.DownloadUrl = downloadUrl;

                    downloadUrls.Add(downloadUrl);
                }
                else
                {
                    _logger.LogError("File is null or empty for certificate and no DownloadUrl exists.");
                    throw new Exception("File is null or empty, and no DownloadUrl exists.");
                }
            }

            return downloadUrls;
        }

        private IQueryable<CvEntry> ApplyFilters(IQueryable<CvEntry> entities, PageQueryFiterBase filter)
        {

            var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;

            filter.GetValue<string>("employeeId", (v) =>
            {
                entities = entities.Where(e => e.UserId == userId);
            });

            filter.GetValue<string>("employeeName", (v) =>
            {
                entities = entities.Where(e => e.ApplicationUser.FullName.ToLower().Contains(v.ToLower()));
            });

            filter.GetValue<string>("createdBy", (v) =>
            {
                entities = entities.Where(e => e.CreatedByUser.FullName.ToLower().Contains(v.ToLower()));
            });

            filter.GetValue<string>("createdByUserId", (v) =>
            {
                entities = entities.Where(e => e.CreatedByUserId == userId);
            });

            filter.GetValue<string>("title", (v) =>
            {
                if (!string.IsNullOrEmpty(v))
                {
                    entities = entities.Where(e => e.Title.ToLower().Contains(v.ToLower()));
                }
            });

            filter.GetValue<DateTime>("updatedAt", (v) =>
            {
                entities = entities.Where(e =>(e.UpdatedAt.HasValue && e.UpdatedAt < v) ||(!e.UpdatedAt.HasValue && e.CreatedAt != null && e.CreatedAt < v));
            }, OperatorType.lessthan, true);
            filter.GetValue<DateTime>("updatedAt", (v) =>
            {
                entities = entities.Where(e => (e.UpdatedAt.HasValue && e.UpdatedAt > v) || (!e.UpdatedAt.HasValue && e.CreatedAt != null && e.CreatedAt > v));
            }, OperatorType.greaterthan, true);

            return entities;
        }

        private IQueryable<CvEntry> ApplySorting(IQueryable<CvEntry> orders, SortModel sort)
        {
            try
            {
               if (sort?.Name == null)
                {
                    orders = orders.OrderByDescending(o => o.CreatedAt);
                    return orders;
                }
                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {

                    if (columnName.ToUpper() == nameof(CvEntryDto.EmployeeName).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.ApplicationUser.FullName);
                    }
                    if (columnName.ToUpper() == nameof(CvEntryDto.Title).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Title);
                    }
                    if (columnName.ToUpper() == nameof(CvEntryDto.CreatedBy).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.CreatedByUser.FullName);
                    }
                    if (columnName.ToUpper() == nameof(CvEntryDto.UpdatedAt).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.UpdatedAt);
                    }

                }
                else
                {
                    if (columnName.ToUpper() == nameof(CvEntryDto.EmployeeName).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.ApplicationUser.FullName);
                    }
                    if (columnName.ToUpper() == nameof(CvEntryDto.Title).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.Title);
                    }
                    if (columnName.ToUpper() == nameof(CvEntryDto.CreatedBy).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.CreatedByUser.FullName);
                    }
                    if (columnName.ToUpper() == nameof(CvEntryDto.CreatedBy).ToUpper())
                    {
                        orders = orders.OrderByDescending(o => o.UpdatedAt);
                    }

                }
                return orders;

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return orders;
            }
        }


        public async Task<PagedApiResult<CvEntryDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value.ToString();

                var entities = await _cvEntryRepository.QueryAsync(
                    e => !e.IsDeleted,
                    include: entities => entities.Include(e => e.CreatedByUser)
                                                 .Include(e => e.UpdatedByUser)
                                                 .Include(e => e.ApplicationUser),
                    orderBy: x => x.OrderByDescending(x => x.CreatedAt)
                );
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                var filteredEntities = ApplyFilters(entities.AsQueryable(), filters);

                var pagedResult = PageHelper<CvEntry, CvEntryDto>.ApplyPaging(
                    filteredEntities,
                    filters,
                    entities => entities.Select(e => new CvEntryDto
                    {
                        Id = e.Id,
                        UserId = e.UserId,
                        EmployeeName = e.ApplicationUser.FullName,
                        Title = e.Title,
                        CreatedBy = e.CreatedByUser.FullName,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt,
                    }).ToList()
                );
                return PagedApiResult<CvEntryDto>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<bool>> Save(CvEntryDto model)
        {
            try
            {

                var currentUserId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value.ToString();
                int userId;


                if (string.IsNullOrWhiteSpace(model.EntryData.Contact[0].EmployeeId) ||
                    !int.TryParse(model.EntryData.Contact[0].EmployeeId, out userId))
                {
                    userId = int.Parse(currentUserId);
                }

                List<string> downloadUrls = null;
                if (model.EntryData.Certificate != null && model.EntryData.Certificate.Count > 0)
                {
                    downloadUrls = await SaveVersionAndGenerateDownloadUrls(model.EntryData);
                    for (int i = 0; i < model.EntryData.Certificate.Count; i++)
                    {
                        model.EntryData.Certificate[i].DownloadUrl = downloadUrls[i];
                    }
                }
                var entity = _mapper.Map<CvEntry>(model);
                entity.UserId = userId;
                entity.EntryData = JsonSerializer.Serialize(model.EntryData, jsonOptions);
                await _cvEntryRepository.InsertAsync(entity);
                var entityHistory = new CvEntryHistory
                {
                    CVEntryId = entity.Id,
                    EntryData = entity.EntryData,
                };

                await _cvEntryHistoryRepository.InsertAsync(entityHistory);
                return ApiResult<bool>.Success(true);
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
                var entity = await _cvEntryRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Data");
                }

                entity.IsDeleted = true;
                await _cvEntryRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<CvEntryDto>> GetById(Guid id)
        {
            try
            {
                var entity = await _cvEntryRepository.FirstOrDefaultAsync(
                    e => e.Id == id,
                    ignoreGlobalFilter: true
                );

                if (entity == null)
                {
                    return ApiResult<CvEntryDto>.Failure("CV entry not found or access denied.");
                }


                var entryData = string.IsNullOrWhiteSpace(entity.EntryData)
                    ? new CvEntryDataDto()
                    : JsonSerializer.Deserialize<CvEntryDataDto>(entity.EntryData, jsonOptions);

                var cvEntryDto = new CvEntryDto
                {
                    Id = entity.Id,
                    UserId = entity.UserId,
                    IsDeleted = entity.IsDeleted,
                    EntryData = entryData,
                    CreatedAt = entity.CreatedAt,
                    CreatedBy = entity.CreatedByUser?.FullName ?? string.Empty,
                    UpdatedAt = entity.UpdatedAt,
                    UpdatedBy = entity.UpdatedByUser?.FullName ?? string.Empty,
                };

                return ApiResult<CvEntryDto>.Success(cvEntryDto);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return ApiResult<CvEntryDto>.Failure("An error occurred while retrieving the CV entry.");
            }
        }


        public async Task<ApiResult<CvEntryDto>> Update(Guid id, CvEntryDto model)
        {
            try
            {
                var entity = await _cvEntryRepository.FirstOrDefaultAsync(e => e.Id == id);
                if (entity == null)
                {
                    return ApiResult<CvEntryDto>.Failure("CV entry not found.");
                }

                var existingEntryData = JsonSerializer.Deserialize<CvEntryDataDto>(entity.EntryData, jsonOptions);
                if (existingEntryData == null)
                {
                    return ApiResult<CvEntryDto>.Failure("Existing entry data could not be deserialized.");
                }

                if (model.EntryData?.Certificate != null)
                {
                    existingEntryData.Certificate = model.EntryData.Certificate.ToList();

                }
                else
                {
                    existingEntryData.Certificate = null;
                }

                existingEntryData.Contact = model.EntryData.Contact ?? existingEntryData.Contact;
                existingEntryData.Education = model.EntryData.Education == null || !model.EntryData.Education.Any() ? null : model.EntryData.Education;
                existingEntryData.Experience = model.EntryData.Experience == null || !model.EntryData.Experience.Any() ? null : model.EntryData.Experience;
                existingEntryData.Project = model.EntryData.Project == null || !model.EntryData.Project.Any() ? null : model.EntryData.Project;
                existingEntryData.Language = model.EntryData.Language == null || !model.EntryData.Language.Any() ? null : model.EntryData.Language;
                existingEntryData.TechnicalKnowledge = model.EntryData.TechnicalKnowledge == null || !model.EntryData.TechnicalKnowledge.Any() ? null : model.EntryData.TechnicalKnowledge;

                if (!string.IsNullOrEmpty(model.Title))
                {
                    entity.Title = model.Title;

                    if (existingEntryData.Contact != null && existingEntryData.Contact.Any())
                    {
                        existingEntryData.Contact[0].Title = model.Title;
                    }
                }

                entity.EntryData = JsonSerializer.Serialize(existingEntryData, jsonOptions);
                await _cvEntryRepository.UpdateAsync(entity);

                var entityHistory = new CvEntryHistory
                {
                    CVEntryId = entity.Id,
                    EntryData = entity.EntryData,
                };

                await _cvEntryHistoryRepository.InsertAsync(entityHistory);


                var mappedEntity = _mapper.Map<CvEntryDto>(entity);
                return ApiResult<CvEntryDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return ApiResult<CvEntryDto>.Failure("An error occurred while updating the CV entry.");
            }
        }


        public async Task<ApiResult<bool>> SendCv(SendCvDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Sender))
            {
                _logger.LogError(" Sender Email is missing.");
                return ApiResult<bool>.Success(false);
            }
            var cvEntry = (await GetById(model.CvEntryId)).Result;
            var subject = $"CV {model.EmployeeName}";
            var link = GetCvLinkForClient(cvEntry.Id);
            var emailTemplate = _templateService.ReadTemplate(EmailTemplates.SendCvToClient);
            emailTemplate = emailTemplate
                               .Replace("[ClientName]", model.ClientName)
                               .Replace("[EmployeeName]", model.EmployeeName)
                               .Replace("[UserName]", model.UserName)
                               .Replace("[LinkToCv]", link);
            var emailResult = await _emailService.SendEmailSync(model.SendTo,subject,emailTemplate, fromEmail: model.Sender);

            return ApiResult<bool>.Success(emailResult);
        }


        public string GetCvLinkForClient(Guid cvEntryId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            return $"{baseUrl}admin/cv-management/view-cv/{cvEntryId}?sidenav=collapsed&type=cv";
        }
    }

}

