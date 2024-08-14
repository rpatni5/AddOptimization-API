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
    public class InvoiceService : IInvoiceService
    {
        private readonly IGenericRepository<Invoice> _invoiceRepository;
        private readonly IGenericRepository<InvoiceDetail> _invoiceDetailRepository;
        private readonly IGenericRepository<Customer> _customer;
        private readonly IGenericRepository<InvoicePaymentHistory> _invoicePaymentRepository;
        private readonly IGenericRepository<InvoiceCreditNotes> _invoiceCreditNoteRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<SchedulerEvent> _schedulersRepository;
        private readonly IGenericRepository<SchedulerEventDetails> _schedulersDetailsRepository;
        private readonly ISchedulersStatusService _schedulersStatusService;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly IInvoiceStatusService _invoiceStatusService;
        private readonly ISchedulerEventTypeService _schedulerEventTypeService;
        private readonly IGenericRepository<PublicHoliday> _publicHolidayRepository;
        private readonly IGenericRepository<Employee> _employeeRepository;
        private readonly IGenericRepository<Company> _companyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly List<string> _currentUserRoles;
        private readonly IConfiguration _configuration;
        private readonly CustomDataProtectionService _protectionService;
        private readonly ITemplateService _templateService;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository<InvoiceHistory> _invoiceHistoryRepository;
        private readonly IGenericRepository<ApplicationUser> _appUserRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IApplicationUserService _applicationService;


        private readonly ILogger<InvoiceService> _logger;
        public InvoiceService(IGenericRepository<Invoice> invoiceRepository,
            IGenericRepository<Customer> customer,
            IGenericRepository<Country> countryRepository,
            IGenericRepository<SchedulerEvent> schedulersRepository,
            IGenericRepository<SchedulerEventDetails> schedulersDetailsRepository,
            ISchedulersStatusService schedulersStatusService,
            IInvoiceStatusService invoiceStatusService,
            IPaymentStatusService paymentStatusService,
            ISchedulerEventTypeService schedulerEventTypeService,
            IGenericRepository<PublicHoliday> publicHolidayRepository,
            IGenericRepository<InvoiceDetail> invoiceDetailRepository,
            IGenericRepository<Employee> employeeRepository,
            IGenericRepository<Company> companyRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
             IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            CustomDataProtectionService protectionService,
            ITemplateService templateService,
            IEmailService emailService,
            IGenericRepository<InvoiceHistory> invoiceHistoryRepository,
            IGenericRepository<ApplicationUser> appUserRepository,
            IGenericRepository<Role> roleRepository,
            IApplicationUserService applicationService,
            IGenericRepository<InvoiceCreditNotes> invoiceCreditNoteRepository,
            IGenericRepository<InvoicePaymentHistory> invoicePaymentRepository,

        ILogger<InvoiceService> logger)
        {
            _paymentStatusService = paymentStatusService;
            _invoiceStatusService = invoiceStatusService;
            _companyRepository = companyRepository;
            _publicHolidayRepository = publicHolidayRepository;
            _invoiceRepository = invoiceRepository;
            _countryRepository = countryRepository;
            _customer = customer;
            _schedulersRepository = schedulersRepository;
            _schedulerEventTypeService = schedulerEventTypeService;
            _schedulersStatusService = schedulersStatusService;
            _schedulersDetailsRepository = schedulersDetailsRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _invoiceStatusService = invoiceStatusService;
            _mapper = mapper;
            _configuration = configuration;
            _protectionService = protectionService;
            _templateService = templateService;
            _emailService = emailService;
            _currentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
            _invoiceHistoryRepository = invoiceHistoryRepository;
            _appUserRepository = appUserRepository;
            _roleRepository = roleRepository;
            _applicationService = applicationService;
            _invoiceCreditNoteRepository = invoiceCreditNoteRepository;
            _invoicePaymentRepository = invoicePaymentRepository;
        }

        public async Task<ApiResult<bool>> GenerateInvoice(Guid customerId, MonthDateRange month,
            List<CustomerEmployeeAssociationDto> associatedEmployees)
        {
            try
            {
                _logger.LogInformation("GenerateInvoice service Started.");
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

                string customerAddress = await GetCustomerAddress(customer);
                var company = await _companyRepository.FirstOrDefaultAsync(ignoreGlobalFilter: true);
                string companyAddress = await GenerateCompanyAddress(company);
                string companyBankDetails = GenerateCompanyBankDetails(company);
                var invoiceNumber = await GenerateInvoiceNumber(month);

                var invoiceStatus = (await _invoiceStatusService.Search()).Result;
                var draftStatusId = invoiceStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusEnum.DRAFT.ToString()).Id;
                var paymentStatus = (await _paymentStatusService.Search()).Result;
                var unPaidStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusEnum.UNPAID.ToString()).Id;
                var maxId = await _invoiceRepository.MaxAsync<Int64>(e => e.Id, ignoreGlobalFilter: true);
                var newId = maxId + 1;
                var invoice = new Invoice
                {
                    Id = newId,
                    CustomerId = customer.Id,
                    CustomerAddress = customerAddress,
                    CompanyAddress = companyAddress,
                    CompanyBankDetails = companyBankDetails,
                    ExpiryDate = customer.PaymentClearanceDays.HasValue ? DateTime.UtcNow.AddDays(customer.PaymentClearanceDays.Value) : DateTime.UtcNow.AddDays(15),
                    InvoiceDate = month.EndDate,
                    InvoiceNumber = Convert.ToInt64(invoiceNumber),
                    InvoiceStatusId = draftStatusId,
                    PaymentStatusId = unPaidStatusId,
                    PaymentClearanceDays = customer.PaymentClearanceDays.HasValue ? customer.PaymentClearanceDays.Value : 15,
                    MetaData = "Timesheet",
                };
                var invoiceResult = await _invoiceRepository.InsertAsync(invoice);

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
                    if (employeeEvent == null) break;

                    var employeeEventDetails = (await _schedulersDetailsRepository.QueryAsync(c => c.SchedulerEventId == employeeEvent.Id && !c.IsDeleted)).ToList();

                    var description = string.Empty;
                    decimal unitPrice;
                    //Normal day timesheet Mon-Fri
                    var monFriTimesheetList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsWeekday(c.Date.Value) && !publicHolidays.Contains(c.Date.Value.Date) && c.EventTypeId == timesheetEventId).ToList();
                    description = empl?.ApplicationUser?.FullName + '-' + empl.JobTitle;
                    await CalculateAndSaveInvoiceDetails(invoiceResult, monFriTimesheetList, daily, customer.PartnerVAT ??customer.VAT ?? 0, description);

                    //Sat timesheet including overtime
                    var saturdayTimesheetList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsSaturday(c.Date.Value)).ToList();
                    unitPrice = daily / 8 * saturday / 100;
                    description = $"{empl?.ApplicationUser?.FullName}-{empl.JobTitle}-WE (Saturday) {saturday}% ({daily / 8} eur/h)";   // WE (Sunday) 210% (71,88 eur/h)
                    await CalculateInvoiceDetailsForWeekend(invoiceResult, saturdayTimesheetList, unitPrice, customer.PartnerVAT ?? customer.VAT ?? 0, description, timesheetEventId, overtimeEventId);

                    //Sun timesheet including overtime
                    unitPrice = daily / 8 * sunday / 100;
                    description = $"{empl?.ApplicationUser?.FullName}-{empl.JobTitle}-WE (Sunday) {sunday}% ({daily / 8} eur/h)";   // WE (Sunday) 210% (71,88 eur/h)
                    var sundayTimesheetList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsSunday(c.Date.Value)).ToList();
                    await CalculateInvoiceDetailsForWeekend(invoiceResult, sundayTimesheetList, unitPrice, customer.PartnerVAT ?? customer.VAT ?? 0, description, timesheetEventId, overtimeEventId);

                    //Overtime Mon-Fri
                    unitPrice = daily / 8 * overTime / 100;
                    description = $"{empl?.ApplicationUser?.FullName}-{empl.JobTitle}-Overtime {overTime}% ({daily / 8} eur/h)";
                    var overtimeList = employeeEventDetails.Where(c => c.EventTypeId == overtimeEventId && MonthDateRangeHelper.IsWeekday(c.Date.Value)).ToList();
                    await CalculateAndSaveInvoiceDetails(invoiceResult, overtimeList, unitPrice, customer.PartnerVAT ?? customer.VAT ?? 0, description);

                    //Timesheet Mon-Fri on public holiday
                    unitPrice = daily * publicHoliday / 100;
                    description = $"{empl?.ApplicationUser?.FullName}-{empl.JobTitle}-Holiday {publicHoliday}% ({daily} eur/d)";
                    var publicHolidaysList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsWeekday(c.Date.Value) && publicHolidays.Contains(c.Date.Value.Date) && c.EventTypeId == timesheetEventId).ToList();
                    await CalculateAndSaveInvoiceDetails(invoiceResult, publicHolidaysList, unitPrice, customer.PartnerVAT ?? customer.VAT ?? 0, description);

                }

                decimal totalIn = 0;
                decimal totalEx = 0;
                var invoiceDetails = (await _invoiceDetailRepository.QueryAsync(c => c.InvoiceId == invoiceResult.Id, ignoreGlobalFilter: true)).ToList();

                if (!invoiceDetails.Any())  // if no record is found in invoice details then do not update the invoice.
                    return ApiResult<bool>.Failure("false");

                foreach (var detail in invoiceDetails)
                {
                    totalIn += detail.TotalPriceIncludingVat;
                    totalEx += detail.TotalPriceExcludingVat;
                }
                invoiceResult.DueAmount = invoiceResult.TotalPriceIncludingVat = totalIn;
                invoiceResult.TotalPriceExcludingVat = totalEx;
                invoiceResult.VatValue = totalIn - totalEx;

                var finalInvoice = await _invoiceRepository.UpdateAsync(invoiceResult);
                _logger.LogInformation("GenerateInvoice service Completed.");
                return ApiResult<bool>.Success("true");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private static string GenerateCompanyBankDetails(Company company)
        {
            StringBuilder companyBankDetails = new StringBuilder();
            companyBankDetails.AppendLine(company.BankName);
            companyBankDetails.AppendLine(company.BankAccountName);
            companyBankDetails.AppendLine(company.BankAccountNumber);
            companyBankDetails.AppendLine(company.BankAddress);
            return companyBankDetails.ToString();
        }

        private async Task<string> GenerateCompanyAddress(Company company)
        {
            var countryId = company.CountryId;
            var country = await _countryRepository.FirstOrDefaultAsync(c => c.Id == countryId, ignoreGlobalFilter: true);
            StringBuilder companyAddress = new StringBuilder();
            companyAddress.AppendLine(company.CompanyName);
            companyAddress.AppendLine(company.Address);
            companyAddress.AppendLine(company.City);
            companyAddress.AppendLine(company.State);
            companyAddress.AppendLine(company.ZipCode.ToString());
            companyAddress.AppendLine(country.CountryName);
            return companyAddress.ToString();
        }
        
        private async Task<string> GenerateInvoiceNumber(MonthDateRange month)
        {
            var invoice = (await _invoiceRepository.QueryAsync(ignoreGlobalFilter: true)).ToList();
            var maxInvoiceNo = invoice.Count == 0 ? 0 : invoice.Max(c => c.Id);
            if (maxInvoiceNo != 0)
            {
                return $"{month.StartDate.Year}{month.StartDate.Month}{maxInvoiceNo + 1}";
            }
            else
            {
                return $"{month.StartDate.Year}{month.StartDate.Month}{maxInvoiceNo + 1}";
            }
        }

        private async Task<string> GetCustomerAddress(Customer customer)
        {
            var customerAddress = string.Empty;
            var country = await _countryRepository.FirstOrDefaultAsync(c => c.Id == customer.CountryId, ignoreGlobalFilter: true);
            string sb = string.Empty;

            if (customer.PartnerName != null)
            {
                sb+=JoinNonNull(customer.PartnerAddress, customer.PartnerAddress2);
                sb+=JoinNonNull(customer.PartnerCity, customer.PartnerState);
                sb+=JoinNonNull(customer.PartnerZipCode?.ToString(), country.CountryName);
                sb += JoinNonNull(customer.PartnerVATNumber);
            }
            else
            {
                sb += JoinNonNull(customer.Address, customer.Address2);
                sb += JoinNonNull(customer.City, customer.State);
                sb += JoinNonNull(country.CountryName, customer.ZipCode.ToString());
                sb += JoinNonNull(customer.VATNumber);
            }
            customerAddress = sb.ToString();

            return customerAddress;
        }

        public string JoinNonNull(params string[] values)
        {
            var nonNullValues = values.Where(v => !string.IsNullOrEmpty(v)).ToList();
            var newString =  string.Join(", ", nonNullValues);
            return string.IsNullOrEmpty(newString) ? string.Empty : newString + Environment.NewLine;
        }

        private async Task CalculateAndSaveInvoiceDetails(Invoice invoice, List<SchedulerEventDetails> schedulerEventDetails, decimal daily, decimal vat, string description)
        {
            var quantity = schedulerEventDetails.Sum(c => c.Duration);
            if(quantity > 0)
            {
                var invoiceDetail = new InvoiceDetail
                {
                    InvoiceId = invoice.Id,
                    Description = description,
                    Quantity = quantity,
                    UnitPrice = daily,
                    TotalPriceExcludingVat = daily * quantity,
                    TotalPriceIncludingVat = (daily * quantity) + (daily * quantity * vat / 100),
                    VatPercent = vat,
                    Metadata = "Timesheet"
                };
                await _invoiceDetailRepository.InsertAsync(invoiceDetail);
            }            
        }

        private async Task CalculateInvoiceDetailsForWeekend(Invoice invoice, List<SchedulerEventDetails> schedulerEventDetails, decimal unitPrice, decimal vat, string description, Guid timesheetEventId, Guid overtimeEventId)
        {
            var timesheetQuantityInHr = schedulerEventDetails.Where(x => x.EventTypeId == timesheetEventId).Sum(c => c.Duration) * 8;
            var overtimeQuantityInHr = schedulerEventDetails.Where(x => x.EventTypeId == overtimeEventId).Sum(c => c.Duration);

            var quantity = timesheetQuantityInHr + overtimeQuantityInHr;
            if (quantity > 0)
            {
                var invoiceDetail = new InvoiceDetail
                {
                    InvoiceId = invoice.Id,
                    Description = description,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPriceExcludingVat = unitPrice * quantity,
                    TotalPriceIncludingVat = (unitPrice * quantity) + (unitPrice * quantity * vat / 100),
                    VatPercent = vat,
                    Metadata = "Timesheet"
                };
                await _invoiceDetailRepository.InsertAsync(invoiceDetail);
            }
        }

        private async Task<bool> SendRequestInvoiceEmailReminderToCustomer(string email, Invoice invoice)
        {
            try
            {
                var amount = String.Format(new CultureInfo("en-US"), "€{0:N2}", invoice?.DueAmount);
                var subject = $"AddOptimization invoice pending for {invoice?.InvoiceDate.Date.ToString("dd/MM/yyyy")} of {amount}";
                var link = GetInvoiceLinkForCustomer(invoice.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.UnpaidInvoiceReminder);
                _ = int.TryParse(invoice?.Customer?.PaymentClearanceDays.ToString(), out int clearanceDays);
                emailTemplate = emailTemplate
                                .Replace("[CustomerName]", invoice?.Customer?.ManagerName)
                                .Replace("[CompanyName]", invoice?.Customer?.Organizations)
                                .Replace("[InvoiceNumber]", invoice?.InvoiceNumber.ToString())
                                .Replace("[InvoiceDate]", invoice?.InvoiceDate.Date.ToString("dd/MM/yyyy"))
                                .Replace("[TotalAmountDue]", invoice?.DueAmount.ToString("N2", CultureInfo.InvariantCulture))
                                .Replace("[DueDate]", invoice?.ExpiryDate.Date.ToString("dd/MM/yyyy"))
                                .Replace("[LinkToInvoice]", link);
                return await _emailService.SendEmail(email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }

        private async Task<bool> SendRequestInvoiceEmailToCustomer(string email, Invoice invoice)
        {
            try
            {
                var amount = String.Format(new CultureInfo("en-US"), "€{0:N2}", invoice?.DueAmount);
                var subject = $"AddOptimization invoice pending for {invoice?.InvoiceDate.Date.ToString("dd/MM/yyyy")} of {amount}";
                var link = GetInvoiceLinkForCustomer(invoice.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.UnpaidInvoice);
                _ = int.TryParse(invoice?.Customer?.PaymentClearanceDays.ToString(), out int clearanceDays);
                emailTemplate = emailTemplate
                                .Replace("[CustomerName]", invoice?.Customer?.ManagerName)
                                .Replace("[CompanyName]", invoice?.Customer?.Organizations)
                                .Replace("[InvoiceNumber]", invoice?.InvoiceNumber.ToString())
                                .Replace("[InvoiceDate]", invoice?.InvoiceDate.Date.ToString("dd/MM/yyyy"))
                                .Replace("[TotalAmountDue]", invoice?.DueAmount.ToString("N2", CultureInfo.InvariantCulture))
                                .Replace("[DueDate]", invoice?.ExpiryDate.Date.ToString("dd/MM/yyyy"))
                                .Replace("[LinkToInvoice]", link);
                return await _emailService.SendEmail(email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }

        private async Task<bool> SendInvoiceDeclinedEmailToAccountAdmins(IEnumerable<dynamic> accountAdmins, string customerName, long invoiceNumber, int? dueDate, decimal totalAmountDue, string comment)
        {
            try
            {
                var subject = "Invoice Declined";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.DeclinedInvoice);
                emailTemplate = emailTemplate.Replace("[CustomerName]", customerName)
                                             .Replace("[InvoiceNumber]", invoiceNumber.ToString())
                                             .Replace("[TotalAmountDue]", totalAmountDue.ToString("N2", CultureInfo.InvariantCulture))
                                             .Replace("[DueDate]", dueDate.ToString())
                                             .Replace("[Comment]", !string.IsNullOrEmpty(comment) ? comment : "No comment added.");
                foreach (var admin in accountAdmins)
                {
                    var personalizedEmailTemplate = emailTemplate.Replace("[AccountAdmin]", admin.Name);
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

        private IQueryable<Invoice> ApplyFilters(IQueryable<Invoice> entities, PageQueryFiterBase filter)
        {
            filter.GetValue<string>("invoiceNumber", (v) =>
            {
                entities = entities.Where(e => e.InvoiceNumber == Convert.ToInt32(v));
            });

            filter.GetValue<string>("customerName", (v) =>
            {
                entities = entities.Where(e => e.Customer.Organizations.ToLower().Contains(v.ToLower()));
            });
            filter.GetValue<DateTime>("invoiceDate", (v) =>
            {
                entities = entities.Where(e => e.InvoiceDate != null && e.InvoiceDate < v);
            }, OperatorType.lessthan, true);
            filter.GetValue<DateTime>("invoiceDate", (v) =>
            {
                entities = entities.Where(e => e.InvoiceDate != null && e.InvoiceDate > v);
            }, OperatorType.greaterthan, true);

            filter.GetValue<int>("totalPriceExcludingVat", (v) =>
            {
                entities = entities.Where(e => e.TotalPriceExcludingVat == v);
            });
            filter.GetValue<int>("totalPriceIncludingVat", (v) =>
            {
                entities = entities.Where(e => e.TotalPriceIncludingVat == v);
            });
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

            filter.GetValue<DateTime>("expiryDate", (v) =>
            {
                entities = entities.Where(e => e.ExpiryDate != null && e.ExpiryDate < v);
            }, OperatorType.lessthan, true);
            filter.GetValue<DateTime>("expiryDate", (v) =>
            {
                entities = entities.Where(e => e.ExpiryDate != null && e.ExpiryDate > v);
            }, OperatorType.greaterthan, true);

            return entities;
        }

        private IQueryable<Invoice> ApplySorting(IQueryable<Invoice> entities, SortModel sort)
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
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.CustomerName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.Customer.ManagerName);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.InvoiceNumber).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.InvoiceNumber);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.TotalPriceExcludingVat).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.TotalPriceExcludingVat);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.TotalPriceIncludingVat).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.TotalPriceIncludingVat);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.InvoiceStatusName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.InvoiceStatus.Name);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.PaymentStatusName).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.PaymentStatus.Name);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.InvoiceDate).ToUpper())
                    {
                        entities = entities.OrderBy(o => o.InvoiceDate);
                    }
                }

                else
                {
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.CustomerName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.Customer.ManagerName);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.InvoiceNumber).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.InvoiceNumber);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.TotalPriceExcludingVat).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.TotalPriceExcludingVat);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.TotalPriceIncludingVat).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.TotalPriceIncludingVat);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.InvoiceStatusName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.InvoiceStatus.Name);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.PaymentStatusName).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.PaymentStatus.Name);
                    }
                    if (columnName.ToUpper() == nameof(InvoiceResponseDto.InvoiceDate).ToUpper())
                    {
                        entities = entities.OrderByDescending(o => o.InvoiceDate);
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


        public async Task<ApiResult<InvoiceResponseDto>> Create(InvoiceRequestDto model)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var statusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.DRAFT.ToString()).Id;
                var company = await _companyRepository.FirstOrDefaultAsync(ignoreGlobalFilter: true);
                string companyAddress = await GenerateCompanyAddress(company);
                string companyBankDetails = GenerateCompanyBankDetails(company);

                var paymentStatus = (await _paymentStatusService.Search()).Result;
                var paymentStatusId = paymentStatus.FirstOrDefault(x => x.StatusKey == PaymentStatusesEnum.UNPAID.ToString()).Id;

                var maxId = await _invoiceRepository.MaxAsync<Int64>(e => e.Id, ignoreGlobalFilter: true);
                var newId = maxId + 1;
                var invoiceNumber = long.Parse($"{DateTime.UtcNow:yyyyMM}{newId}");
                
                Invoice entity = new Invoice
                {
                    Id = newId,
                    InvoiceNumber = Convert.ToInt64(invoiceNumber),
                    PaymentStatusId = paymentStatusId,
                    VatValue = model.InvoiceDetails.Sum(x => (x.UnitPrice * x.Quantity * x.VatPercent) / 100),
                    TotalPriceIncludingVat = model.InvoiceDetails.Sum(x => x.TotalPriceIncludingVat),
                    TotalPriceExcludingVat = model.InvoiceDetails.Sum(x => x.TotalPriceExcludingVat),
                    CustomerId = model.CustomerId,
                    ExpiryDate = model.ExpiryDate,
                    InvoiceDate = model.InvoiceDate,
                    CustomerAddress = model.CustomerAddress,
                    InvoiceStatusId = statusId,
                    PaymentClearanceDays = model.PaymentClearanceDays,
                    CompanyAddress = companyAddress,
                    CompanyBankDetails = companyBankDetails,
                    MetaData = "Manual",
                };
                entity.DueAmount = entity.TotalPriceIncludingVat;
                await _invoiceRepository.InsertAsync(entity);

                foreach (var summary in model.InvoiceDetails)
                {
                    var invoiceDetail = new InvoiceDetail
                    {
                        InvoiceId = entity.Id,
                        Description = summary.Description,
                        Quantity = summary.Quantity,
                        VatPercent = summary.VatPercent,
                        UnitPrice = summary.UnitPrice,
                        TotalPriceExcludingVat = summary.TotalPriceExcludingVat,
                        TotalPriceIncludingVat = summary.TotalPriceIncludingVat,
                        Metadata = "Manual",

                    };

                    await _invoiceDetailRepository.InsertAsync(invoiceDetail);
                    entity.InvoiceDetails.Add(invoiceDetail);
                }
                await _unitOfWork.CommitTransactionAsync();
                var mappedEntity = _mapper.Map<InvoiceResponseDto>(entity);
                return ApiResult<InvoiceResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<PagedApiResult<InvoiceResponseDto>> Search(PageQueryFiterBase filters)
        {
            try
            {
                var entities = await _invoiceRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(x => x.Customer).Include(x => x.PaymentStatus).Include(x => x.InvoiceStatus), ignoreGlobalFilter: true);
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);
                entities = entities.Where(e => !(e.InvoiceStatus.Name == "Draft" && e.ExpiryDate < DateTime.UtcNow));
                var pagedResult = PageHelper<Invoice, InvoiceResponseDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new InvoiceResponseDto
                {
                    Id = e.Id,
                    InvoiceNumber = e.InvoiceNumber,
                    InvoiceDate = e.InvoiceDate,
                    InvoiceStatusId = e.InvoiceStatusId,
                    InvoiceStatusName = e.InvoiceStatus.Name,
                    PaymentStatusId = e.PaymentStatusId,
                    PaymentStatusName = e.PaymentStatus.Name,
                    CustomerAddress = e.CustomerAddress,
                    CompanyAddress = e.CompanyAddress,
                    VatValue = e.VatValue,
                    CompanyBankDetails = e.CompanyBankDetails,
                    TotalPriceExcludingVat = e.TotalPriceExcludingVat,
                    TotalPriceIncludingVat = e.TotalPriceIncludingVat,
                    CustomerId = e.CustomerId,
                    CustomerName = e.Customer.Organizations,
                    ExpiryDate = e.ExpiryDate,
                    PaymentClearanceDays = e.PaymentClearanceDays,
                    DueAmount = e.DueAmount,
                    HasCreditNotes=e.HasCreditNotes,
                    CreditNoteNumber = e.CreditNoteNumber,
                }).ToList());

                var result = pagedResult;
                return PagedApiResult<InvoiceResponseDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<InvoiceResponseDto>> FetchInvoiceDetails(int id, bool getRoleBasedData = true)
        {
            try
            {
                bool ignoreGlobalFilter = true;
                if (getRoleBasedData)
                {
                    var superAdminRole = _currentUserRoles.Where(c => c.Contains("Super Admin") || c.Contains("Account Admin")).ToList();
                    ignoreGlobalFilter = superAdminRole.Count != 0;
                }
                var company = await _companyRepository.FirstOrDefaultAsync(include: entities => entities
            .Include(e => e.CountryName),ignoreGlobalFilter: true);

             
                var model = new InvoiceResponseDto();
                var entity = await _invoiceRepository.FirstOrDefaultAsync(e => e.Id == id , ignoreGlobalFilter: true);
                model.Id = entity.Id;
                model.CustomerId = entity.CustomerId;
                model.ExpiryDate = entity.ExpiryDate;
                model.InvoiceDate = entity.InvoiceDate;
                model.CustomerAddress = entity.CustomerAddress;
                model.CompanyAddress = company.Address;
                model.CompanyCity = company.City;
                model.CompanyCountry = company.CountryName.CountryName;
                model.CompanyState = company.State;
                model.CompanyZipCode = company.ZipCode;
                model.CompanyBankName = company.BankName;
                model.CompanyBankAccountName = company.BankAccountName;
                model.CompanyBankAccontNumber = company.BankAccountNumber;
                model.CompanyBankAddress = company.BankAddress;
                model.CompanyBankDetails= entity.CompanyBankDetails;
                model.InvoiceStatusId = entity.InvoiceStatusId;
                model.PaymentStatusId = entity.PaymentStatusId;
                model.InvoiceNumber = entity.InvoiceNumber;
                model.VatValue = entity.VatValue;
                model.TotalPriceExcludingVat = entity.TotalPriceExcludingVat;
                model.TotalPriceIncludingVat = entity.TotalPriceIncludingVat;
                model.PaymentClearanceDays = entity.PaymentClearanceDays;
                model.CreditNoteNumber = entity.CreditNoteNumber;
                model.CreatedAt = entity.CreatedAt;
                model.SwiftCode = company.SwiftCode;
                model.DueAmount = entity.DueAmount;
                var invoiceSummary = (await _invoiceDetailRepository.QueryAsync(e => e.InvoiceId == id, disableTracking: true)).ToList();
                model.InvoiceDetails = _mapper.Map<List<InvoiceDetailDto>>(invoiceSummary);
                return ApiResult<InvoiceResponseDto>.Success(model);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<InvoiceResponseDto>> Update(int id, InvoiceRequestDto model)
        {
            try
            {
                var entity = await _invoiceRepository.FirstOrDefaultAsync(e => e.Id == id);
                if (entity == null)
                {
                    return ApiResult<InvoiceResponseDto>.NotFound("Invoice");
                }

                var existingDetails = await _invoiceDetailRepository.QueryAsync(e => e.InvoiceId == id);
                foreach (var detail in existingDetails.ToList())
                {
                    await _invoiceDetailRepository.DeleteAsync(detail);
                }

                var newInvoiceDetails = new List<InvoiceDetail>();
                foreach (var summary in model.InvoiceDetails)
                {
                    var invoiceDetail = new InvoiceDetail
                    {
                        InvoiceId = entity.Id,
                        Description = summary.Description,
                        Quantity = summary.Quantity,
                        VatPercent = summary.VatPercent,
                        UnitPrice = summary.UnitPrice,
                        TotalPriceExcludingVat = summary.TotalPriceExcludingVat,
                        TotalPriceIncludingVat = summary.TotalPriceIncludingVat,
                        Metadata = summary.Metadata,
                    };
                    await _invoiceDetailRepository.InsertAsync(invoiceDetail);
                    newInvoiceDetails.Add(invoiceDetail);
                }

                entity.VatValue = newInvoiceDetails.Sum(x => (x.UnitPrice * x.Quantity * x.VatPercent) / 100);
                entity.TotalPriceIncludingVat = newInvoiceDetails.Sum(x => x.TotalPriceIncludingVat);
                entity.TotalPriceExcludingVat = newInvoiceDetails.Sum(x => x.TotalPriceExcludingVat);

                var existingCreditNotes = await _invoiceCreditNoteRepository.QueryAsync(e => e.InvoiceId == id);
                var existingPayments = await _invoicePaymentRepository.QueryAsync(e => e.InvoiceId == id);
                var totalCreditNotes = existingCreditNotes.Sum(x => x.TotalPriceIncludingVat);
                var totalPayments = existingPayments.Sum(x => x.Amount);
                var totalPaidAmount = totalPayments + totalCreditNotes;
                entity.DueAmount = entity.TotalPriceIncludingVat - totalPaidAmount;

                _mapper.Map(model, entity);
                entity.InvoiceDetails = newInvoiceDetails;
                await _invoiceRepository.UpdateAsync(entity);

                var mappedEntity = _mapper.Map<InvoiceResponseDto>(entity);
                return ApiResult<InvoiceResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public string GetInvoiceLinkForCustomer(long invoiceId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            var encryptedId = _protectionService.Encode(invoiceId.ToString());
            return $"{baseUrl}invoice/approval/{encryptedId}";
        }

        public async Task<bool> SendInvoiceEmailReminderToCustomer(int invoiceId)
        {
            var entity = (await _invoiceRepository.QueryAsync(x => x.Id == invoiceId, include: entities => entities.Include(e => e.Customer))).FirstOrDefault();
            var details = (await _invoiceDetailRepository.QueryAsync(x => x.InvoiceId == invoiceId)).ToList();
            return await SendRequestInvoiceEmailReminderToCustomer(entity.Customer.ManagerEmail, entity);
        }


        public async Task<bool> SendInvoiceToCustomer(int invoiceId)
        {
            var eventStatus = (await _invoiceStatusService.Search()).Result;
            var sentToCustomerStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.SEND_TO_CUSTOMER.ToString()).Id;

            var entity = (await _invoiceRepository.QueryAsync(x => x.Id == invoiceId, include: entities => entities.Include(e => e.Customer))).FirstOrDefault();
            entity.InvoiceStatusId = sentToCustomerStatusId;
            await _invoiceRepository.UpdateAsync(entity);

            var details = (await _invoiceDetailRepository.QueryAsync(x => x.InvoiceId == invoiceId)).ToList();
            return await SendRequestInvoiceEmailToCustomer(entity.Customer.ManagerEmail, entity);
        }

        public async Task<ApiResult<bool>> DeclineRequest(InvoiceActionRequestDto model)
        {
            try
            {
                var eventDetails = await _invoiceRepository.FirstOrDefaultAsync(x => x.Id == model.Id);
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var declinedStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.DECLINED.ToString()).Id;
                eventDetails.InvoiceStatusId = declinedStatusId;
                var result = await _invoiceRepository.UpdateAsync(eventDetails);
                InvoiceHistory entity = new InvoiceHistory()
                {
                    InvoiceId = eventDetails.Id,
                    InvoiceStatusId = eventDetails.InvoiceStatusId,
                    Comment = model.Comment,
                };
                await _invoiceHistoryRepository.InsertAsync(entity);
                var entities = (await _invoiceRepository.QueryAsync(x => x.Id == result.Id, include: entities => entities.Include(e => e.Customer))).FirstOrDefault();
                var accountAdmins = (await _applicationService.GetAccountAdmins()).Result;
                var adminEmails = accountAdmins.Select(admin => new { Name = admin.UserName, Email = admin.Email });

                await SendInvoiceDeclinedEmailToAccountAdmins(adminEmails, entities.Customer.ManagerEmail, entities.InvoiceNumber, entities.PaymentClearanceDays, entities.TotalPriceIncludingVat, entity.Comment);

                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<InvoiceResponseDto>>> GetUnpaidInvoicesForEmailReminder()
        {
            var entity = await _invoiceRepository.QueryAsync(x => x.PaymentStatus.StatusKey == PaymentStatusesEnum.UNPAID.ToString() && !x.IsDeleted, include: entities => entities.Include(e => e.Customer));
            if (entity == null || !entity.Any())
            {
                return ApiResult<List<InvoiceResponseDto>>.Failure(ValidationCodes.UnpaidInvoiceDoesNotExists);
            }
            var response = _mapper.Map<List<InvoiceResponseDto>>(entity);
            return ApiResult<List<InvoiceResponseDto>>.Success(response);
        }
    }
}
