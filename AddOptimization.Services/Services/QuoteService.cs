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

namespace AddOptimization.Services.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly IGenericRepository<Quote> _quoteRepository;
        private readonly IGenericRepository<QuoteSummary> _quoteSumamryRepository;
        private readonly ILogger<QuoteService> _logger;
        private readonly IMapper _mapper;
        private readonly IQuoteStatusService _quoteStatusService;
        private readonly IQuoteSummaryService _quoteSummaryService;

        public QuoteService(IGenericRepository<Quote> quoteRepository, ILogger<QuoteService> logger, IMapper mapper, IQuoteStatusService quoteStatusService, IGenericRepository<QuoteSummary> quoteSumamryRepository)
        {
            _quoteRepository = quoteRepository;
            _logger = logger;
            _mapper = mapper;
            _quoteStatusService = quoteStatusService;
            _quoteSumamryRepository = quoteSumamryRepository;
        }

        public async Task<ApiResult<QuoteResponseDto>> Create(QuoteRequestDto model)
        {
            try
            {
                var eventStatus = (await _quoteStatusService.Search()).Result;
                var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == QuoteStatusesEnum.DRAFT.ToString()).Id;

                Quote entity = new Quote
                {
                    CustomerId = model.CustomerId,
                    ProductId = model.ProductId,
                    ExpiryDate = model.ExpiryDate,
                    QuoteDate = model.QuoteDate,
                    BillingAddress = model.BillingAddress,
                    QuoteStatusId = statusId,
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

                    await _quoteSumamryRepository.InsertAsync(quoteSummary);
                    entity.QuoteSummaries.Add(quoteSummary);
                }

                var mappedEntity = _mapper.Map<QuoteResponseDto>(entity);
                return ApiResult<QuoteResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

    }
}
