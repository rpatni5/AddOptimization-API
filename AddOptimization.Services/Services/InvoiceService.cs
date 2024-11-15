﻿using AddOptimization.Contracts.Constants;
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
using iText.Layout.Element;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AddOptimization.Services.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IGenericRepository<Invoice> _invoiceRepository;
        private readonly IGenericRepository<InvoiceDetail> _invoiceDetailRepository;
        private readonly IGenericRepository<Customer> _customer;
        private readonly IGenericRepository<CustomerEmployeeAssociation> _associationRepository;
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
        private readonly ICompanyService _companyService;
        private readonly INotificationService _notificationService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ICustomerEmployeeAssociationService _customerEmployeeAssociationService;


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
            ICompanyService companyService,
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
            IGenericRepository<CustomerEmployeeAssociation> associationRepository,
            INotificationService notificationService,
            IApplicationUserService applicationUserService,
            ICustomerEmployeeAssociationService customerEmployeeAssociationService,
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
            _companyService = companyService;
            _templateService = templateService;
            _emailService = emailService;
            _currentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
            _invoiceHistoryRepository = invoiceHistoryRepository;
            _appUserRepository = appUserRepository;
            _roleRepository = roleRepository;
            _applicationService = applicationService;
            _invoiceCreditNoteRepository = invoiceCreditNoteRepository;
            _invoicePaymentRepository = invoicePaymentRepository;
            _associationRepository = associationRepository;
            _notificationService = notificationService;
            _applicationUserService = applicationUserService;
            _customerEmployeeAssociationService = customerEmployeeAssociationService;
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
                    InvoiceDate = month.EndDate,
                    InvoiceNumber = invoiceNumber,
                    InvoiceStatusId = draftStatusId,
                    PaymentStatusId = unPaidStatusId,
                    PaymentClearanceDays = customer.PaymentClearanceDays.HasValue ? customer.PaymentClearanceDays.Value : 15,
                    MetaData = "Timesheet",
                };
                var invoiceResult = await _invoiceRepository.InsertAsync(invoice);

                foreach (var employee in associatedEmployees)
                {
                    var associatedCustomer = await _associationRepository.FirstOrDefaultAsync(t => t.CustomerId == customerId && t.EmployeeId == employee.EmployeeId, ignoreGlobalFilter: true);

                    var publicHolidays = (await _publicHolidayRepository.QueryAsync(o => o.CountryId == associatedCustomer.PublicHolidayCountryId
                        && o.Date.Month == month.StartDate.Month
                        && o.Date.Year == month.StartDate.Year)).Select(x => x.Date.Date);

                    var daily = employee.DailyWeightage;
                    var overTime = employee.Overtime;
                    var publicHoliday = employee.PublicHoliday;
                    var saturday = employee.Saturday;
                    var sunday = employee.Sunday;
                    var jobTitle = employee.JobTitle;

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
                    description = jobTitle;
                    await CalculateAndSaveInvoiceDetails(invoiceResult, monFriTimesheetList, daily, customer.PartnerVAT ?? customer.VAT ?? 0, description);

                    //Sat timesheet including overtime
                    var saturdayTimesheetList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsSaturday(c.Date.Value)).ToList();
                    unitPrice = daily / 8 * saturday / 100;
                    description = $"{jobTitle}-WE (Saturday) {saturday}% ({daily / 8} eur/h)";   // WE (Sunday) 210% (71,88 eur/h)
                    await CalculateInvoiceDetailsForWeekend(invoiceResult, saturdayTimesheetList, unitPrice, customer.PartnerVAT ?? customer.VAT ?? 0, description, timesheetEventId, overtimeEventId);

                    //Sun timesheet including overtime
                    unitPrice = daily / 8 * sunday / 100;
                    description = $"{jobTitle}-WE (Sunday) {sunday}% ({daily / 8} eur/h)";   // WE (Sunday) 210% (71,88 eur/h)
                    var sundayTimesheetList = employeeEventDetails.Where(c => MonthDateRangeHelper.IsSunday(c.Date.Value)).ToList();
                    await CalculateInvoiceDetailsForWeekend(invoiceResult, sundayTimesheetList, unitPrice, customer.PartnerVAT ?? customer.VAT ?? 0, description, timesheetEventId, overtimeEventId);

                    //Overtime Mon-Fri
                    unitPrice = daily / 8 * overTime / 100;
                    description = $"{jobTitle}-Overtime {overTime}% ({daily / 8} eur/h)";
                    var overtimeList = employeeEventDetails.Where(c => c.EventTypeId == overtimeEventId && MonthDateRangeHelper.IsWeekday(c.Date.Value)).ToList();
                    await CalculateAndSaveInvoiceDetails(invoiceResult, overtimeList, unitPrice, customer.PartnerVAT ?? customer.VAT ?? 0, description);

                    //Timesheet Mon-Fri on public holiday
                    unitPrice = daily * publicHoliday / 100;
                    description = $"{jobTitle}-Holiday {publicHoliday}% ({daily} eur/d)";
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
                var applicationUser = (await _applicationUserService.GetAccountAdmins()).Result;
                var invoiceDetail = (await _invoiceRepository.QueryAsync(c => c.Id == finalInvoice.Id, include: entities => entities.Include(i => i.Customer), ignoreGlobalFilter: true)).ToList();

                var historyEntity = new InvoiceHistory
                {
                    InvoiceId = invoice.Id,
                    InvoiceStatusId = invoice.InvoiceStatusId,
                    CreatedAt = DateTime.UtcNow,
                    Comment = "Automatic Generated Unpaid Invoice",
                };
                await _invoiceHistoryRepository.InsertAsync(historyEntity);


                var invoiceResponseDtoList = _mapper.Map<List<InvoiceResponseDto>>(invoiceDetail);
                foreach (var admin in applicationUser)
                {
                    await SendNotificationToAccountAdmin(invoiceResponseDtoList, admin);
                }

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
            try
            {
                var dateFormat = $"{month.StartDate.Year}{month.StartDate.Month:D2}";
                var draftInvoicesCount = (await _invoiceRepository.QueryAsync(x => x.InvoiceNumber.Contains(dateFormat), ignoreGlobalFilter: true)).Count();
                var newDraftNumber = draftInvoicesCount + 1;
                var draftInvoiceNumber = $"Draft-{dateFormat}{newDraftNumber}";

                return draftInvoiceNumber;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private async Task<string> GetCustomerAddress(Customer customer)
        {
            var customerAddress = string.Empty;
            var country = await _countryRepository.FirstOrDefaultAsync(c => c.Id == customer.CountryId, ignoreGlobalFilter: true);
            string sb = string.Empty;

            if (!customer.PartnerCompany.IsNullOrEmpty())
            {
                var partnerCountry = await _countryRepository.FirstOrDefaultAsync(c => c.Id == customer.PartnerCountryId, ignoreGlobalFilter: true);

                sb += JoinNonNull(customer.PartnerAddress);
                sb += JoinNonNull(customer?.PartnerAddress2);
                sb += JoinNonNullWithoutCommaAfterZip(customer.PartnerZipCode?.ToString(), customer.PartnerCity, customer.PartnerState);
                sb += JoinNonNull( partnerCountry?.CountryName);
                sb += JoinNonNull(customer.PartnerVATNumber);
            }
            else
            {
                sb += JoinNonNull(customer.Address);
                sb += JoinNonNull(customer?.Address2);
                sb += JoinNonNullWithoutCommaAfterZip(customer.ZipCode.ToString(), customer.City, customer.State);
                sb += JoinNonNull(country.CountryName);
                sb += JoinNonNull(customer.VATNumber);
            }
            customerAddress = sb.ToString();

            return customerAddress;
        }

        public string JoinNonNull(params string[] values)
        {
            var nonNullValues = values.Where(v => !string.IsNullOrEmpty(v)).ToList();
            var newString = string.Join(", ", nonNullValues);
            return string.IsNullOrEmpty(newString) ? string.Empty : newString + Environment.NewLine;
        }

        public string JoinNonNullWithoutCommaAfterZip(string zipCode, string city, string state)
        {
            var result = zipCode;
            if (!string.IsNullOrEmpty(city))
            {
                result += " " + city;
            }
            if (!string.IsNullOrEmpty(state))
            {
                result += ", " + state;
            }
            return string.IsNullOrEmpty(result) ? string.Empty : result + Environment.NewLine;
        }


        private async Task CalculateAndSaveInvoiceDetails(Invoice invoice, List<SchedulerEventDetails> schedulerEventDetails, decimal daily, decimal vat, string description)
        {
            var quantity = schedulerEventDetails.Sum(c => c.Duration);
            if (quantity > 0)
            {
                var invoiceDetail = new InvoiceDetail
                {
                    InvoiceId = invoice.Id,
                    Description = description,
                    ReferenceName = "Timesheet",
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
                    ReferenceName = "Timesheet",
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

        private async Task<ApiResult<bool>> SendRequestInvoiceEmailToCustomer(string email, Invoice invoice, CompanyDto companyInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogError(" Sender Email is missing.");
                    return ApiResult<bool>.Success(false);
                }
                var amount = LocaleHelper.FormatCurrency(invoice.DueAmount);
                var subject = $"Your invoice from AddOptimization.";
                var link = GetInvoiceLinkForCustomer(invoice.Id);
                var contactName = string.IsNullOrEmpty(invoice?.Customer?.PartnerCompany) ? invoice?.Customer?.Organizations
    : invoice?.Customer?.PartnerCompany;

                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.UnpaidInvoice);
                _ = int.TryParse(invoice?.Customer?.PaymentClearanceDays.ToString(), out int clearanceDays);
                emailTemplate = emailTemplate
                                .Replace("[CustomerName]", invoice?.Customer?.ManagerName)
                                .Replace("[CompanyName]", invoice?.Customer?.Organizations)
                                .Replace("[Customer]", contactName)
                                .Replace("[InvoiceNumber]", invoice?.InvoiceNumber)
                                .Replace("[InvoiceDate]", LocaleHelper.FormatDate(invoice.InvoiceDate.Date))
                                .Replace("[TotalAmountDue]", LocaleHelper.FormatCurrency(invoice.DueAmount))
                                .Replace("[DueDate]", LocaleHelper.FormatDate(invoice.ExpiryDate.Value.Date))
                                .Replace("[LinkToInvoice]", link)
                                .Replace("[CompanyAccountingEmail]", companyInfo.AccountingEmail)
                                .Replace("[company]", companyInfo.CompanyName)
                                .Replace("[BankName]", companyInfo.BankName)
                                .Replace("[IbanNumber]", companyInfo.BankAccountNumber)
                                .Replace("[SwiftCode]", companyInfo.SwiftCode);

                var emailResult = await _emailService.SendEmail(email, subject, emailTemplate);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        private async Task<bool> SendInvoiceDeclinedEmailToAccountAdmins(IEnumerable<dynamic> accountAdmins, string accountContactName, string invoiceNumber, DateTime expiryDate, decimal totalAmountDue, string comment, Invoice invoice)
        {
            try
            {
                var customer = string.IsNullOrEmpty(invoice?.Customer?.PartnerCompany) ? invoice?.Customer?.Organizations
    : invoice?.Customer?.PartnerCompany;
                var subject = $"Invoice #{invoiceNumber} is Declined by {customer}.";
                var link = GetInvoiceLinkForCustomer(invoice.Id);
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.DeclinedInvoice);
                emailTemplate = emailTemplate.Replace("[AccountContactName]", accountContactName)
                                             .Replace("[InvoiceNumber]", invoiceNumber)
                                             .Replace("[TotalAmountDue]", LocaleHelper.FormatCurrency(totalAmountDue))
                                             .Replace("[ExpiryDate]", LocaleHelper.FormatDate(expiryDate))
                                             .Replace("[Customer]", customer)
                                              .Replace("[LinkToInvoice]", link)
                                             .Replace("[InvoiceDate]", LocaleHelper.FormatDate(invoice.InvoiceDate))
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
                entities = entities.Where(e => e.InvoiceNumber.ToLower().Contains(v.ToLower()) || e.InvoiceNumber.ToLower().Contains(v.ToLower()));
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
                    entities = entities.OrderByDescending(o => o.Id);
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

                string invoiceNumber;
                if (model.HasInvoiceFinalized)
                {
                    invoiceNumber = await GenerateInvoiceNumber();
                }
                else
                {
                    invoiceNumber = await GenerateDraftNumber();
                }
                var id = await _invoiceRepository.MaxAsync(e => (int)e.Id, ignoreGlobalFilter: true);

                Invoice entity = new Invoice
                {
                    Id = id + 1,
                    InvoiceNumber = invoiceNumber,
                    PaymentStatusId = paymentStatusId,
                    VatValue = model.InvoiceDetails.Sum(x => (x.UnitPrice * x.Quantity * x.VatPercent) / 100),
                    TotalPriceIncludingVat = model.InvoiceDetails.Sum(x => x.TotalPriceIncludingVat),
                    TotalPriceExcludingVat = model.InvoiceDetails.Sum(x => x.TotalPriceExcludingVat),
                    CustomerId = model.CustomerId,
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
                        ReferenceName = summary.ReferenceName,
                        Quantity = summary.Quantity,
                        VatPercent = summary.VatPercent,
                        UnitPrice = summary.UnitPrice,
                        TotalPriceExcludingVat = summary.TotalPriceExcludingVat,
                        TotalPriceIncludingVat = summary.TotalPriceIncludingVat,
                        Metadata = "Manual",
                        Description = summary.Description,

                    };

                    await _invoiceDetailRepository.InsertAsync(invoiceDetail);
                    entity.InvoiceDetails.Add(invoiceDetail);
                }
                InvoiceHistory historyEntity = new InvoiceHistory
                {
                    InvoiceId = entity.Id,
                    InvoiceStatusId = entity.InvoiceStatusId,
                    Comment = $"Draft Invoice [{invoiceNumber}] Generated",
                };
                await _invoiceHistoryRepository.InsertAsync(historyEntity);

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
                var entities = await _invoiceRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(x => x.Customer).Include(x => x.PaymentStatus).Include(x => x.InvoiceStatus), orderBy: x => x.OrderByDescending(x => x.CreatedAt) , ignoreGlobalFilter: true);
                entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
                entities = ApplyFilters(entities, filters);
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
                    HasCreditNotes = e.HasCreditNotes,
                    CreditNoteNumber = e.CreditNoteNumber,
                    HasInvoiceFinalized = e.HasInvoiceFinalized,
                    HasInvoiceSentToAccAdmin = e.HasInvoiceSentToAccAdmin,
                    CreatedAt = e.CreatedAt,
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
            .Include(e => e.CountryName), ignoreGlobalFilter: true);


                var model = new InvoiceResponseDto();
                var entity = await _invoiceRepository.FirstOrDefaultAsync(e => e.Id == id, ignoreGlobalFilter: true);
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
                model.CompanyBankDetails = entity.CompanyBankDetails;
                model.InvoiceStatusId = entity.InvoiceStatusId;
                model.InvoiceSentDate = entity.InvoiceSentDate;
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
                model.HasInvoiceFinalized = entity?.HasInvoiceFinalized;
                model.HasInvoiceSentToAccAdmin = entity?.HasInvoiceSentToAccAdmin;
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
                var eventStatus = (await _invoiceStatusService.Search()).Result;

                var draftId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.DRAFT.ToString()).Id;
                var declinedId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.DECLINED.ToString()).Id;


                var entity = await _invoiceRepository.FirstOrDefaultAsync(e => e.Id == id);
                if (entity == null)
                {
                    return ApiResult<InvoiceResponseDto>.NotFound("Invoice");
                }
                if (entity.InvoiceStatusId == declinedId)
                {
                    entity.InvoiceStatusId = draftId;
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
                        ReferenceName = summary.ReferenceName,
                        Quantity = summary.Quantity,
                        VatPercent = summary.VatPercent,
                        UnitPrice = summary.UnitPrice,
                        TotalPriceExcludingVat = summary.TotalPriceExcludingVat,
                        TotalPriceIncludingVat = summary.TotalPriceIncludingVat,
                        Metadata = summary.Metadata,
                        Description = summary.Description,
                    };
                    await _invoiceDetailRepository.InsertAsync(invoiceDetail);
                    newInvoiceDetails.Add(invoiceDetail);
                }
                InvoiceHistory historyEntity = new InvoiceHistory
                {
                    InvoiceId = entity.Id,
                    InvoiceStatusId = entity.InvoiceStatusId,
                };
                await _invoiceHistoryRepository.InsertAsync(historyEntity);

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
            return $"{baseUrl}invoice/approval/{encryptedId}?sidenav=collapsed";
        }

        public async Task<ApiResult<bool>> SendInvoiceToCustomer(int invoiceId, bool onlyEmail = false)
        {
            var entity = (await _invoiceRepository.QueryAsync(x => x.Id == invoiceId, include: entities => entities.Include(e => e.Customer))).FirstOrDefault();

            if (!onlyEmail)
            {
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var sentToCustomerStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.SEND_TO_CUSTOMER.ToString()).Id;
                entity.InvoiceStatusId = sentToCustomerStatusId;
                entity.InvoiceSentDate = DateTime.UtcNow;
                entity.ExpiryDate = entity.InvoiceSentDate.Value.AddDays(entity.PaymentClearanceDays.Value);
                await _invoiceRepository.UpdateAsync(entity);
            }
            InvoiceHistory historyEntity = new InvoiceHistory
            {
                InvoiceId = entity.Id,
                InvoiceStatusId = entity.InvoiceStatusId,
            };
            await _invoiceHistoryRepository.InsertAsync(historyEntity);
            var details = (await _invoiceDetailRepository.QueryAsync(x => x.InvoiceId == invoiceId)).ToList();
            var companyInfoResult = await _companyService.GetCompanyInformation();
            var companyInfo = companyInfoResult.Result;
            var invoices = (await GetInvoiceById(invoiceId)).Result;
            if (onlyEmail)
            {
                return await SendUnpaidInvoiceReminderEmailCustomer(invoices, companyInfo);
            }
            else
            {
                return await SendRequestInvoiceEmailToCustomer(entity.Customer.AccountContactEmail, entity, companyInfo);

            }
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
                var adminEmails = accountAdmins.Select(admin => new { Name = admin.FullName, Email = admin.Email });

                await SendInvoiceDeclinedEmailToAccountAdmins(adminEmails, entities.Customer.AccountContactName, entities.InvoiceNumber, entities.ExpiryDate.Value, entities.DueAmount, entity.Comment, entities);

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
            var eventStatus = (await _invoiceStatusService.Search()).Result;
            var invoiceStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusEnum.SEND_TO_CUSTOMER.ToString()).Id;

            var entity = await _invoiceRepository.QueryAsync(x => x.PaymentStatus.StatusKey == PaymentStatusesEnum.UNPAID.ToString() && !x.IsDeleted && x.InvoiceStatusId == invoiceStatusId, include: entities => entities.Include(e => e.Customer));

            if (entity == null || !entity.Any())
            {
                return ApiResult<List<InvoiceResponseDto>>.Failure(ValidationCodes.UnpaidInvoiceDoesNotExists);
            }
            var response = _mapper.Map<List<InvoiceResponseDto>>(entity);
            return ApiResult<List<InvoiceResponseDto>>.Success(response);
        }


        public async Task<ApiResult<List<InvoiceHistoryDto>>> GetInvoiceHistoryById(int id)
        {
            try
            {

                var entities = await _invoiceHistoryRepository.QueryAsync(e => e.InvoiceId == id, disableTracking: true);
                var mappedEntities = entities
            .OrderByDescending(e => e.CreatedAt).Select(e => new InvoiceHistoryDto
            {
                Id = e.Id,
                InvoiceId = e.InvoiceId,
                InvoiceStatusId = e.InvoiceStatusId,
                InvoiceStatusName = e.InvoiceStatus.Name,
                InvoiceNumber = e.Invoice.InvoiceNumber,
                Comment = e.Comment,
                CreatedAt = e.CreatedAt,
            }).ToList();

                return ApiResult<List<InvoiceHistoryDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        private async Task<ApiResult<bool>> SendUnpaidInvoiceReminderEmailCustomer(InvoiceResponseDto invoice, CompanyDto companyInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoice?.Customer?.AccountContactEmail))
                {
                    _logger.LogError("Recipient Email is missing.");
                    return ApiResult<bool>.Success(false);
                }

                var amount = LocaleHelper.FormatCurrency(invoice.DueAmount);
                var subject = $"Invoice #{invoice.InvoiceNumber} is pending for payment.";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.UnpaidInvoiceReminder);
                var customer = string.IsNullOrEmpty(invoice?.Customer?.PartnerCompany) ? invoice?.Customer?.Company : invoice?.Customer?.PartnerCompany;
                var link = GetInvoiceLinkForCustomer(invoice.Id);
                _ = int.TryParse(invoice?.PaymentClearanceDays.ToString(), out int clearanceDays);
                emailTemplate = emailTemplate
                                .Replace("[CustomerName]", invoice?.Customer?.ManagerName)
                                 .Replace("[AccountContactName]", invoice?.Customer?.AccountContactName)
                                 .Replace("[CompanyAccountingEmail]", companyInfo.AccountingEmail)
                                .Replace("[InvoiceNumber]", invoice?.InvoiceNumber)
                                .Replace("[Customer]", customer)
                                .Replace("[Company]", invoice?.Customer?.Company)
                                .Replace("[InvoiceDate]", LocaleHelper.FormatDate(invoice.InvoiceDate.Date))
                                .Replace("[TotalAmountDue]", LocaleHelper.FormatCurrency(invoice.DueAmount))
                                .Replace("[DueDate]", LocaleHelper.FormatDate(invoice.ExpiryDate.Value.Date))
                                .Replace("[LinkToInvoice]", link);
                var emailResult = await _emailService.SendEmail(invoice?.Customer?.AccountContactEmail, subject, emailTemplate);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while sending unpaid invoice reminder email to customer.");
                _logger.LogException(ex);
                throw;
            }
        }
        private async Task SendNotificationToAccountAdmin(List<InvoiceResponseDto> invoices, ApplicationUserDto accountAdmin)
        {
            var notifications = new List<NotificationDto>();
            foreach (var invoice in invoices)
            {
                var subject = $"Invoice created for {invoice?.Customer?.Company}";
                var bodyContent = $"Invoice created for {invoice?.Customer?.Company} with invoice number #{invoice?.InvoiceNumber}";
                var linkUrl = GetInvoiceLinkForCustomer(invoice.Id);
                var model = new NotificationDto
                {
                    Subject = subject,
                    Content = bodyContent,
                    Link = linkUrl,
                    AppplicationUserId = accountAdmin.Id,
                    GroupKey = $"Invoice created #{invoice?.Customer?.Company}",
                };
                notifications.Add(model);
            }
            await _notificationService.BulkCreateAsync(notifications);
        }
        public async Task<ApiResult<InvoiceResponseDto>> GetInvoiceById(int invoiceId)
        {
            try
            {
                var entity = await _invoiceRepository.FirstOrDefaultAsync(e => e.Id == invoiceId && !e.IsDeleted, include: entities => entities.Include(e => e.Customer));
                if (entity == null)
                {
                    return ApiResult<InvoiceResponseDto>.NotFound("Invoice");
                }

                var mappedEntity = _mapper.Map<InvoiceResponseDto>(entity);
                return ApiResult<InvoiceResponseDto>.Success(mappedEntity);
            }

            catch (Exception ex)
            {
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


        private async Task<string> GenerateInvoiceNumber()
        {
            try
            {
                var now = DateTime.UtcNow;
                var currentYear = now.Year;
                var currentMonth = now.Month;
                var currentDateFormat = $"{currentYear}{currentMonth:D2}";

                var finalizedInvoicesCount = (await _invoiceRepository.QueryAsync(x => (x.HasInvoiceFinalized == true) && x.InvoiceNumber.StartsWith(currentDateFormat), ignoreGlobalFilter: true)).Count();

                var invoiceNumber = finalizedInvoicesCount + 1;
                var finalizedInvoiceNumber = $"{DateTime.UtcNow:yyyyMM}{invoiceNumber}";

                return finalizedInvoiceNumber;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<ApiResult<InvoiceResponseDto>> FinalizedInvoice(int id)
        {
            try
            {

                var entity = await _invoiceRepository.FirstOrDefaultAsync(e => e.Id == id);
                if (entity == null)
                {
                    return ApiResult<InvoiceResponseDto>.NotFound("Invoice");
                }
                var eventStatus = (await _invoiceStatusService.Search()).Result;
                var readyToSendStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusesEnum.READY_TO_SEND.ToString()).Id;

                var invoiceNumber = await GenerateInvoiceNumber();
                entity.InvoiceNumber = invoiceNumber;
                entity.HasInvoiceFinalized = true;

                entity.InvoiceStatusId = readyToSendStatusId;
                await _invoiceRepository.UpdateAsync(entity);
                InvoiceHistory historyEntity = new InvoiceHistory
                {
                    InvoiceId = entity.Id,
                    InvoiceStatusId = entity.InvoiceStatusId,
                    Comment = "Invoice Finalized",
                };
                await _invoiceHistoryRepository.InsertAsync(historyEntity);
                var mappedEntity = _mapper.Map<InvoiceResponseDto>(entity);
                return ApiResult<InvoiceResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<bool>> Delete(int id)
        {
            try
            {
                var entity = await _invoiceRepository.FirstOrDefaultAsync(t => t.Id == id);
                entity.IsDeleted = true;
                await _invoiceRepository.UpdateAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<ApiResult<bool>> SendOverdueNotificationToAccountAdmin(InvoiceResponseDto invoice, List<ApplicationUserDto> accountAdmin)
        {
            var notifications = new List<NotificationDto>();
            var subject = $"Invoice overdue for {invoice?.Customer?.Company}";
            var bodyContent = $"Invoice overerdue for {invoice?.Customer?.Company} with invoice number #{invoice?.InvoiceNumber}";
            var linkUrl = GetInvoiceLinkForCustomer(invoice.Id);
            foreach (var admin in accountAdmin)
            {
                var model = new NotificationDto
                {
                    Subject = subject,
                    Content = bodyContent,
                    Link = linkUrl,
                    AppplicationUserId = admin.Id,
                    GroupKey = $"Invoice overdue #{invoice?.Customer?.Company}",
                };
                notifications.Add(model);
            }
            return await _notificationService.BulkCreateAsync(notifications);

        }
        private async Task<ApiResult<InvoiceResponseDto>> UpdateInvoiceNotificationSentToAccountAdmin(InvoiceResponseDto invoice)
        {
            var invoiceEntity = await _invoiceRepository.FirstOrDefaultAsync(e => e.Id == invoice.Id);
            invoiceEntity.HasInvoiceSentToAccAdmin = invoice.HasInvoiceSentToAccAdmin;
            await _invoiceRepository.UpdateAsync(invoiceEntity);
            return ApiResult<InvoiceResponseDto>.Success();
        }

        public async Task<bool> GetUnpaidInvoiceData(bool isNotification = false)
        {
            try
            {
                var invoices = (await GetUnpaidInvoicesForEmailReminder()).Result;
                if (invoices == null) return false;

                var companyInfoResult = (await _companyService.GetCompanyInformation()).Result;
                var customerEmployeeAssociation = (await _customerEmployeeAssociationService.Search()).Result;
                var approver = (await _applicationService.GetAccountAdmins()).Result;                

                foreach (var invoice in invoices)
                {
                    var paymentClearanceDays = invoice.PaymentClearanceDays;
                    if (invoice.ExpiryDate.HasValue && invoice.ExpiryDate.Value < DateTime.Today
                        && invoice?.DueAmount > 0)
                    {
                        if (isNotification)
                        {
                            var applicationUser = (await _applicationService.GetAccountAdmins()).Result;
                            await SendOverdueNotificationToAccountAdmin(invoice, applicationUser);
                        }
                        else
                        {
                            await SendUnpaidInvoiceReminderEmailCustomer(invoice, companyInfoResult);
                            if (invoice?.HasInvoiceSentToAccAdmin == false)
                            {
                                await SendOverdueNotificationToAccountAdmin(invoice, approver);
                                invoice.HasInvoiceSentToAccAdmin = true;
                                await UpdateInvoiceNotificationSentToAccountAdmin(invoice);
                            }
                        }
                    }
                };
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while getting unpaid invoice data for reminder email.");
                _logger.LogException(ex);
                return false;
            }
        }


    }
}

