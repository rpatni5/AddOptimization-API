using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace AddOptimization.Services.Services
{
    public class SharedEntryService : ISharedEntryService
    {
        private readonly IGenericRepository<SharedEntry> _sharedEntryRepository;
        private readonly ILogger<SharedEntryService> _logger;
        private readonly IMapper _mapper;


        public SharedEntryService(IGenericRepository<SharedEntry> sharedEntryRepository, ILogger<SharedEntryService> logger, IMapper mapper)
        {
            _sharedEntryRepository = sharedEntryRepository;
            _logger = logger;
            _mapper = mapper;

        }

        public async Task<ApiResult<bool>> Create(SharedEntryRequestDto model)
        {
            try
            {

                var sharedEntries = new List<SharedEntry>();
                foreach (var item in model.sharedField)
                {
                    var sharedEntry = new SharedEntry
                    {
                        Id = Guid.NewGuid(),
                        EntryId = model.EntryId,
                        SharedByUserId = model.SharedByUserId,
                        SharedWithId = item.Id,
                        SharedWithType = item.Type,
                        PermissionLevel = model.PermissionLevel,
                        SharedDate = model.SharedDate
                    };

                    sharedEntries.Add(sharedEntry);
                }

                await _sharedEntryRepository.BulkInsertAsync(sharedEntries);
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

