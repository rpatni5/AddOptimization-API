using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Data.Repositories;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;


namespace AddOptimization.Services.Services
{
    public class ExternalInvoiceService : IExternalInvoiceService
    {
        private readonly IGenericRepository<ExternalInvoice> _externalInvoiceRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<ExternalInvoiceDetail> _invoiceDetailRepository;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly CustomDataProtectionService _protectionService;
        private readonly IInvoiceStatusService _invoiceStatusService;
        private readonly IGenericRepository<Customer> _customer;
        private readonly IGenericRepository<CustomerEmployeeAssociation> _customerEmployeeAssociation;
        private readonly IGenericRepository<SchedulerEvent> _schedulersRepository;
        private readonly IGenericRepository<SchedulerEventDetails> _schedulersDetailsRepository;
        private readonly ISchedulersStatusService _schedulersStatusService;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly ISchedulerEventTypeService _schedulerEventTypeService;
        private readonly IGenericRepository<PublicHoliday> _publicHolidayRepository;
        private readonly IGenericRepository<Employee> _employeeRepository;
        private readonly IGenericRepository<Company> _companyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExternalInvoiceService> _logger;
        private readonly IMapper _mapper;

        public ExternalInvoiceService(IGenericRepository<ExternalInvoice> externalInvoiceRepository,
            IGenericRepository<Customer> customer,
            IGenericRepository<CustomerEmployeeAssociation> customerEmployeeAssociation,
            IGenericRepository<SchedulerEvent> schedulersRepository,
            IGenericRepository<SchedulerEventDetails> schedulersDetailsRepository,
            ISchedulersStatusService schedulersStatusService,
             IInvoiceStatusService invoiceStatusService,
              IHttpContextAccessor httpContextAccessor,
               IPaymentStatusService paymentStatusService,
               IEmailService emailService,
               ITemplateService templateService,
                IConfiguration configuration,             ISchedulerEventTypeService schedulerEventTypeService,
            IGenericRepository<PublicHoliday> publicHolidayRepository,
            IGenericRepository<ExternalInvoiceDetail> invoiceDetailRepository,
            IGenericRepository<Employee> employeeRepository,
            IGenericRepository<Company> companyRepository,
              IMapper mapper,
              IUnitOfWork unitOfWork,
        ILogger<ExternalInvoiceService> logger)
        {
            _companyRepository = companyRepository;
            _publicHolidayRepository = publicHolidayRepository;
            _externalInvoiceRepository = externalInvoiceRepository;
            _invoiceStatusService = invoiceStatusService;
            _httpContextAccessor = httpContextAccessor;
            _paymentStatusService = paymentStatusService;
            _customerEmployeeAssociation = customerEmployeeAssociation;
            _customer = customer;
            _emailService= emailService;
            _templateService= templateService;
            _schedulersRepository = schedulersRepository;
            _schedulerEventTypeService = schedulerEventTypeService;
            _schedulersStatusService = schedulersStatusService;
            _schedulersDetailsRepository = schedulersDetailsRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResult<List<ExternalInvoiceResponseDto>>> GenerateExternalInvoice(Guid customerId, MonthDateRange month,
        List<CustomerEmployeeAssociationDto> associatedEmployees)
        {
            try
            {
                var events = (await _schedulersRepository.QueryAsync(x => x.CustomerId == customerId
                && x.StartDate.Month == month.StartDate.Month
                && x.StartDate.Year == month.StartDate.Year)).ToList();
                var eventTypes = (await _schedulerEventTypeService.Search()).Result;
                var timesheetEventId = eventTypes.FirstOrDefault(x => x.Name.Equals("timesheet", StringComparison.InvariantCultureIgnoreCase)).Id;
                var overtimeEventId = eventTypes.FirstOrDefault(x => x.Name.Equals("overtime", StringComparison.InvariantCultureIgnoreCase)).Id;
                var customer = await _customer.FirstOrDefaultAsync(t => t.Id == customerId, ignoreGlobalFilter: true);
                var publicHolidays = (await _publicHolidayRepository.QueryAsync(o => o.CountryId == customer.CountryId
                    && o.Date.Month == month.StartDate.Month
                    && o.Date.Year == month.StartDate.Year)).Select(x => x.Date.Date);


                //Address Calculations

                var company = await _companyRepository.FirstOrDefaultAsync(ignoreGlobalFilter: true);

                var companyAddress = string.Empty;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(company.CompanyName);
                sb.AppendLine(company.Address);
                sb.AppendLine(company.City);
                sb.AppendLine(company.State);
                sb.AppendLine(company.ZipCode.ToString());
                //sb.AppendLine(company.Company);
                companyAddress = sb.ToString();

                var companyBankDetails = string.Empty;

                StringBuilder bd = new StringBuilder();
                bd.AppendLine(company.BankName);
                bd.AppendLine(company.BankAccountName);
                bd.AppendLine(company.BankAccountNumber);
                bd.AppendLine(company.BankAddress);
                companyAddress = bd.ToString();

                var invoice = new ExternalInvoice
                {
                    //CustomerAddress = customerAddress,
                    CompanyAddress = companyAddress,
                    CompanyBankDetails = companyBankDetails,
                    //DueDate = customer.PaymentClearanceDays.HasValue ? DateTime.UtcNow.AddDays(customer.PaymentClearanceDays.Value) : DateTime.UtcNow.AddDays(15),
                    //InvoiceDate = DateTime.UtcNow,
                    //InvoiceNumber = "",
                    InvoiceStatusId = Guid.Empty,
                    PaymentStatusId = Guid.Empty,
                    TotalPriceExcludingVat = 0,
                    TotalPriceIncludingVat = 1,
                    //Vat = 1,
                };

                var invoiceResult = await _externalInvoiceRepository.InsertAsync(invoice);
                foreach (var employee in associatedEmployees)
                {
                    var daily = employee.DailyWeightage;
                    var overTime = employee.Overtime;
                    var publicHoliday = employee.PublicHoliday;
                    var saturday = employee.Saturday;
                    var sunday = employee.Sunday;

                    var empl = (await _employeeRepository.QueryAsync(e => e.UserId == employee.EmployeeId,
                        include: empl => empl.Include(x => x.ApplicationUser),
                        ignoreGlobalFilter: true)).FirstOrDefault();
                    var employeeEvent = events.FirstOrDefault(x => x.UserId == employee.EmployeeId);
                    var employeeEventDetails = (await _schedulersDetailsRepository.QueryAsync(c => c.SchedulerEventId == employeeEvent.Id && !c.IsDeleted)).ToList();

                    var description = string.Empty;
                    var jobTitle = "";
                    decimal unitPrice;
                    //Normal day timesheet Mon-Fri
                    var monFriTimesheetList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsWeekday(c.Date.Value) && !publicHolidays.Contains(c.Date.Value.Date) && c.EventTypeId == timesheetEventId).ToList();
                    description = empl.ApplicationUser.FullName + '-' + "job title";
                    CalculateAndSaveInvoiceDetails(invoiceResult, monFriTimesheetList, daily, empl, customer.VAT ?? 0, description);

                    //Sat timesheet including overtime
                    var saturdayTimesheetList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsSaturday(c.Date.Value)).ToList();
                    unitPrice = daily / 8 * saturday / 100;
                    description = $"{empl.ApplicationUser.FullName}-{jobTitle}-WE (Saturday) {saturday}% ({daily / 8} eur/h)";   // WE (Sunday) 210% (71,88 eur/h)
                    CalculateInvoiceDetailsForWeekend(invoiceResult, saturdayTimesheetList, unitPrice, empl, customer.VAT ?? 0, description, timesheetEventId, overtimeEventId);

                    //Sun timesheet including overtime
                    unitPrice = daily / 8 * sunday / 100;
                    description = $"{empl.ApplicationUser.FullName}-{jobTitle}-WE (Sunday) {sunday}% ({daily / 8} eur/h)";   // WE (Sunday) 210% (71,88 eur/h)
                    var sundayTimesheetList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsSunday(c.Date.Value)).ToList();
                    CalculateInvoiceDetailsForWeekend(invoiceResult, sundayTimesheetList, unitPrice, empl, customer.VAT ?? 0, description, timesheetEventId, overtimeEventId);

                    //Overtime Mon-Fri
                    unitPrice = daily / 8 * overTime / 100;
                    description = $"{empl.ApplicationUser.FullName}-{jobTitle}-Overtime {overTime}% ({daily / 8} eur/h)";
                    var overtimeList = employeeEventDetails.Where(c => c.EventTypeId == overtimeEventId && MonthDateRangeHelper.IsWeekday(c.Date.Value)).ToList();
                    CalculateAndSaveInvoiceDetails(invoiceResult, overtimeList, unitPrice, empl, customer.VAT ?? 0, description);

                    //Timesheet Mon-Fri on public holiday
                    unitPrice = daily * publicHoliday / 100;
                    description = $"{empl.ApplicationUser.FullName}-{jobTitle}-Holiday {publicHoliday}% ({daily} eur/d)";
                    var publicHolidaysList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsWeekday(c.Date.Value) && publicHolidays.Contains(c.Date.Value.Date) && c.EventTypeId == timesheetEventId).ToList();
                    CalculateAndSaveInvoiceDetails(invoiceResult, publicHolidaysList, unitPrice, empl, customer.VAT ?? 0, description);

                }
                return ApiResult<List<ExternalInvoiceResponseDto>>.Success("");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        private void CalculateAndSaveInvoiceDetails(ExternalInvoice invoice, List<SchedulerEventDetails> schedulerEventDetails, decimal daily, Employee empl, decimal vat, string description)
        {
            var quantity = schedulerEventDetails.Sum(c => c.Duration);
            var invoiceDetail = new ExternalInvoiceDetail
            {
                ExternalInvoiceId = invoice.Id,
                Description = description,
                Quantity = quantity,
                UnitPrice = daily,
                TotalPriceExcludingVat = daily * quantity,
                TotalPriceIncludingVat = (daily * quantity) + (daily * quantity * vat / 100),
                VatPercent = vat,
            };
            var invoiceDetails = _invoiceDetailRepository.InsertAsync(invoiceDetail);
        }

        private void CalculateInvoiceDetailsForWeekend(ExternalInvoice invoice, List<SchedulerEventDetails> schedulerEventDetails, decimal unitPrice, Employee empl, decimal vat, string description, Guid timesheetEventId, Guid overtimeEventId)
        {
            var timesheetQuantityInHr = schedulerEventDetails.Where(x => x.SchedulerEventId == timesheetEventId).Sum(c => c.Duration) * 8;
            var overtimeQuantityInHr = schedulerEventDetails.Where(x => x.SchedulerEventId == overtimeEventId).Sum(c => c.Duration);

            var quantity = timesheetQuantityInHr + overtimeQuantityInHr;

            var invoiceDetail = new ExternalInvoiceDetail
            {
                ExternalInvoiceId = invoice.Id,
                Description = description,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPriceExcludingVat = unitPrice * quantity,
                TotalPriceIncludingVat = (unitPrice * quantity) + (unitPrice * quantity * vat / 100),
                VatPercent = vat,
            };
            var invoiceDetails = _invoiceDetailRepository.InsertAsync(invoiceDetail);

        }
        public async Task<ApiResult<ExternalInvoiceResponseDto>> Create(ExternalInvoiceRequestDto model)
        {
            try
            {
                int userId = model.EmployeeId != null ? model.EmployeeId.Value : _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                await _unitOfWork.BeginTransactionAsync();
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.DRAFT.ToString()).Id;

                var paymentStatus = (await _paymentStatusService.Search()).Result;
                var paymentStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusesEnum.UNPAID.ToString()).Id;
                var maxId = await _externalInvoiceRepository.MaxAsync<Int64>(e => e.Id, ignoreGlobalFilter: true);
                var newId = maxId + 1;
                var invoiceNumber = $"{DateTime.UtcNow:yyyyMM}{newId}";

                ExternalInvoice entity = new ExternalInvoice
                {
                    Id= newId,
                    InvoiceNumber = Convert.ToInt64(invoiceNumber),
                    PaymentStatusId = paymentStatusId,
                    VatValue = model.ExternalInvoiceDetails.Sum(x => (x.UnitPrice * x.Quantity * x.VatPercent) / 100),
                    TotalPriceIncludingVat = model.ExternalInvoiceDetails.Sum(x => x.TotalPriceIncludingVat),
                    TotalPriceExcludingVat = model.ExternalInvoiceDetails.Sum(x => x.TotalPriceExcludingVat),
                    CompanyId = model.CompanyId,
                    CompanyName = model.CompanyName,
                    CompanyAddress = model.CompanyAddress,
                    EmployeeId = userId,
                    ExpiryDate = model.ExpiryDate,
                    InvoiceDate = model.InvoiceDate,
                    InvoiceStatusId = statusId,
                    PaymentClearanceDays = model.PaymentClearanceDays,

                };
                await _externalInvoiceRepository.InsertAsync(entity);
                foreach (var detail in model.ExternalInvoiceDetails)
                {
                    var invoiceDetail = new ExternalInvoiceDetail
                    {
                        ExternalInvoiceId = entity.Id,
                        Description = detail.Description,
                        Quantity = detail.Quantity,
                        VatPercent = detail.VatPercent,
                        UnitPrice = detail.UnitPrice,
                        TotalPriceExcludingVat = detail.TotalPriceExcludingVat,
                        TotalPriceIncludingVat = detail.TotalPriceIncludingVat

                    };

                    await _invoiceDetailRepository.InsertAsync(invoiceDetail);
                    entity.InvoiceDetails.Add(invoiceDetail);
                }
                await _unitOfWork.CommitTransactionAsync();
                var mappedEntity = _mapper.Map<ExternalInvoiceResponseDto>(entity);
                return ApiResult<ExternalInvoiceResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<List<ExternalInvoiceResponseDto>>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _externalInvoiceRepository.QueryAsync((e => !e.IsDeleted), include: source => source.Include(x => x.Company).Include(x => x.InvoiceStatus).Include(x => x.PaymentStatus).Include(x=>x.ApplicationUser), ignoreGlobalFilter: true);

                var result = entities.ToList();
                var mappedEntities = _mapper.Map<List<ExternalInvoiceResponseDto>>(result);

                return ApiResult<List<ExternalInvoiceResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<ExternalInvoiceResponseDto>> FetchInvoiceDetails(long id)
        {
            try
            {
                var model = new ExternalInvoiceResponseDto();
                var entity = await _externalInvoiceRepository.FirstOrDefaultAsync(e => e.Id == id, ignoreGlobalFilter: true);
                model.Id = entity.Id;
                model.CompanyId = entity.CompanyId;
                model.CompanyName = entity.CompanyName;
                model.ExpiryDate = entity.ExpiryDate;
                model.InvoiceDate = entity.InvoiceDate;
                model.CompanyAddress = entity.CompanyAddress;
                model.InvoiceStatusId = entity.InvoiceStatusId;
                model.PaymentStatusId = entity.PaymentStatusId;
                model.CompanyAddress = entity.CompanyAddress;
                model.InvoiceNumber = entity.InvoiceNumber;
                //model.EmployeeId = entity.EmployeeId;

                var quoteSummary = (await _invoiceDetailRepository.QueryAsync(e => e.ExternalInvoiceId == id, disableTracking: true)).ToList();
                model.ExternalInvoiceDetails = _mapper.Map<List<ExternalInvoiceDetailDto>>(quoteSummary);
                return ApiResult<ExternalInvoiceResponseDto>.Success(model);

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<ExternalInvoiceResponseDto>> Update(long id, ExternalInvoiceRequestDto model)
        {
            try
            {
                var isExists = await _externalInvoiceRepository.IsExist(e => e.Id != id);
                var entity = await _externalInvoiceRepository.FirstOrDefaultAsync(e => e.Id == id);
                var details = await _invoiceDetailRepository.QueryAsync(e => e.ExternalInvoiceId == id);

                foreach (var detail in details.ToList())
                {
                    await _invoiceDetailRepository.DeleteAsync(detail);
                }
                if (entity == null)
                {
                    return ApiResult<ExternalInvoiceResponseDto>.NotFound("External Invoice");
                }
                foreach (var detail in model.ExternalInvoiceDetails)
                {
                    var externalInvoiceDetail = new ExternalInvoiceDetail
                    {
                        ExternalInvoiceId = entity.Id,
                        Description = detail.Description,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        VatPercent = detail.VatPercent,
                        TotalPriceIncludingVat = detail.TotalPriceIncludingVat,
                        TotalPriceExcludingVat = detail.TotalPriceExcludingVat,
                    };
                    await _invoiceDetailRepository.InsertAsync(externalInvoiceDetail);
                }

                _mapper.Map(model, entity);
                await _externalInvoiceRepository.UpdateAsync(entity);
                var mappedEntity = _mapper.Map<ExternalInvoiceResponseDto>(entity);
                return ApiResult<ExternalInvoiceResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public string GetInvoiceLinkForAccountAdmin(long Id)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            var encryptedId = _protectionService.Encode(Id.ToString());
            return $"{baseUrl}quote/approval/{encryptedId}";
        }

        public async Task<bool> SendInvoiceApprovalEmailToAccountAdmin(long Id)
        {
            var entity = (await _externalInvoiceRepository.QueryAsync(x => x.Id == Id, include: entities => entities.Include(e => e.ApplicationUser))).FirstOrDefault();
            return await SendInvoiceToAccountAdmin(entity.ApplicationUser.Email, entity, entity.ApplicationUser.Email, entity.ApplicationUser.FullName);
        }

        private async Task<bool> SendInvoiceToAccountAdmin(string email, ExternalInvoice invoice, string managerName,
                                string employee)
        {
            try
            {
                var subject = "External Invoice";
                var link = GetInvoiceLinkForAccountAdmin(invoice.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.SendInvoice);
                emailTemplate = emailTemplate.Replace("[EmployeeName]", employee)
                                             .Replace("[MangerName]", managerName)
                                             .Replace("[LinkToInvoice]", link)
                                             .Replace("[Month]", DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(invoice.InvoiceDate.Month))
                                             .Replace("[Year]", invoice.InvoiceDate.Year.ToString());
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
    

