using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Services.Services;

public class EmployeeIdentityService : IEmployeeIdentityService
{
    private readonly ILogger<EmployeeIdentityService> _logger;
    private readonly IGenericRepository<EmployeeIdentity> _employeeIdenityRepository;
    private readonly IMapper _mapper;

    public EmployeeIdentityService(ILogger<EmployeeIdentityService> logger, IGenericRepository<EmployeeIdentity> EmployeeIdentityRepository, IMapper mapper)
    {
        _logger = logger;
        _employeeIdenityRepository = EmployeeIdentityRepository;
        _mapper = mapper;
    }

    public async Task<ApiResult<List<EmployeeIdentityDto>>> Search()
    {
        try
        {
            var entities = await _employeeIdenityRepository.QueryAsync();
            var mappedEntities = _mapper.Map<List<EmployeeIdentityDto>>(entities.ToList());  
            return ApiResult<List<EmployeeIdentityDto>>.Success(mappedEntities);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }
}

