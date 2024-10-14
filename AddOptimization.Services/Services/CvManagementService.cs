using AddOptimization.Contracts.Dto;
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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace AddOptimization.Services.Services
{
    public class CvManagementService : ICvManagementService
    {
        private readonly IGenericRepository<CvEntry> _cvEntryRepository;
        private readonly ILogger<CvManagementService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CvManagementService(IGenericRepository<CvEntry> cvEntryRepository, ILogger<CvManagementService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _cvEntryRepository = cvEntryRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        private async Task<List<string>> SaveVersionAndGenerateDownloadUrls(CvEntryDataDto request)
        {
            string path = Path.Combine(_hostingEnvironment.WebRootPath, "services", "certificates");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var downloadUrls = new List<string>();

            foreach (var certificate in request.Certificate)
            {
                if (certificate.File != null && certificate.File.Length > 0) 
                {
                    string fileName = $"CERT_{Guid.NewGuid()}{Path.GetExtension(certificate.File.FileName)}";
                    string filePath = Path.Combine(path, fileName);

                    if (File.Exists(filePath))
                        File.Delete(filePath);

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

                    certificate.CertificatePath = downloadUrl;
                    downloadUrls.Add(downloadUrl);
                }
                else
                {
                    _logger.LogError("File is null or empty for certificate.");
                    throw new Exception("File is null or empty.");
                }
            }

            return downloadUrls;
        }

        private IQueryable<CvEntry> ApplyFilters(IQueryable<CvEntry> entities, PageQueryFiterBase filter)
        {

            filter.GetValue<int>("userId", (v) =>
            {
                entities = entities.Where(e => e.UserId == v);
            });

            filter.GetValue<string>("title", (v) =>
            {
                entities = entities.Where(e => e.Title != null && (e.Title.ToLower().Contains(v.ToLower())));
            });

            return entities;
        }


        private IQueryable<CvEntry> ApplySorting(IQueryable<CvEntry> entities, SortModel sort)
        {
            try
            {
                if (sort?.Name == null)
                {
                    entities = entities.OrderByDescending(o => o.CreatedAt);
                    return entities;
                }
                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {
                    if (columnName.ToUpper() == nameof(CvEntryDto.Title).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Title); ;
                    }

                }

                else
                {
                    if (columnName.ToUpper() == nameof(CvEntryDto.Title).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Title); ;
                    }
                }
                return entities;

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return entities;
            }

        }


        public async Task<ApiResult<bool>> Save(CvEntryDto model)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                List<string> downloadUrls = null;
                if (model.EntryData.Certificate != null && model.EntryData.Certificate.Count > 0)
                {
                    downloadUrls = await SaveVersionAndGenerateDownloadUrls(model.EntryData);
                    for (int i = 0; i < model.EntryData.Certificate.Count; i++)
                    {
                        model.EntryData.Certificate[i].CertificatePath = downloadUrls[i];
                    }
                }
                var entity = _mapper.Map<CvEntry>(model);
                entity.UserId = userId;
                entity.EntryData = JsonSerializer.Serialize(model.EntryData, jsonOptions);
                await _cvEntryRepository.InsertAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<PagedApiResult<CvEntryDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _cvEntryRepository.QueryAsync(e => !e.IsDeleted, include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);
                var pagedResult = PageHelper<CvEntry, CvEntryDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new CvEntryDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    UserId = e.UserId,
                    EntryData = e.EntryData != null ? JsonSerializer.Deserialize<CvEntryDataDto>(e.EntryData.ToString(), jsonOptions) : null,
                    
                }).ToList());

                var result = pagedResult;
                return PagedApiResult<CvEntryDto>.Success(result);
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




    }


}

