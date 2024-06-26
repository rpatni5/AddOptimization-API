﻿using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;


namespace AddOptimization.Contracts.Services;


public interface ICustomerStatusService
{
    Task<ApiResult<List<CustomerStatusDto>>> Search();
}
