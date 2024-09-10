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
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using AddOptimization.Utilities.Enums;
using Microsoft.AspNet.Identity;
using iText.Layout.Element;

namespace AddOptimization.Services.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly IGenericRepository<Quote> _quoteRepository;
        private readonly IGenericRepository<QuoteSummary> _quoteSummaryRepository;
        private readonly IGenericRepository<Invoice> _invoiceRepository;
        private readonly IGenericRepository<QuoteHistory> _quoteHistoryRepository;
        private readonly IGenericRepository<InvoiceDetail> _invoiceDetailRepository;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly IInvoiceStatusService _invoiceStatusService;
        private readonly ILogger<QuoteService> _logger;
        private readonly IMapper _mapper;
        private readonly IQuoteStatusService _quoteStatusService;
        private readonly IQuoteSummaryService _quoteSummaryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Company> _companyRepository;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IGenericRepository<InvoiceHistory> _invoiceHistoryRepository;
        private readonly IConfiguration _configuration;
        private readonly CustomDataProtectionService _protectionService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IGenericRepository<Customer> _customersRepository;

        public QuoteService(IGenericRepository<Quote> quoteRepository, IGenericRepository<Invoice> invoiceRepository, IGenericRepository<InvoiceDetail> invoiceDetailRepository, IInvoiceStatusService invoiceStatusService, IGenericRepository<Customer> customersRepository,
            IPaymentStatusService paymentStatusService, IApplicationUserService applicationUserService,
          ILogger<QuoteService> logger, IMapper mapper, IQuoteStatusService quoteStatusService, IGenericRepository<QuoteSummary> quoteSummaryRepository, IUnitOfWork unitOfWork, IGenericRepository<Company> companyRepository, IGenericRepository<InvoiceHistory> invoiceHistoryRepository, IGenericRepository<QuoteHistory> quoteHistoryRepository, IConfiguration configuration, IEmailService emailService, ITemplateService templateService, CustomDataProtectionService protectionService)
        {
            _paymentStatusService = paymentStatusService;
            _invoiceStatusService = invoiceStatusService;
            _quoteRepository = quoteRepository;
            _logger = logger;
            _mapper = mapper;
            _quoteStatusService = quoteStatusService;
            _invoiceRepository = invoiceRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _quoteSummaryRepository = quoteSummaryRepository;
            _unitOfWork = unitOfWork;
            _companyRepository = companyRepository;
            _configuration = configuration;
            _emailService = emailService;
            _templateService = templateService;
            _unitOfWork = unitOfWork;
            _protectionService = protectionService;
            _applicationUserService = applicationUserService;
            _customersRepository = customersRepository;
            _quoteHistoryRepository = quoteHistoryRepository;
            _invoiceHistoryRepository = invoiceHistoryRepository;
        }

        public async Task<PagedApiResult<QuoteResponseDto>> Search(PageQueryFiterBase filters)

        {
            try
            {
                var entities = await _quoteRepository.QueryAsync((e => !e.IsDeleted), include: source => source.Include(x => x.Customer).Include(x => x.QuoteStatuses));
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);
                var pagedResult = PageHelper<Quote, QuoteResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new QuoteResponseDto
                {
                    Id = e.Id,
                    CustomerId = e.CustomerId,
                    CustomerAddress = e.CustomerAddress,
                    QuoteDate = e.QuoteDate,
                    CustomerName = e.Customer.Organizations,
                    ExpiryDate = e.ExpiryDate,
                    QuoteStatusId = e.QuoteStatusId,
                    QuoteStatusesName = e.QuoteStatuses.Name,

                }).ToList());
                var retVal = pagedResult;
                return PagedApiResult<QuoteResponseDto>.Success(retVal);
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
                var company = await (await _companyRepository.QueryAsync(include: entities => entities.Include(e => e.CountryName))).FirstOrDefaultAsync();
                var id = await _quoteRepository.MaxAsync(e => (int)e.Id, ignoreGlobalFilter: true);

                var companyAddress = string.Empty;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(company.Address ?? string.Empty);
                string postalCityState = $"{company.ZipCode.ToString() ?? string.Empty} {company.City ?? string.Empty}";
                if (!string.IsNullOrWhiteSpace(company.State))
                {
                    postalCityState += $", {company.State ?? string.Empty}";
                }
                sb.AppendLine(postalCityState);
                sb.AppendLine(company.CountryName.CountryName);
                sb.AppendLine($"{company.TaxNumber ?? string.Empty}");
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
                    Id = id + 1,
                    CustomerId = model.CustomerId,
                    ExpiryDate = model.ExpiryDate,
                    QuoteDate = model.QuoteDate,
                    CustomerAddress = model.CustomerAddress,
                    CompanyAddress = companyAddress,
                    CompanyBankAddress = companyBankDetails,
                    QuoteStatusId = statusId,
                    QuoteNo = quoteNo,
                    TotalPriceExcVat = model.QuoteSummaries.ToList().Sum(x => x.TotalPriceExcVat),
                    TotalPriceIncVat = model.QuoteSummaries.ToList().Sum(x => x.TotalPriceIncVat),
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
                        TotalPriceIncVat = summary.TotalPriceIncVat,
                        Description = summary.Description,
                    };

                    await _quoteSummaryRepository.InsertAsync(quoteSummary);
                    entity.QuoteSummaries.Add(quoteSummary);
                }

                QuoteHistory historyEntity = new QuoteHistory
                {
                    QuoteId = entity.Id,
                    QuoteStatusId = entity.QuoteStatusId,
                };
                await _quoteHistoryRepository.InsertAsync(historyEntity);
                   

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
                var eventStatus = (await _quoteStatusService.Search()).Result;

                var draftId = eventStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.DRAFT.ToString()).Id;
                var declinedId = eventStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.DECLINED.ToString()).Id;
                var entity = await _quoteRepository.FirstOrDefaultAsync(e => e.Id == id);
                if (entity == null)
                {
                    return ApiResult<QuoteResponseDto>.NotFound("Quote not found");
                }
                if (entity.QuoteStatusId == declinedId)
                {
                    entity.QuoteStatusId = draftId;
                }

                var existingSummaries = await _quoteSummaryRepository.QueryAsync(e => e.QuoteId == id);


                foreach (var summary in existingSummaries.ToList())
                {
                    await _quoteSummaryRepository.DeleteAsync(summary);
                }

                var newSummaries = new List<QuoteSummary>();
                foreach (var summary in model.QuoteSummaries)
                {
                    var quotesummaries = new QuoteSummary
                    {
                        QuoteId = entity.Id,
                        Name = summary.Name,
                        Quantity = summary.Quantity,
                        Vat = summary.Vat,
                        UnitPrice = summary.UnitPrice,
                        TotalPriceExcVat = summary.TotalPriceExcVat,
                        TotalPriceIncVat = summary.TotalPriceIncVat,
                        Description = summary.Description,
                    };
                    await _quoteSummaryRepository.InsertAsync(quotesummaries);
                    newSummaries.Add(quotesummaries);
                }

                QuoteHistory historyEntity = new QuoteHistory
                {
                    QuoteId = entity.Id,
                    QuoteStatusId = entity.QuoteStatusId,
                };
                await _quoteHistoryRepository.InsertAsync(historyEntity);
                entity.TotalPriceIncVat = newSummaries.Sum(x => x.TotalPriceIncVat);
                entity.TotalPriceExcVat = newSummaries.Sum(x => x.TotalPriceExcVat);

                _mapper.Map(model, entity);
                entity.QuoteSummaries = newSummaries;
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
                var eventStatus = (await _quoteStatusService.Search()).Result;
                var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.SEND_TO_CUSTOMER.ToString()).Id;
                model.Id = entity.Id;
                model.CustomerId = entity.CustomerId;
                model.ExpiryDate = entity.ExpiryDate;
                model.QuoteDate = entity.QuoteDate;
                model.CustomerAddress = entity.CustomerAddress;
                model.QuoteStatusId = entity.QuoteStatusId;
                model.QuoteNo = entity.QuoteNo;
                model.CompanyAddress = entity.CompanyAddress;
                model.CompanyBankAddress = entity.CompanyBankAddress;
                model.IsCustomerApprovalPending = entity.QuoteStatusId.ToString() == statusId.ToString();
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
            var now = DateTime.UtcNow;
            var currentYear = now.Year;
            var currentMonth = now.Month;
            var dateFormat = $"{currentYear}{currentMonth:D2}";

            var id = (await _quoteRepository.QueryAsync(x=>x.QuoteNo.ToString().StartsWith(dateFormat), ignoreGlobalFilter: true)).Count();
            

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

        public async Task<ApiResult<bool>> SendQuoteEmailToCustomer(long quoteId)
        {
            var eventStatus = (await _quoteStatusService.Search()).Result;
            var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.SEND_TO_CUSTOMER.ToString()).Id;
            var entity = (await _quoteRepository.QueryAsync(x => x.Id == quoteId, include: entities => entities.Include(e => e.Customer))).FirstOrDefault();
            entity.QuoteStatusId = statusId;

            QuoteHistory historyEntity = new QuoteHistory
            {
                QuoteId = entity.Id,
                QuoteStatusId = entity.QuoteStatusId,
            };
            await _quoteHistoryRepository.InsertAsync(historyEntity);
            await _quoteRepository.UpdateAsync(entity);
           return await SendQuoteToCustomer(entity.Customer.TechnicalContactEmail, entity, entity.Customer.TechnicalContactName, entity.Customer.Organizations);
        }

        private async Task<ApiResult<bool>> SendQuoteToCustomer(string email, Quote quote, string technicalContactName,
                                    string customer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogError(" Sender Email is missing.");
                    return ApiResult<bool>.Success(false);
                }
                var subject = $"Quote #{quote.QuoteNo} for {customer}.";
                var link = GetQuoteLinkForCustomer(quote.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.CustomerQuote);
                emailTemplate = emailTemplate.Replace("[TechnicalContactName]", technicalContactName)
                                             .Replace("[QuoteNumber]", quote.QuoteNo.ToString())
                                             .Replace("[QuoteDate]",LocaleHelper.FormatDate(quote.QuoteDate))
                                             .Replace("[ExpiryDate]", LocaleHelper.FormatDate(quote.ExpiryDate))
                                             .Replace("[LinkToQuote]", link)
                                             .Replace("[CustomerName]", customer)
                                             .Replace("[Month]", DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(quote.QuoteDate.Month))
                                             .Replace("[Year]", quote.QuoteDate.Year.ToString());
                var emailResult = await _emailService.SendEmail(email, subject, emailTemplate);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw;
            }
        }

        public async Task<ApiResult<InvoiceResponseDto>> ConvertInvoice(long quoteId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var quoteStatus = (await _quoteStatusService.Search()).Result;
                var quotestatusId = quoteStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.FINALIZED.ToString()).Id;
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.DRAFT.ToString()).Id;

                var paymentStatus = (await _paymentStatusService.Search()).Result;
                var paymentStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusesEnum.UNPAID.ToString()).Id;
                var quote = await _quoteRepository.FirstOrDefaultAsync(e => e.Id == quoteId, include: source => source.Include(x => x.QuoteSummaries));


                //var now = DateTime.UtcNow;
                //var currentYear = now.Year;
                //var currentMonth = now.Month;
                //var dateFormat = $"{currentYear}{currentMonth:D2}";

                //var maxId = (await _invoiceRepository.QueryAsync(x => x.InvoiceNumber.ToString().StartsWith(dateFormat), ignoreGlobalFilter: true)).Count();
                //var newId = maxId + 1;
                //var invoiceNumber = long.Parse($"{DateTime.UtcNow:yyyyMM}{newId}");

                var invoiceNumber = await GenerateDraftNumber();

                var id = await _invoiceRepository.MaxAsync(e => (int)e.Id, ignoreGlobalFilter: true);

                if (quote == null)
                {
                    return ApiResult<InvoiceResponseDto>.NotFound("Quote not found");
                }
                quote.QuoteStatusId = quotestatusId;

                var invoice = new Invoice
                {
                    Id = id+1,
                    InvoiceNumber = invoiceNumber,
                    CustomerId = quote.CustomerId,
                    InvoiceDate = DateTime.UtcNow,
                    CustomerAddress = quote.CustomerAddress,
                    PaymentStatusId = paymentStatusId,
                    InvoiceStatusId = statusId,
                    MetaData = Convert.ToString(quoteId),
                    VatValue = quote.QuoteSummaries.Sum(x => (x.UnitPrice * x.Quantity * x.Vat) / 100),
                    TotalPriceIncludingVat = quote.QuoteSummaries.Sum(x => x.TotalPriceIncVat),
                    DueAmount = quote.QuoteSummaries.Sum(x => x.TotalPriceIncVat),
                    TotalPriceExcludingVat = quote.QuoteSummaries.Sum(x => x.TotalPriceExcVat),
                    ExpiryDate = quote.ExpiryDate,
                    PaymentClearanceDays = (quote.ExpiryDate - DateTime.UtcNow).Days,
                    InvoiceDetails = new List<InvoiceDetail>()
                };

                foreach (var quoteSummary in quote.QuoteSummaries)
                {
                    var invoiceDetail = new InvoiceDetail
                    {
                        InvoiceId = invoice.Id,
                        ReferenceName = quoteSummary.Name,
                        Description = quoteSummary.Description,
                        Quantity = quoteSummary.Quantity,
                        UnitPrice = quoteSummary.UnitPrice,
                        VatPercent = quoteSummary.Vat,
                        TotalPriceExcludingVat = quoteSummary.TotalPriceExcVat,
                        TotalPriceIncludingVat = quoteSummary.TotalPriceIncVat,

                    };
                    invoice.InvoiceDetails.Add(invoiceDetail);
                }
                await _quoteRepository.UpdateAsync(quote);
                await _invoiceRepository.InsertAsync(invoice);
                var quoteHistory = new QuoteHistory
                {
                    QuoteId = quote.Id,
                    QuoteStatusId = quotestatusId,
                    CreatedAt = DateTime.UtcNow,
                    Comment = "Quote Converted to Invoice",
                };
                var historyEntity = new InvoiceHistory
                {
                    InvoiceId = invoice.Id,
                    InvoiceStatusId = invoice.InvoiceStatusId,
                    CreatedAt = DateTime.UtcNow,
                    Comment = "Quote Converted to Invoice",
                };
                await _quoteHistoryRepository.InsertAsync(quoteHistory);
                await _invoiceHistoryRepository.InsertAsync(historyEntity);
                await _unitOfWork.CommitTransactionAsync();

                var mappedInvoice = _mapper.Map<InvoiceResponseDto>(invoice);
                return ApiResult<InvoiceResponseDto>.Success(mappedInvoice);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogException(ex);
                throw;
            }
        }

        private async Task<string> GenerateDraftNumber()
        {
            try
            {
                var now = DateTime.UtcNow;
                var currentYear = now.Year;
                var currentMonth = now.Month;
                var dateFormat = $"{currentYear}{currentMonth:D2}";

                var draftInvoicesCount = (await _invoiceRepository.QueryAsync(x => x.InvoiceNumber.Contains(dateFormat), ignoreGlobalFilter: true)).Count();
                var newDraftNumber = draftInvoicesCount + 1;
                var draftInvoiceNumber = $"Draft-{DateTime.UtcNow:yyyyMM}{newDraftNumber}";

                return draftInvoiceNumber;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        private IQueryable<Quote> ApplyFilters(IQueryable<Quote> entities, PageQueryFiterBase filter)
        {
            filter.GetValue<string>("customerName", (v) =>
            {
                entities = entities.Where(e => e.Customer.Organizations == v);
            });

            filter.GetValue<string>("customerAddress", (v) =>
            {
                var searchTerms = v.Split(' ');
                foreach (var term in searchTerms)
                {
                    var lowerCaseTerm = term.ToLower();
                    entities = entities.Where(e => e.CustomerAddress != null && e.CustomerAddress.ToLower().Contains(lowerCaseTerm));
                }
            });

            filter.GetValue<DateTime>("quoteDate", (v) =>
            {
                entities = entities.Where(e => e.QuoteDate != null && e.QuoteDate < v);
            }, OperatorType.lessthan, true);
            filter.GetValue<DateTime>("quoteDate", (v) =>
            {
                entities = entities.Where(e => e.QuoteDate != null && e.QuoteDate > v);
            }, OperatorType.greaterthan, true);


            filter.GetValue<DateTime>("expiryDate", (v) =>
        {
            entities = entities.Where(e => e.ExpiryDate != null && e.ExpiryDate < v);
        }, OperatorType.lessthan, true);
            filter.GetValue<DateTime>("expiryDate", (v) =>
            {
                entities = entities.Where(e => e.ExpiryDate != null && e.ExpiryDate > v);
            }, OperatorType.greaterthan, true);

            filter.GetValue<string>("quoteStatusId", (v) =>
            {
                entities = entities.Where(e => e.QuoteStatusId.ToString() == v);
            });

            filter.GetValue<string>("quoteStatusesName", (v) =>
            {
                var lowerV = v.ToLower();
                entities = entities.Where(e => e.QuoteStatuses.Name.ToLower().Contains(lowerV));
            });

            return entities;
        }

        private IQueryable<Quote> ApplySorting(IQueryable<Quote> orders, SortModel sort)
        {
            try
            {
                if (sort?.Name == null)
                {
                    orders = orders.OrderByDescending(o => o.CreatedAt);
                    return orders;
                }
                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.CustomerName).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Customer.Organizations);
                    }
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.CustomerAddress).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.CustomerAddress);
                    }
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.QuoteStatusesName).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.QuoteStatuses.Name);
                    }
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.ExpiryDate).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.ExpiryDate);
                    }
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.QuoteDate).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.QuoteDate);
                    }

                }
                else
                {
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.CustomerName).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.Customer.Organizations);
                    }
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.CustomerAddress).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.CustomerAddress);
                    }
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.QuoteStatusesName).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.QuoteStatuses.Name);
                    }
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.ExpiryDate).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.ExpiryDate);
                    }
                    if (columnName.ToUpper() == nameof(QuoteResponseDto.QuoteDate).ToUpper())
                    {
                        orders = orders.OrderBy(o => o.QuoteDate);
                    }
                }
                return orders;

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return orders;
            }
        }


        public async Task<ApiResult<bool>> QuoteAction(QuoteActionDto model)
        {
            try
            {
                var eventDetails = await _quoteRepository.FirstOrDefaultAsync(x => x.Id == model.Id);
                var eventStatus = (await _quoteStatusService.Search()).Result;
                var customerApprovedId = eventStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.ACCEPTED.ToString()).Id;
                var customerDeclinedId = eventStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.DECLINED.ToString()).Id;

                if (model.IsApproved)
                {
                    eventDetails.QuoteStatusId = customerApprovedId;
                }
                else
                {
                    eventDetails.QuoteStatusId = customerDeclinedId;
                }
                var result = await _quoteRepository.UpdateAsync(eventDetails);

                QuoteHistory historyEntity = new QuoteHistory()
                {
                    QuoteId = eventDetails.Id,
                    QuoteStatusId = eventDetails.QuoteStatusId,
                    Comment = model.Comment,
                    CreatedAt = DateTime.UtcNow,
                };
                await _quoteHistoryRepository.InsertAsync(historyEntity);

                var entities = (await _quoteRepository.QueryAsync(x => x.Id == result.Id, include: entities => entities.Include(e => e.Customer))).FirstOrDefault();
                var customer = (await _customersRepository.FirstOrDefaultAsync(x => x.Id == result.CustomerId));
                var accountAdminResult = await _applicationUserService.GetAccountAdmins();
                var sendSuccess = true;
                foreach (var accountAdmin in accountAdminResult.Result)
                {
                    var success = await SendTimesheetActionEmailToAccountAdmin(accountAdmin,customer, model.IsApproved, model.Comment,entities.QuoteNo,entities.QuoteDate, entities.ExpiryDate, entities);
                    if (!success)
                    {
                        sendSuccess = false;
                    }
                }
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<QuoteHistoryDto>>> GetQuoteHistoryById(int id)
        {
            try
            {

                var entities = await _quoteHistoryRepository.QueryAsync(e => e.QuoteId == id, disableTracking: true);
                var mappedEntities = entities.OrderByDescending(e => e.CreatedAt).Select(e => new QuoteHistoryDto
                {
                    Id = e.Id,
                    QuoteId = e.QuoteId,
                    QuoteStatusId = e.QuoteStatusId,
                    QuoteStatusName = e.QuoteStatuses.Name,
                    Comment = e.Comment,
                    CreatedAt = e.CreatedAt,
                    QuoteNumber=e.Quote.QuoteNo,
                }).ToList();

                return ApiResult<List<QuoteHistoryDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        private async Task<bool> SendTimesheetActionEmailToAccountAdmin(ApplicationUserDto accountAdmin, Customer customer, bool isApprovedEmail, string comment, long quoteNo, DateTime quoteDate, DateTime expiryDate, Quote quote)
        {
            try
            {
                var subject = $"Quote #{quoteNo} for {customer.Organizations} is {(isApprovedEmail ? "Approved" : "Declined")}.";
                var link = GetQuoteLinkForCustomer(quote.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.QuoteActions);
                emailTemplate = emailTemplate.Replace("[AccountAdminName]", accountAdmin.FullName)
                                             .Replace("[TechnicalContactName]", customer.TechnicalContactName)
                                             .Replace("[QuoteNumber]", quoteNo.ToString())
                                             .Replace("[QuoteDate]", LocaleHelper.FormatDate(quoteDate))
                                             .Replace("[ExpiryDate]", LocaleHelper.FormatDate(expiryDate))
                                             .Replace("[Customer]",customer.Organizations)
                                             .Replace("[LinkToQuote]", link)
                                             .Replace("[TimesheetAction]", isApprovedEmail ? "approved" : "declined")
                                             .Replace("[Comment]", comment);
                return await _emailService.SendEmail(accountAdmin.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }
    }
}
