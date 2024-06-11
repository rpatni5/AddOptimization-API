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

        public QuoteService(IGenericRepository<Quote> quoteRepository, ILogger<QuoteService> logger, IMapper mapper, IQuoteStatusService quoteStatusService, IGenericRepository<QuoteSummary> quoteSumamryRepository, IUnitOfWork unitOfWork)
        {
            _quoteRepository = quoteRepository;
            _logger = logger;
            _mapper = mapper;
            _quoteStatusService = quoteStatusService;
            _quoteSummaryRepository = quoteSumamryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResult<List<QuoteResponseDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _quoteRepository.QueryAsync(include: source => source.Include(x => x.Customer).Include(x => x.Product).Include(x => x.QuoteStatuses), ignoreGlobalFilter: true);

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

        public async Task<ApiResult<QuoteResponseDto>> Update(Guid id, QuoteRequestDto model)
        {
            try
            {
                var isExists = await _quoteRepository.IsExist(e => e.Id != id);
                var entity = await _quoteRepository.FirstOrDefaultAsync(e => e.Id == id);
                var summaries = await _quoteSummaryRepository.QueryAsync(e  => e.QuoteId == id);
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
                    entity.QuoteSummaries.Add(quoteSummary);
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

        public async Task<ApiResult<QuoteResponseDto>> FetchQuoteDetails(Guid id)
        {
            try
            {
                var model = new QuoteResponseDto();
                var entity = await _quoteRepository.FirstOrDefaultAsync(e => e.Id == id, ignoreGlobalFilter: true);
                model.Id = entity.Id;
                model.CustomerId = entity.CustomerId;
                model.ProductId = entity.ProductId;
                model.ExpiryDate = entity.ExpiryDate;
                model.QuoteDate = entity.QuoteDate;
                model.BillingAddress = entity.BillingAddress;
                model.QuoteStatusId = entity.QuoteStatusId;

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

    }
}
