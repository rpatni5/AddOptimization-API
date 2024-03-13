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

    internal class CustomerStatusService : ICustomerStatusService
    {
        private readonly ILogger<CustomerStatusService> _logger;
        private readonly IGenericRepository<CustomerStatus> _customerStatusRepository;
        private readonly IMapper _mapper;

        public CustomerStatusService(ILogger<CustomerStatusService> logger, IGenericRepository<CustomerStatus> customerStatusRepository, IMapper mapper)
        {
            _logger = logger;
            _customerStatusRepository = customerStatusRepository;
            _mapper = mapper;
        }

        public async Task<ApiResult<List<CustomerStatusDto>>> Search()
        {
            try
            {
                var entities = await _customerStatusRepository.QueryAsync();
                var mappedEntities = _mapper.Map<List<CustomerStatusDto>>(entities.ToList());
                return ApiResult<List<CustomerStatusDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }

