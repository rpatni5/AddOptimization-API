using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Services.Services
{
    public class InvoiceService: IInvoiceService
    {
        //private readonly IGenericRepository<User> _userRepository;
        //private readonly ILogger<InvoiceService> _logger;
        //public InvoiceService(IGenericRepository<Invoice> userRepository, ILogger<InvoiceService> logger)
        //{
        //    _userRepository = userRepository;
        //    _logger = logger;
        //}

        //public async Task<ApiResult<List<UserSummaryDto>>> GenerateInvoice()
        //{
        //    try
        //    {
        //        var entities = await _userRepository.QueryAsync();
        //        var usersList = entities.Select(s => new UserSummaryDto
        //        {
        //            Id = s.Id,
        //            FullName = s.FullName
        //        }).OrderBy(x => x.FullName).ToList();

        //        return ApiResult<List<UserSummaryDto>>.Success(usersList);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogException(ex);
        //        throw;
        //    }
        //}
    }
}
