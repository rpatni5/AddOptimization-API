using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Data.Repositories;
using AddOptimization.Services.Constants;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Enums;
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
using Microsoft.VisualBasic;
using System.Globalization;
using System.Text;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;


namespace AddOptimization.Services.Services
{
    public class ExternalInvoiceService : IExternalInvoiceService
    {
        private readonly IGenericRepository<ExternalInvoice> _externalInvoiceRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<ExternalInvoiceDetail> _invoiceDetailRepository;
        private readonly IGenericRepository<ExternalInvoicePaymentHistory> _externalInvoicePaymentRepository;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly CustomDataProtectionService _protectionService;
        private readonly IInvoiceStatusService _invoiceStatusService;
        private readonly IGenericRepository<Customer> _customer;
        private readonly IGenericRepository<CustomerEmployeeAssociation> _customerEmployeeAssociation;
        private readonly IGenericRepository<SchedulerEvent> _schedulersRepository;
        private readonly IGenericRepository<SchedulerEventDetails> _schedulersDetailsRepository;
        private readonly List<string> _currentUserRoles;
        private readonly ISchedulersStatusService _schedulersStatusService;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly ISchedulerEventTypeService _schedulerEventTypeService;
        private readonly IGenericRepository<PublicHoliday> _publicHolidayRepository;
        private readonly IGenericRepository<Employee> _employeeRepository;
        private readonly IGenericRepository<Company> _companyRepository;
        private readonly IGenericRepository<ExternalInvoiceHistory> _externalInvoiceHistoryRepository;
        private readonly IApplicationUserService _applicationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExternalInvoiceService> _logger;
        private readonly IMapper _mapper;

        public ExternalInvoiceService(IGenericRepository<ExternalInvoice> externalInvoiceRepository,
            IGenericRepository<Customer> customer,
            IGenericRepository<CustomerEmployeeAssociation> customerEmployeeAssociation,
            IGenericRepository<ExternalInvoicePaymentHistory> externalInvoicePaymentRepository,
            IGenericRepository<SchedulerEvent> schedulersRepository,
            IGenericRepository<SchedulerEventDetails> schedulersDetailsRepository,
            ISchedulersStatusService schedulersStatusService,
             IInvoiceStatusService invoiceStatusService,
              IHttpContextAccessor httpContextAccessor,
               IPaymentStatusService paymentStatusService,
                CustomDataProtectionService protectionService,
               IEmailService emailService,
               ITemplateService templateService,
                IConfiguration configuration, ISchedulerEventTypeService schedulerEventTypeService,
            IGenericRepository<PublicHoliday> publicHolidayRepository,
            IGenericRepository<ExternalInvoiceDetail> invoiceDetailRepository,
            IGenericRepository<Employee> employeeRepository,
            IGenericRepository<Company> companyRepository,
                IGenericRepository<ExternalInvoiceHistory> externalInvoiceHistoryRepository,
               IApplicationUserService applicationService,
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
            _configuration = configuration;
            _protectionService = protectionService;
            _emailService = emailService;
            _templateService = templateService;
            _schedulersRepository = schedulersRepository;
            _schedulerEventTypeService = schedulerEventTypeService;
            _schedulersStatusService = schedulersStatusService;
            _schedulersDetailsRepository = schedulersDetailsRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _employeeRepository = employeeRepository;
            _externalInvoiceHistoryRepository = externalInvoiceHistoryRepository;
            _applicationService = applicationService;
            _externalInvoicePaymentRepository= externalInvoicePaymentRepository;
            _currentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
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

                var now = DateTime.UtcNow;
                var currentYear = now.Year;
                var currentMonth = now.Month;
                var dateFormat = $"{currentYear}{currentMonth:D2}";
                var maxId = (await _externalInvoiceRepository.QueryAsync(x => x.InvoiceNumber.ToString().StartsWith(dateFormat), ignoreGlobalFilter: true)).Count();
                var newId = maxId + 1;
                var invoiceNumber = long.Parse($"{DateTime.UtcNow:yyyyMM}{newId}");
                var id = await _externalInvoiceRepository.MaxAsync(e => (int)e.Id, ignoreGlobalFilter: true);

                ExternalInvoice entity = new ExternalInvoice
                {
                    Id = id + 1,
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
                entity.DueAmount = entity.TotalPriceIncludingVat;

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

                ExternalInvoiceHistory historyEntity = new ExternalInvoiceHistory
                {
                    InvoiceId = entity.Id,
                    InvoiceStatusId = entity.InvoiceStatusId,
                };
                await _externalInvoiceHistoryRepository.InsertAsync(historyEntity);

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

        public async Task<PagedApiResult<ExternalInvoiceResponseDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _externalInvoiceRepository.QueryAsync((e => !e.IsDeleted), include: source => source.Include(x => x.Company).Include(x => x.InvoiceStatus).Include(x => x.PaymentStatus).Include(x => x.ApplicationUser), ignoreGlobalFilter: true);
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);
                var pagedResult = PageHelper<ExternalInvoice, ExternalInvoiceResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new ExternalInvoiceResponseDto
                {
                    Id = e.Id,
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.ApplicationUser.FullName,
                    CompanyName = e.CompanyName,
                    ExpiryDate = e.ExpiryDate,
                    InvoiceDate = e.InvoiceDate,
                    CompanyAddress = e.CompanyAddress,
                    InvoiceStatusId = e.InvoiceStatusId,
                    InvoiceStatusName = e.InvoiceStatus.Name,
                    PaymentStatusId = e.PaymentStatusId,
                    PaymentStatusName = e.PaymentStatus.Name,
                    InvoiceNumber = e.InvoiceNumber,
                    VatValue = e.VatValue,
                    TotalPriceExcludingVat = e.TotalPriceExcludingVat,
                    TotalPriceIncludingVat = e.TotalPriceIncludingVat,
                    DueAmount = e.DueAmount,

                }).ToList());

                var retVal = pagedResult;
                return PagedApiResult<ExternalInvoiceResponseDto>.Success(retVal);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<ExternalInvoiceResponseDto>> FetchExternalInvoiceDetails(long id, bool getRoleBasedData = true)
        {
            try
            {
                var model = new ExternalInvoiceResponseDto();
                var entity = await _externalInvoiceRepository.FirstOrDefaultAsync(e => e.Id == id, include: source => source.Include(x => x.InvoiceStatus).Include(x => x.PaymentStatus).Include(x => x.ApplicationUser), ignoreGlobalFilter: true);
                var employee = await _employeeRepository.FirstOrDefaultAsync(e => e.UserId == entity.EmployeeId, ignoreGlobalFilter: true);
                model.Id = entity.Id;
                model.CompanyId = entity.CompanyId;
                model.EmployeeId = entity.EmployeeId;
                model.CompanyName = entity.CompanyName;
                model.ExpiryDate = entity.ExpiryDate;
                model.InvoiceDate = entity.InvoiceDate;
                model.CompanyAddress = entity.CompanyAddress;
                model.CustomerAddress=entity.CustomerAddress;
                model.InvoiceStatusId = entity.InvoiceStatusId;
                model.InvoiceStatusName = entity.InvoiceStatus.Name;
                model.PaymentStatusId = entity.PaymentStatusId;
                model.PaymentStatusName = entity.PaymentStatus.Name;
                model.CompanyAddress = entity.CompanyAddress;
                model.InvoiceNumber = entity.InvoiceNumber;
                model.VatValue = entity.VatValue;
                model.TotalPriceExcludingVat = entity.TotalPriceExcludingVat;
                model.TotalPriceIncludingVat = entity.TotalPriceIncludingVat;
                model.ExternalCompanyName = employee?.CompanyName;
                model.ExternalCompanyAddress = employee?.ExternalAddress;
                model.ExternalCompanyCity = employee?.ExternalCity;
                model.ExternalCompanyState = employee?.ExternalState;
                model.ExternalCompanyZipCode = employee?.ExternalZipCode;
                model.BankName=employee?.BankName;
                model.BankAddress=employee?.BankAddress;
                model.BankCity=employee?.BankCity;
                model.BankState=employee?.BankState;
                model.BankAccountName=employee?.BankAccountName;
                model.BankZipCode=employee?.BankPostalCode;
                model.ExternalEmployeeAddress = employee?.Address;
                model.ExternalCity=employee?.City;
                model.ExternalState=employee?.State;
                model.ExternalZipCode=employee?.ZipCode;
                model.BankAccountNumber = employee?.BankAccountNumber;
                model.EmployeeName = entity.ApplicationUser.FullName;
                model.VATNumber = employee?.VATNumber;
                model.DueAmount=entity.DueAmount;

                var externalInvoiceSummary = (await _invoiceDetailRepository.QueryAsync(e => e.ExternalInvoiceId == id, disableTracking: true)).ToList();
                model.ExternalInvoiceDetails = _mapper.Map<List<ExternalInvoiceDetailDto>>(externalInvoiceSummary);
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
                var entity = await _externalInvoiceRepository.FirstOrDefaultAsync(e => e.Id == id);
                if (entity == null)
                {
                    return ApiResult<ExternalInvoiceResponseDto>.NotFound("External Invoice");
                }

                var existingDetails = await _invoiceDetailRepository.QueryAsync(e => e.ExternalInvoiceId == id);
                foreach (var detail in existingDetails.ToList())
                {
                    await _invoiceDetailRepository.DeleteAsync(detail);
                }

                var newInvoiceDetails = new List<ExternalInvoiceDetail>();
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
                    newInvoiceDetails.Add(externalInvoiceDetail);
                }
                ExternalInvoiceHistory historyEntity = new ExternalInvoiceHistory
                {
                    InvoiceId = entity.Id,
                    InvoiceStatusId = entity.InvoiceStatusId,
                };
                await _externalInvoiceHistoryRepository.InsertAsync(historyEntity);

                entity.VatValue = newInvoiceDetails.Sum(x => (x.UnitPrice * x.Quantity * x.VatPercent) / 100);
                entity.TotalPriceIncludingVat = newInvoiceDetails.Sum(x => x.TotalPriceIncludingVat);
                entity.TotalPriceExcludingVat = newInvoiceDetails.Sum(x => x.TotalPriceExcludingVat);

                var existingPayments = await _externalInvoicePaymentRepository.QueryAsync(e => e.InvoiceId == id);
                var totalPayments = existingPayments.Sum(x => x.Amount);
                var totalPaidAmount = totalPayments; 

                entity.DueAmount = entity.TotalPriceIncludingVat - totalPaidAmount;

                _mapper.Map(model, entity);
                entity.InvoiceDetails = newInvoiceDetails;
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
        public string GetInvoiceLinkForAccountAdmin(int externalInvoiceId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            return $"{baseUrl}external-Invoice/approval/{externalInvoiceId}";
        }
        public async Task<bool> SendInvoiceApprovalEmailToAccountAdmin(int Id)
        {

            var eventStatus = (await _invoiceStatusService.Search()).Result;
            var invoiceStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.SEND_TO_CUSTOMER.ToString()).Id;
            var paymentStatus = (await _paymentStatusService.Search()).Result;
            var paymentStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusesEnum.UNPAID.ToString()).Id;
            var externalInvoiceDetailsResponse = await FetchExternalInvoiceDetails(Id);
            var entity = (await _externalInvoiceRepository.QueryAsync(x => x.Id == Id, include: entities => entities.Include(e => e.ApplicationUser).Include(e => e.Company))).FirstOrDefault();
            var details = (await _invoiceDetailRepository.QueryAsync(x => x.ExternalInvoiceId == Id)).ToList();
            entity.PaymentStatusId = paymentStatusId;
            entity.InvoiceStatusId = invoiceStatusId;
            ExternalInvoiceHistory historyEntity = new ExternalInvoiceHistory
            {
                InvoiceId = entity.Id,
                InvoiceStatusId = entity.InvoiceStatusId,
            };
            await _externalInvoiceHistoryRepository.InsertAsync(historyEntity);
            await _externalInvoiceRepository.UpdateAsync(entity);
            var accountAdmins = (await _applicationService.GetAccountAdmins()).Result;
            var externalInvoiceDetail = externalInvoiceDetailsResponse.Result;


            return await SendInvoiceToAccountAdmin(accountAdmins, entity, entity.Company.CompanyName, entity.ApplicationUser.FullName, entity.InvoiceNumber, entity.TotalPriceIncludingVat , externalInvoiceDetail.ExternalCompanyName);
        }
        private async Task<bool> SendInvoiceToAccountAdmin(List<ApplicationUserDto> accountAdmins, ExternalInvoice invoice, string companyName, string employeeName, long invoiceNumber, decimal totalAmountDue ,string externalCompany
         )
        {
            try
            {
                var subject = $"External Invoice from {externalCompany} #{invoiceNumber}";
                var link = GetInvoiceLinkForAccountAdmin((int)invoice.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.RequestExternalInvoice);
                emailTemplate = emailTemplate.Replace("[CompanyName]",externalCompany)
                                             .Replace("[EmployeeName]", employeeName)
                                             .Replace("[LinkToOrder]", link)
                                             .Replace("[InvoiceNumber]", invoiceNumber.ToString())
                                             .Replace("[TotalAmountDue]", LocaleHelper.FormatCurrency(totalAmountDue));

                foreach (var admin in accountAdmins)
                {
                    var personalizedEmailTemplate = emailTemplate.Replace("[AccountAdmin]", admin.FullName);
                    await _emailService.SendEmail(admin.Email, subject, personalizedEmailTemplate);
                }

                return true;

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }
        private async Task<bool> SendInvoiceDeclinedEmailToEmployee(ApplicationUser employee, string companyName, string accountAdmin, long invoiceNumber, decimal totalAmountDue, string comment, string externalCompany)
        {
            try
            {
                var subject = "External Invoice Declined";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.DeclinedExternalInvoice);
                emailTemplate = emailTemplate.Replace("[AccountAdmin]", accountAdmin)
                    .Replace("[CompanyName]", externalCompany)
                                             .Replace("[EmployeeName]", employee.FullName)
                                             .Replace("[InvoiceNumber]", invoiceNumber.ToString())
                                             .Replace("[TotalAmountDue]", LocaleHelper.FormatCurrency(totalAmountDue))
                                             .Replace("[Comment]", !string.IsNullOrEmpty(comment) ? comment : "No comment added.");

                await _emailService.SendEmail(employee.Email, subject, emailTemplate);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }
        public async Task<ApiResult<bool>> DeclineRequest(ExternalInvoiceActionRequestDto model)
        {
            try
            {
                var eventDetails = await _externalInvoiceRepository.FirstOrDefaultAsync(x => x.Id == model.Id);
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var declinedStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.DECLINED.ToString()).Id;
                eventDetails.InvoiceStatusId = declinedStatusId;
                var externalInvoiceDetailsResponse = await FetchExternalInvoiceDetails(model.Id);
                var result = await _externalInvoiceRepository.UpdateAsync(eventDetails);
                ExternalInvoiceHistory entity = new ExternalInvoiceHistory()
                {
                    InvoiceId = eventDetails.Id,
                    InvoiceStatusId = eventDetails.InvoiceStatusId,
                    Comment = model.Comment,
                };
                await _externalInvoiceHistoryRepository.InsertAsync(entity);
                var invoiceEntity = (await _externalInvoiceRepository.QueryAsync(x => x.Id == result.Id, include: entities => entities.Include(e => e.Company).Include(e => e.ApplicationUser))).FirstOrDefault();

                var accountAdminId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                var accountAdmins = (await _applicationService.GetAccountAdmins()).Result;
                var accountAdmin = accountAdmins.Where(x => x.Id == accountAdminId).FirstOrDefault();
                var externalInvoiceDetail = externalInvoiceDetailsResponse.Result;


                if (accountAdmin != null)
                {
                    await SendInvoiceDeclinedEmailToEmployee(invoiceEntity.ApplicationUser, invoiceEntity.Company.CompanyName, accountAdmin.FullName, invoiceEntity.InvoiceNumber, invoiceEntity.TotalPriceIncludingVat, entity.Comment, externalInvoiceDetail.ExternalCompanyName);
                }


                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<bool> SendInvoiceApprovalEmailToCustomer(int Id)
        {
            var entity = (await _externalInvoiceRepository.QueryAsync(x => x.Id == Id, include: entities => entities.Include(e => e.ApplicationUser).Include(e => e.Company))).FirstOrDefault();
            var details = (await _invoiceDetailRepository.QueryAsync(x => x.ExternalInvoiceId == Id)).ToList();
            var externalInvoiceDetailsResponse = await FetchExternalInvoiceDetails(Id);
            ExternalInvoiceHistory historyEntity = new ExternalInvoiceHistory
            {
                InvoiceId = entity.Id,
                InvoiceStatusId = entity.InvoiceStatusId,
            };
            await _externalInvoiceHistoryRepository.InsertAsync(historyEntity);
            await _externalInvoiceRepository.UpdateAsync(entity);
            var externalInvoiceDetail = externalInvoiceDetailsResponse.Result;

            return await SendInvoiceToCustomer(entity.Company.Email, entity, entity.Company.CompanyName, entity.ApplicationUser.FullName, entity.InvoiceNumber, entity.TotalPriceIncludingVat ,externalInvoiceDetail.ExternalCompanyName);
        }
        private async Task<bool> SendInvoiceToCustomer(string email, ExternalInvoice invoice, string companyName, string employeeName, long invoiceNumber, decimal totalAmountDue,string externalCompany
         )
        {
            try
            {
                var subject = "External Invoice Request";
                var link = GetInvoiceLinkForAccountAdmin((int)invoice.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.RequestExternalInvoice);
                emailTemplate = emailTemplate.Replace("[CompanyName]", externalCompany)
                                             .Replace("[EmployeeName]", employeeName)
                                             .Replace("[LinkToOrder]", link)
                                             .Replace("[InvoiceNumber]", invoiceNumber.ToString())
                                             .Replace("[TotalAmountDue]", LocaleHelper.FormatCurrency(totalAmountDue));
                ;

                return await _emailService.SendEmail(email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }


        public async Task<ApiResult<List<ExternalInvoiceHistoryDto>>> GetExternalInvoiceHistoryById(int id)
        {
            try
            {
                var entities = await _externalInvoiceHistoryRepository.QueryAsync(e => e.InvoiceId == id, disableTracking: true);
           
                var mappedEntities = entities.Select(e => new ExternalInvoiceHistoryDto
                {
                    Id = e.Id,
                    InvoiceId = e.InvoiceId,
                    InvoiceStatusId = e.InvoiceStatusId,
                    InvoiceStatusName = e.InvoiceStatus.Name,
                    Comment = e.Comment,
                    CreatedAt = e.CreatedAt,
                }).ToList();

                return ApiResult<List<ExternalInvoiceHistoryDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        private IQueryable<ExternalInvoice> ApplyFilters(IQueryable<ExternalInvoice> entities, PageQueryFiterBase filter)
        {
            filter.GetValue<string>("employeeId", (v) =>
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                entities = entities.Where(e => e.EmployeeId == userId);
            });


            filter.GetValue<bool>("excludeDraft", (v) =>
            {
                var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
                entities = entities.Where(e => !e.InvoiceStatus.StatusKey.ToLower().Contains("draft"));
            });

            filter.GetValue<string>("invoiceNumber", (v) =>
            {
                entities = entities.Where(e => e.InvoiceNumber == Convert.ToInt32(v));
            });

            filter.GetValue<string>("companyName", (v) =>
            {
                var searchTerms = v.Split(' ');
                foreach (var term in searchTerms)
                {
                    var lowerCaseTerm = term.ToLower();
                    entities = entities.Where(e => e.CompanyName != null && e.CompanyName.ToLower().Contains(lowerCaseTerm));
                }
            });
          
            filter.GetValue<string>("employeeName", (v) =>
            {
                var searchTerms = v.Split(' ');
                foreach (var term in searchTerms)
                {
                    var lowerCaseTerm = term.ToLower();
                    entities = entities.Where(e => e.ApplicationUser.FullName != null && e.ApplicationUser.FullName.ToLower().Contains(lowerCaseTerm));
                }
            });

            filter.GetValue<string>("companyAddress", (v) =>
            {
                var searchTerms = v.Split(' ');
                foreach (var term in searchTerms)
                {
                    var lowerCaseTerm = term.ToLower();
                    entities = entities.Where(e => e.CompanyAddress != null && e.CompanyAddress.ToLower().Contains(lowerCaseTerm));
                }
            });
            filter.GetValue<DateTime>("invoiceDate", (v) =>
            {
                entities = entities.Where(e => e.InvoiceDate != null && e.InvoiceDate < v);
            }, OperatorType.lessthan, true);
            filter.GetValue<DateTime>("invoiceDate", (v) =>
            {
                entities = entities.Where(e => e.InvoiceDate != null && e.InvoiceDate > v);
            }, OperatorType.greaterthan, true);


            filter.GetValue<DateTime>("expiryDate", (v) =>
            {
                entities = entities.Where(e => e.ExpiryDate != null && e.ExpiryDate < v);
            }, OperatorType.lessthan, true);
            filter.GetValue<DateTime>("expiryDate", (v) =>
            {
                entities = entities.Where(e => e.ExpiryDate != null && e.ExpiryDate > v);
            }, OperatorType.greaterthan, true);


            filter.GetValue<string>("invoiceStatusId", (v) =>
            {
                entities = entities.Where(e => e.InvoiceStatusId.ToString() == v);
            });
            filter.GetValue<string>("invoiceStatusName", (v) =>
            {
                entities = entities.Where(e => e.InvoiceStatus.Name.ToLower().Contains(v.ToLower()) || e.InvoiceStatus.Name.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<string>("paymentStatusId", (v) =>
            {
                entities = entities.Where(e => e.PaymentStatusId.ToString() == v);
            });
            filter.GetValue<string>("paymentStatusName", (v) =>
            {
                entities = entities.Where(e => e.PaymentStatus.Name.ToLower().Contains(v.ToLower()) || e.PaymentStatus.Name.ToLower().Contains(v.ToLower()));
            });

            return entities;
        }
        private IQueryable<ExternalInvoice> ApplySorting(IQueryable<ExternalInvoice> entities, SortModel sort)
        {
            try
            {
                if (sort?.Name == null)
                {
                    entities = entities.OrderByDescending(o => o.CreatedAt);
                    return entities;
                }
                var columnName = sort.Name.ToUpper();
                if (sort.Direction == SortDirection.ascending.ToString())
                {
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.CompanyName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Company.CompanyName);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.EmployeeName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.ApplicationUser.FullName);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.CompanyAddress).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Company.CompanyName);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.InvoiceNumber).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.InvoiceNumber);
                    }

                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.InvoiceStatusName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.InvoiceStatus.Name);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.PaymentStatusName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.PaymentStatus.Name);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.InvoiceDate).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.InvoiceDate);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.ExpiryDate).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.ExpiryDate);
                    }
                }

                else
                {
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.CompanyName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Company.CompanyName);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.EmployeeName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.ApplicationUser.FullName);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.InvoiceNumber).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.InvoiceNumber);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.InvoiceStatusName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.InvoiceStatus.Name);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.PaymentStatusName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.PaymentStatus.Name);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.InvoiceDate).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.InvoiceDate);
                    }
                    if (columnName.ToUpper() == nameof(ExternalInvoiceResponseDto.ExpiryDate).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.ExpiryDate);
                    }

                }
                return entities;

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return entities;
            }

        }

    }

}


