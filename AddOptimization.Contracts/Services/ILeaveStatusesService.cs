﻿using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
   
    public interface ILeaveStatusesService
    {
        Task<ApiResult<List<LeaveStatusesDto>>> Search(PageQueryFiterBase filters);

    }
}
