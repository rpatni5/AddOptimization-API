using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Services.Constants;
using AddOptimization.Services.Services;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Services;
using NPOI.SS.Formula.Eval;
using NPOI.SS.Formula.Functions;
using Sgbj.Cron;
using Stripe;
using System.Globalization;
using System.Text;

namespace AddOptimization.API.HostedService.BackgroundServices
{
    public class UnpaidInvoiceReminderToCustomerBackgroundService : BackgroundService
    {
        #region Private Variables
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LicenseRenewalEmailBackgroundService> _logger;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly CustomDataProtectionService _protectionService;
        #endregion

        #region Constructor
        public UnpaidInvoiceReminderToCustomerBackgroundService(IConfiguration configuration,
            ITemplateService templateService,
            IServiceProvider serviceProvider,
            CustomDataProtectionService protectionService,
            ILogger<LicenseRenewalEmailBackgroundService> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _templateService = templateService;
            _protectionService = protectionService;
        }
        #endregion

        #region Protected Methods
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //#if DEBUG
            //            return;
            //#endif
            try
            {
                _logger.LogInformation("ExecuteAsync Started.");
                using var timer = new CronTimer("0 12 */2 * *", TimeZoneInfo.Utc);
                while (!stoppingToken.IsCancellationRequested &&
                       await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Send Unpaid Invoice Reminder Email Background Service Started.");
                    await GetUnpaidInvoiceData();
                    _logger.LogInformation("Send Unpaid Invoice Reminder Email Background Service Completed.");
                }
                _logger.LogInformation("ExecuteAsync Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while executing UnpaidInvoiceReminderToCustomerBackgroundService.");
                _logger.LogException(ex);
            }
        }
        #endregion

        #region Private Methods        
        private async Task<bool> GetUnpaidInvoiceData()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                var customerEmployeeAssociationService = scope.ServiceProvider.GetRequiredService<ICustomerEmployeeAssociationService>();
                var appUserService = scope.ServiceProvider.GetRequiredService<IApplicationUserService>();
                var invoiceStatusService = scope.ServiceProvider.GetRequiredService<IInvoiceStatusService>();
                var companyService = scope.ServiceProvider.GetRequiredService<ICompanyService>();
                var invoiceHistoryService = scope.ServiceProvider.GetRequiredService<IGenericRepository<InvoiceHistory>>();
                var invoices = await invoiceService.GetUnpaidInvoicesForEmailReminder();
                if (invoices?.Result == null) return false;

                var companyInfoResult = (await companyService.GetCompanyInformation()).Result;
                var customerEmployeeAssociation = (await customerEmployeeAssociationService.Search()).Result;
                var approver = await appUserService.GetAccountAdmins();
                var eventStatus = (await invoiceStatusService.Search()).Result;
                var invoiceStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusEnum.SEND_TO_CUSTOMER.ToString()).Id;
                foreach (var invoice in invoices?.Result)
                {
                    var paymentClearanceDays = invoice.PaymentClearanceDays;
                    if (invoice.CreatedAt?.AddDays(paymentClearanceDays.Value) < DateTime.Today
                        && invoice?.DueAmount > 0 && invoice.InvoiceStatusId == invoiceStatusId)
                    {
                        await SendUnpaidInvoiceReminderEmailCustomer(invoice, companyInfoResult);
                        await SendUnpaidInvoiceReminderEmailAccountAdmin(invoice, approver.Result.FirstOrDefault(), companyInfoResult);

                        var historyEntity = new InvoiceHistory
                        {
                            InvoiceId = invoice.Id,
                            InvoiceStatusId = invoice.InvoiceStatusId,
                            CreatedAt = DateTime.UtcNow,
                            Comment = "Automatic Generated Unpaid Invoice",
                        };
                        await invoiceHistoryService.InsertAsync(historyEntity);
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

        private async Task<ApiResult<bool>> SendUnpaidInvoiceReminderEmailCustomer(InvoiceResponseDto invoice,  CompanyDto companyInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoice?.Customer?.AccountContactEmail))
                {
                    _logger.LogError("Recipient Email is missing.");
                    return ApiResult<bool>.Success(false);
                }

                var scope = _serviceProvider.CreateScope();
                var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var amount = LocaleHelper.FormatCurrency(invoice.DueAmount);
                var subject = $"AddOptimization invoice pending for {LocaleHelper.FormatDate(invoice.InvoiceDate.Date)} of {amount}";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.UnpaidInvoiceReminder);
                var link = GetInvoiceLinkForCustomer(invoice.Id);
                _ = int.TryParse(invoice?.PaymentClearanceDays.ToString(), out int clearanceDays);
                emailTemplate = emailTemplate
                                .Replace("[CustomerName]", invoice?.Customer?.ManagerName)
                                 .Replace("[AccountContactName]", invoice?.Customer?.AccountContactName)
                                 .Replace("[CompanyAccountingEmail]", companyInfo.AccountingEmail)
                                .Replace("[InvoiceNumber]", invoice?.InvoiceNumber.ToString())
                                .Replace("[CompanyName]", invoice?.Customer?.Company)
                                .Replace("[InvoiceDate]", LocaleHelper.FormatDate(invoice.InvoiceDate.Date))
                                .Replace("[TotalAmountDue]", LocaleHelper.FormatCurrency(invoice.DueAmount))
                                .Replace("[DueDate]",LocaleHelper.FormatDate(invoice.ExpiryDate.Date))
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
        private async Task<bool> SendUnpaidInvoiceReminderEmailAccountAdmin(InvoiceResponseDto invoice, ApplicationUserDto accountAdmin, CompanyDto companyInfo)
        {
            try
            {
                var scope = _serviceProvider.CreateScope();
                var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var amount =LocaleHelper.FormatCurrency(invoice.DueAmount);
                var subject = $"AddOptimization invoice pending for {invoice?.Customer?.ManagerName} dated {LocaleHelper.FormatDate(invoice.InvoiceDate.Date)} of {amount}";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.UnpaidInvoiceReminderAccountAdmin);
                var link = GetInvoiceLinkForAccountAdmin(invoice.Id);
                _ = int.TryParse(invoice?.PaymentClearanceDays.ToString(), out int clearanceDays);
                emailTemplate = emailTemplate
                                .Replace("[AccountAdminName]", accountAdmin.FullName)
                                .Replace("[AccountContactName]", invoice?.Customer?.AccountContactName)
                                .Replace("[CompanyName]", invoice?.Customer?.Company)
                                 .Replace("[CompanyAccountingEmail]", companyInfo.AccountingEmail)
                                .Replace("[InvoiceNumber]", invoice?.InvoiceNumber.ToString())
                                .Replace("[InvoiceDate]", LocaleHelper.FormatDate(invoice.InvoiceDate.Date))
                                .Replace("[TotalAmountDue]",LocaleHelper.FormatCurrency(invoice.DueAmount))
                                .Replace("[DueDate]", LocaleHelper.FormatDate(invoice.ExpiryDate.Date))
                                .Replace("[LinkToInvoice]", link);
                return await _emailService.SendEmail(accountAdmin.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while sending unpaid invoice reminder email to account admin.");
                _logger.LogException(ex);
                return false;
            }
        }
        public string GetInvoiceLinkForCustomer(long invoiceId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            var encryptedId = _protectionService.Encode(invoiceId.ToString());
            return $"{baseUrl}invoice/approval/{encryptedId}";
        }

        public string GetInvoiceLinkForAccountAdmin(long invoiceId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            return $"{baseUrl}admin/invoicing-management/add-invoice/{invoiceId}";
        }
        #endregion
    }
}
