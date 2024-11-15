﻿using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ICreditCardService
    {
        Task<ApiResult<bool>> SaveCreditCardDetails(TemplateEntryDto model);
        Task<ApiResult<TemplateEntryDto>> GetCardDetailsById(Guid id);
        Task<ApiResult<List<TemplateEntryDto>>> Search();
        Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<List<ApplicationUserDto>>> GetAllUsers();
        Task<ApiResult<List<GroupDto>>> GetAllGroups();
    }
}

