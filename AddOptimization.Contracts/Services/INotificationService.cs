using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface INotificationService
    {
        Task<ApiResult<bool>> CreateAsync(NotificationDto model);
        Task<ApiResult<bool>> BulkCreateAsync(List<NotificationDto> model);
        Task<ApiResult<bool>> ToggleRead(int? id);
        Task<PagedApiResult<NotificationDto>> Search(PageQueryFiterBase filters);

    }
}
