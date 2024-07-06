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

namespace AddOptimization.Services.Services
{
    public class SettingService : ISettingService
    {
        private readonly IGenericRepository<Setting> _settingRepository;
        private readonly ILogger<SettingService> _logger;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SettingService(IGenericRepository<Setting> settingRepository, ILogger<SettingService> logger, IMapper mapper, IHostingEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _settingRepository = settingRepository;
            _logger = logger;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResult<SettingDto>> GetSettingByCode(string settingCode)
        {
            try
            {
                var entity = await _settingRepository.FirstOrDefaultAsync(o => o.Code.Equals(settingCode), ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<SettingDto>.NotFound("Settings");
                }
                var mappedEntity = _mapper.Map<SettingDto>(entity);
                return ApiResult<SettingDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

    }
}
