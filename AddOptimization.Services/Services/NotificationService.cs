using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Services.Services
{

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Notification> _notificationRepository;
        private readonly int? _currentUserId;
        public NotificationService(ILogger<NotificationService> logger, IMapper mapper, IGenericRepository<Notification> notificationRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _notificationRepository = notificationRepository;
            _currentUserId = notificationRepository.CurrentUserId;
        }
        public async Task<ApiResult<bool>> CreateAsync(NotificationDto model)
        {
            try
            {
                var entity = _mapper.Map<Notification>(model);
                await _notificationRepository.InsertAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<bool>> BulkCreateAsync(List<NotificationDto> model)
        {
            try
            {
                var entities = _mapper.Map<List<Notification>>(model);
                await _notificationRepository.BulkInsertAsync(entities);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<bool>> ToggleRead(int? id)
        {
            try
            {
                var currentUserId = _notificationRepository.CurrentUserId;
                var entities = (await _notificationRepository.QueryAsync(x => (id != null && x.Id == id) || (id == null && x.AppplicationUserId == currentUserId && x.ReadAt == null))).ToList();
                if (!entities.Any())
                {
                    return ApiResult<bool>.NotFound("Notification");
                }
                foreach (var entity in entities)
                {
                    entity.IsRead = entity.IsRead != true || id == null;
                    entity.ReadAt = entity.IsRead == true && id != null ? null : DateTime.UtcNow;
                }
                await _notificationRepository.BulkUpdateAsync(entities);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<PagedApiResult<NotificationDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _notificationRepository.QueryAsync(e => e.AppplicationUserId == _currentUserId, include: entities => entities.Include(e => e.CreatedByUser), orderBy: entities => entities.OrderByDescending(e => e.CreatedAt));

                filters.GetValue<string>("notificationType", (v) =>
                {
                    if (v.ToLower() != "all")
                        entities = entities.Where(e => (e.IsRead ?? false) == (v.ToLower() == "read"));
                });
                var mappedEntities = _mapper.Map<List<NotificationDto>>(entities.ToList());
                var pagedResult = PageHelper<Notification, NotificationDto>.ApplyPaging(entities, filters, entities => _mapper.Map<List<NotificationDto>>(entities.ToList()));
                return PagedApiResult<NotificationDto>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }

}
