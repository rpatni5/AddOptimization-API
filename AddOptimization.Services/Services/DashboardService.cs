using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace AddOptimization.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IGenericRepository<Quote> _quoteRepository;
        private readonly ILogger<AbsenceApprovalSevice> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IGenericRepository<Invoice> _invoiceRepository;
        private readonly IGenericRepository<InvoiceStatus> _invoiceStatusRepository;
        private readonly IGenericRepository<Data.Entities.QuoteStatuses> _quoteStatusRepository;
        public DashboardService(IGenericRepository<Quote> quoteRepository,
            ILogger<AbsenceApprovalSevice> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILeaveStatusesService leaveStatusesService,
            IConfiguration configuration,
            IEmailService emailService,
            ITemplateService templateService,
            IGenericRepository<ApplicationUser> applicationUserRepository,
            IApplicationUserService applicationUserService,
             IGenericRepository<Invoice> invoiceRepository,
             IGenericRepository<InvoiceStatus> invoiceStatusRepository,
             IGenericRepository<Data.Entities.QuoteStatuses> quoteStatusRepository
            )
        {
          
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _applicationUserService = applicationUserService;
            _invoiceRepository = invoiceRepository;
            _invoiceStatusRepository = invoiceStatusRepository;
            _quoteRepository = quoteRepository;
            _quoteStatusRepository = quoteStatusRepository;

        }

        public async Task<ApiResult<List<DashboardDetailDto>>> GetAllDashboardDetail()
        {

            try
            {
                var entities = await _invoiceRepository.QueryAsync((e => !e.IsDeleted));
                var entitiesInvoiceStatus = await _invoiceStatusRepository.QueryAsync(); ;
                List<DashboardDetailDto> dtoList = new List<DashboardDetailDto>();

                foreach ( var entity in entitiesInvoiceStatus.ToList())
                {
                    DashboardDetailDto dto = new DashboardDetailDto();
                    dto.Type = "Invoice";
                    if (entity.StatusKey == nameof(StatusKey.DRAFT))
                    {
                        dto.NoOfInvoice = entities.Where(x => x.InvoiceStatusId == entity.Id).Count();
                        dto.Amount = entities.Where(x => x.InvoiceStatusId == entity.Id).Sum(x => x.DueAmount);
                        dto.Name = nameof(StatusName.Draft);
                        dto.Color = ColorStatus.Draft;
                        dtoList.Add(dto);
                    }
                  
                    else if(entity.StatusKey == nameof(StatusKey.SEND_TO_CUSTOMER))
                    {
                        
                        dto.NoOfInvoice = entities.Where(x => x.InvoiceStatusId == entity.Id && x.ExpiryDate >= DateTime.UtcNow).Count();
                        dto.Amount = entities.Where(x => x.InvoiceStatusId == entity.Id && x.ExpiryDate >= DateTime.UtcNow).Sum(x => x.DueAmount);
                        dto.Name = nameof(StatusName.Unpaid);
                        dto.Color = ColorStatus.Unpaid;
                        dtoList.Add(dto);
                        DashboardDetailDto dto1 = new DashboardDetailDto();
                        dto1.Type = "Invoice";
                        dto1.NoOfInvoice = entities.Where(x => x.InvoiceStatusId == entity.Id && x.ExpiryDate < DateTime.UtcNow).Count();
                        dto1.Amount = entities.Where(x => x.InvoiceStatusId == entity.Id && x.ExpiryDate < DateTime.UtcNow).Sum(x => x.DueAmount);
                        dto1.Name = nameof(StatusName.Overdue);
                        dto1.Color = ColorStatus.Overdue;
                        dtoList.Add(dto1);
                    }
                    else if (entity.StatusKey == nameof(StatusKey.CLOSED))
                    {
                        dto.NoOfInvoice = entities.Where(x => x.InvoiceStatusId == entity.Id ).Count();
                        dto.Amount = entities.Where(x => x.InvoiceStatusId == entity.Id).Sum(x => x.TotalPriceIncludingVat);
                        dto.Name = nameof(StatusName.Paid);
                        dto.Color = ColorStatus.Paid;
                        dtoList.Add(dto);
                    }
                   
                }
                //for Qoutes

                var entitiesQoutes = await _quoteRepository.QueryAsync((e => !e.IsDeleted || e.IsActive));
                var entitiesQoutesStatus = await _quoteStatusRepository.QueryAsync();


                foreach (var entity in entitiesQoutesStatus.ToList())
                {
                    DashboardDetailDto dtoQoutes = new DashboardDetailDto();
                    dtoQoutes.Type = "Quotes";
                    if (entity.StatusKey == nameof(StatusKey.DRAFT))
                    {
                        dtoQoutes.NoOfInvoice = entitiesQoutes.Where(x => x.QuoteStatusId == entity.Id).Count();
                        dtoQoutes.Amount = entitiesQoutes.Where(x => x.QuoteStatusId == entity.Id).Sum(x => x.TotalPriceIncVat);
                        dtoQoutes.Name = nameof(StatusName.Draft);
                        dtoQoutes.Color = ColorStatus.Draft;
                        dtoList.Add(dtoQoutes);
                    }

                    else if (entity.StatusKey == nameof(StatusKey.SEND_TO_CUSTOMER))
                    {

                        dtoQoutes.NoOfInvoice = entitiesQoutes.Where(x => x.QuoteStatusId == entity.Id && x.ExpiryDate >= DateTime.UtcNow).Count();
                        dtoQoutes.Amount = entitiesQoutes.Where(x => x.QuoteStatusId == entity.Id && x.ExpiryDate >= DateTime.UtcNow).Sum(x => x.TotalPriceIncVat);
                        dtoQoutes.Name = nameof(StatusName.Open);
                        dtoQoutes.Color = ColorStatus.Open;
                        dtoList.Add(dtoQoutes);
                        DashboardDetailDto dtoQoutes1 = new DashboardDetailDto();
                        dtoQoutes1.Type = "Quotes";
                        dtoQoutes1.NoOfInvoice = entitiesQoutes.Where(x => x.QuoteStatusId == entity.Id && x.ExpiryDate < DateTime.UtcNow).Count();
                        dtoQoutes1.Amount = entitiesQoutes.Where(x => x.QuoteStatusId == entity.Id && x.ExpiryDate < DateTime.UtcNow).Sum(x => x.TotalPriceIncVat);
                        dtoQoutes1.Name = nameof(StatusName.Overdue);
                        dtoQoutes1.Color = ColorStatus.Overdue;
                        dtoList.Add(dtoQoutes1);
                    }
                    else if (entity.StatusKey == nameof(StatusKey.CLOSED))
                    {
                        dtoQoutes.NoOfInvoice = entitiesQoutes.Where(x => x.QuoteStatusId == entity.Id).Count();
                        dtoQoutes.Amount = entitiesQoutes.Where(x => x.QuoteStatusId == entity.Id).Sum(x => x.TotalPriceIncVat);
                        dtoQoutes.Name = nameof(StatusName.Paid);
                        dtoQoutes.Color = ColorStatus.Paid;
                        dtoList.Add(dtoQoutes);
                    }

                }
               return ApiResult<List<DashboardDetailDto>>.Success(dtoList);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }

        }
    }
}

