using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Models;
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Services.Constants;
using Microsoft.IdentityModel.Tokens;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Enums;

namespace AddOptimization.Services.Services
{
    public class GuiVersionService : IGuiVersionService
    {
        private readonly IGenericRepository<GuiVersion> _versionRepository;
        private readonly ILogger<GuiVersionService> _logger;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public GuiVersionService(IGenericRepository<GuiVersion> versionRepository, ILogger<GuiVersionService> logger, IMapper mapper, IHostingEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _versionRepository = versionRepository;
            _logger = logger;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApiResult<GuiVersionResponseDto>> Create(GuiVersionCreateDto model)
        {
            try
            {
                if (model.IsLatest)
                {
                    var entities = (await _versionRepository.QueryAsync(x => x.IsLatest && !x.IsDeleted)).ToList();

                    foreach (var item in entities)
                    {
                        item.IsLatest = false;
                    }
                    await _versionRepository.BulkUpdateAsync(entities);
                }

                GuiVersion entity = new GuiVersion();
                entity.FrameworkVersionNo = model.FrameworkVersionNo;
                entity.GuiVersionNo = model.GuiVersionNo;
                entity.IsActive = true;
                entity.IsDeleted = false;
                entity.IsLatest = model.IsLatest;
                //save & get download path
                entity.DownloadPath = await SaveVersionAndGenerateDownloadUrl(model);
                await _versionRepository.InsertAsync(entity);
                var mappedEntity = _mapper.Map<GuiVersionResponseDto>(entity);

                return ApiResult<GuiVersionResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<PagedApiResult<GuiVersionResponseDto>> Search(PageQueryFiterBase filter)
        {
            try
            {
                var entities = await _versionRepository.QueryAsync(
                    e => !e.IsDeleted,
                    include: entities => entities.Include(e => e.CreatedByUser),
                    orderBy: entities => entities.OrderByDescending(c => c.CreatedAt)
                );

                entities = ApplySorting(entities, filter?.Sorted?.FirstOrDefault());

                entities = ApplyFilters(entities, filter);

                var pagedResult = PageHelper<GuiVersion, GuiVersionResponseDto>.ApplyPaging(entities, filter, entities => entities.Select(e => new GuiVersionResponseDto
                {
                    Id = e.Id,
                    GuiVersionNo = e.GuiVersionNo,
                    FrameworkVersionNo = e.FrameworkVersionNo,
                    CreatedBy = e.CreatedByUser.FullName,
                    CreatedAt = e.CreatedAt,
                    IsActive = e.IsActive,
                    IsLatest = e.IsLatest,
                    DownloadPath=e.DownloadPath,
          
    }).ToList());

                return PagedApiResult<GuiVersionResponseDto>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private async Task<string> SaveVersionAndGenerateDownloadUrl(GuiVersionCreateDto request)
        {
            string path = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");

            //create folder if not exist
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //get file extension
            FileInfo fileInfo = new FileInfo(request.VersionFile.FileName);
            string fileName = "GUI_" + request.GuiVersionNo + fileInfo.Extension;
            string fileNameWithPath = Path.Combine(path, fileName);
            if (File.Exists(fileNameWithPath))
                File.Delete(fileNameWithPath);

            var downloadUrl = new UriBuilder
            {
                Scheme = _httpContextAccessor.HttpContext.Request.Scheme,
                Host = _httpContextAccessor.HttpContext.Request.Host.Host,
                Port = _httpContextAccessor.HttpContext.Request.Host.Port ?? -1,
                Path = "/Uploads/" + fileName
            }.ToString();

            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
            {
                await request.VersionFile.CopyToAsync(stream);
            }

            return downloadUrl;
        }



        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            try
            {
                var entity = await _versionRepository.FirstOrDefaultAsync(t => t.Id == id);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Version");
                }
                entity.IsDeleted = true;

                await _versionRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<bool>> UpdateStatus(Guid id)
        {
            try
            {
                var entity = await _versionRepository.FirstOrDefaultAsync(u => u.Id == id, disableTracking: false);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("version");
                }
                entity.IsActive = !entity.IsActive;
                await _versionRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<List<GuiVersionResponseDto>>> GetLatestversion()
        {
            try
            {
                var entities = await _versionRepository.QueryAsync((e => !e.IsDeleted && e.IsLatest) ,include: entities => entities.Include(e => e.CreatedByUser));
                var mappedEntities = _mapper.Map<List<GuiVersionResponseDto>>(entities.ToList());
                return ApiResult<List<GuiVersionResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        private IQueryable<GuiVersion> ApplyFilters(IQueryable<GuiVersion> entities, PageQueryFiterBase filter)
        {
            filter.GetValue<string>("guiVersionNo", (v) =>
            {
                entities = entities.Where(e => e.GuiVersionNo.ToLower().Contains(v.ToLower()));
            });

            filter.GetValue<string>("frameworkVersionNo", (v) =>
            {
                entities = entities.Where(e => e.FrameworkVersionNo.ToLower().Contains(v.ToLower()));
            });

            filter.GetValue<string>("createdBy", (v) =>
            {
                entities = entities.Where(e => e.CreatedByUser != null && e.CreatedByUser.FullName.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<DateTime>("createdAt", (v) =>
            {
                entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt < v);
            }, OperatorType.lessthan, true);
            filter.GetValue<DateTime>("createdAt", (v) =>
            {
                entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt > v);
            }, OperatorType.greaterthan, true);



            return entities;
        }


        private IQueryable<GuiVersion> ApplySorting(IQueryable<GuiVersion> entities, SortModel sort)
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
                    if (columnName == nameof(GuiVersionResponseDto.GuiVersionNo).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.GuiVersionNo);
                    }
                    if (columnName == nameof(GuiVersionResponseDto.FrameworkVersionNo).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.FrameworkVersionNo);
                    }
                    if (columnName == nameof(GuiVersionResponseDto.CreatedBy).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.CreatedByUser.FullName);
                    }
                    if (columnName == nameof(GuiVersionResponseDto.CreatedAt).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.CreatedAt);
                    }
                }
                else
                {
                    if (columnName == nameof(GuiVersionResponseDto.GuiVersionNo).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.GuiVersionNo);
                    }
                    if (columnName == nameof(GuiVersionResponseDto.FrameworkVersionNo).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.FrameworkVersionNo);
                    }
                    if (columnName == nameof(GuiVersionResponseDto.CreatedBy).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.CreatedByUser.FullName);
                    }
                    if (columnName == nameof(GuiVersionResponseDto.CreatedAt).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.CreatedAt);
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

    }
}
