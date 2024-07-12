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
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Constants;
using Newtonsoft.Json;
using iText.Commons.Actions.Contexts;
using AddOptimization.Contracts.Constants;
using System.Globalization;
using AddOptimization.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using AddOptimization.Utilities.Services;

namespace AddOptimization.Services.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly IGenericRepository<Quote> _quoteRepository;
        private readonly IGenericRepository<QuoteSummary> _quoteSummaryRepository;
        private readonly ILogger<QuoteService> _logger;
        private readonly IMapper _mapper;
        private readonly IQuoteStatusService _quoteStatusService;
        private readonly IQuoteSummaryService _quoteSummaryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Company> _companyRepository;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly CustomDataProtectionService _protectionService;

        public QuoteService(IGenericRepository<Quote> quoteRepository, ILogger<QuoteService> logger, IMapper mapper, IQuoteStatusService quoteStatusService, IGenericRepository<QuoteSummary> quoteSummaryRepository, IUnitOfWork unitOfWork, IGenericRepository<Company> companyRepository, IConfiguration configuration, IEmailService emailService, ITemplateService templateService, CustomDataProtectionService protectionService)
        {
            _quoteRepository = quoteRepository;
            _logger = logger;
            _mapper = mapper;
            _quoteStatusService = quoteStatusService;
            _quoteSummaryRepository = quoteSummaryRepository;
            _unitOfWork = unitOfWork;
            _companyRepository = companyRepository;
            _configuration = configuration;
            _emailService = emailService;
            _templateService = templateService;
            _unitOfWork = unitOfWork;
            _protectionService = protectionService;
        }

        public async Task<ApiResult<List<QuoteResponseDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _quoteRepository.QueryAsync((e => !e.IsDeleted || e.IsActive), include: source => source.Include(x => x.Customer).Include(x => x.QuoteStatuses), orderBy: x => x.OrderByDescending(x => x.CreatedAt), ignoreGlobalFilter: true);

                var result = entities.ToList();
                var mappedEntities = _mapper.Map<List<QuoteResponseDto>>(result);

                return ApiResult<List<QuoteResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<QuoteResponseDto>> Create(QuoteRequestDto model)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var eventStatus = (await _quoteStatusService.Search()).Result;
                var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.DRAFT.ToString()).Id;
                var quoteNo = await GenerateQuoteNoAsync();
                var company = await _companyRepository.FirstOrDefaultAsync(ignoreGlobalFilter: true);
                var id = await _quoteRepository.MaxAsync(e => (int)e.Id, ignoreGlobalFilter: true);

                var companyAddress = string.Empty;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(company.CompanyName ?? string.Empty);
                sb.AppendLine(company.Address ?? string.Empty);
                sb.AppendLine(company.City ?? string.Empty);
                sb.AppendLine(company.State ?? string.Empty);
                sb.AppendLine(company.ZipCode.ToString() ?? string.Empty);
                companyAddress = sb.ToString();

                var companyBankDetails = string.Empty;

                StringBuilder bd = new StringBuilder();
                bd.AppendLine(company.BankName);
                bd.AppendLine(company.BankAccountName);
                bd.AppendLine(company.BankAccountNumber);
                bd.AppendLine(company.BankAddress);
                companyBankDetails = bd.ToString();

                Quote entity = new Quote
                {
                    Id= id+1,
                    CustomerId = model.CustomerId,
                    ExpiryDate = model.ExpiryDate,
                    QuoteDate = model.QuoteDate,
                    CustomerAddress = model.CustomerAddress,
                    CompanyAddress = companyAddress,
                    CompanyBankAddress = companyBankDetails,
                    QuoteStatusId = statusId,
                    QuoteNo = quoteNo,
                    QuoteSummaries = new List<QuoteSummary>()
                };
                await _quoteRepository.InsertAsync(entity);
                foreach (var summary in model.QuoteSummaries)
                {
                    var quoteSummary = new QuoteSummary
                    {
                        QuoteId = entity.Id,
                        Name = summary.Name,
                        Quantity = summary.Quantity,
                        Vat = summary.Vat,
                        UnitPrice = summary.UnitPrice,
                        TotalPriceExcVat = summary.TotalPriceExcVat,
                        TotalPriceIncVat = summary.TotalPriceIncVat
                    };

                    await _quoteSummaryRepository.InsertAsync(quoteSummary);
                    entity.QuoteSummaries.Add(quoteSummary);
                }
                await _unitOfWork.CommitTransactionAsync();
                var mappedEntity = _mapper.Map<QuoteResponseDto>(entity);
                return ApiResult<QuoteResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<QuoteResponseDto>> Update(long id, QuoteRequestDto model)
        {
            try
            {
                var isExists = await _quoteRepository.IsExist(e => e.Id != id);
                var entity = await _quoteRepository.FirstOrDefaultAsync(e => e.Id == id);
                var summaries = await _quoteSummaryRepository.QueryAsync(e => e.QuoteId == id);

                foreach (var summary in summaries.ToList())
                {
                    await _quoteSummaryRepository.DeleteAsync(summary);
                }
                if (entity == null)
                {
                    return ApiResult<QuoteResponseDto>.NotFound("Quote");
                }
                foreach (var summary in model.QuoteSummaries)
                {
                    var quoteSummary = new QuoteSummary
                    {
                        QuoteId = entity.Id,
                        Name = summary.Name,
                        Quantity = summary.Quantity,
                        Vat = summary.Vat,
                        UnitPrice = summary.UnitPrice,
                        TotalPriceExcVat = summary.TotalPriceExcVat,
                        TotalPriceIncVat = summary.TotalPriceIncVat
                    };
                    await _quoteSummaryRepository.InsertAsync(quoteSummary);
                }

                _mapper.Map(model, entity);
                await _quoteRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<QuoteResponseDto>(entity);
                return ApiResult<QuoteResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<QuoteResponseDto>> FetchQuoteDetails(long id)
        {
            try
            {
                var model = new QuoteResponseDto();
                var entity = await _quoteRepository.FirstOrDefaultAsync(e => e.Id == id, ignoreGlobalFilter: true);
                model.Id = entity.Id;
                model.CustomerId = entity.CustomerId;
                model.ExpiryDate = entity.ExpiryDate;
                model.QuoteDate = entity.QuoteDate;
                model.CustomerAddress = entity.CustomerAddress;
                model.QuoteStatusId = entity.QuoteStatusId;
                model.QuoteNo = entity.QuoteNo;
                model.CompanyAddress = entity.CompanyAddress;
                model.CompanyBankAddress = entity.CompanyBankAddress;

                var quoteSummary = (await _quoteSummaryRepository.QueryAsync(e => e.QuoteId == id, disableTracking: true)).ToList();

                model.QuoteSummaries = _mapper.Map<List<QuoteSummaryDto>>(quoteSummary);


                return ApiResult<QuoteResponseDto>.Success(model);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private async Task<long> GenerateQuoteNoAsync()
        {
            var id = await _quoteRepository.MaxAsync<Int64>(e => e.Id, ignoreGlobalFilter: true);
            var now = DateTime.UtcNow;
            var currentYear = now.Year;
            var currentMonth = now.Month;

            long maxIncrement = 0;

            if (id != 0)
            {
                maxIncrement = id;
            }

            long newIncrement = maxIncrement + 1;
            return Convert.ToInt64($"{currentYear}{currentMonth:D2}{newIncrement}");
        }

        public string GetQuoteLinkForCustomer(long quoteId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            var encryptedId = _protectionService.Encode(quoteId.ToString());
            return $"{baseUrl}quote/approval/{encryptedId}";
        }

        public async Task<bool> SendQuoteEmailToCustomer(long quoteId)
        {
            var eventStatus = (await _quoteStatusService.Search()).Result;
            var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.SEND_TO_CUSTOMER.ToString()).Id;
            var entity = (await _quoteRepository.QueryAsync(x => x.Id == quoteId, include: entities => entities.Include(e => e.Customer))).FirstOrDefault();
            entity.QuoteStatusId = statusId;
            await _quoteRepository.UpdateAsync(entity);
            return await SendQuoteToCustomer(entity.Customer.ManagerEmail, entity, entity.Customer.ManagerName, entity.Customer.Organizations);
        }

        private async Task<bool> SendQuoteToCustomer(string email, Quote quote, string managerName,
                                    string customer)
        {
            try
            {
                var subject = "Quote";
                var link = GetQuoteLinkForCustomer(quote.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.CustomerQuote);
                emailTemplate = emailTemplate.Replace("[CustomerName]", customer)
                                             .Replace("[MangerName]", managerName)
                                             .Replace("[LinkToQuote]", link)
                                             .Replace("[Month]", DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(quote.QuoteDate.Month))
                                             .Replace("[Year]", quote.QuoteDate.Year.ToString());
                return await _emailService.SendEmail(email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }
    }
}
