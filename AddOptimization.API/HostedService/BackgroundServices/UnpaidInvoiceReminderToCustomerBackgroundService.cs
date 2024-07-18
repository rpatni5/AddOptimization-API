using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Services;
using NPOI.SS.Formula.Functions;
using Sgbj.Cron;
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
            _logger.LogInformation("ExecuteAsync Started.");
            using var timer = new CronTimer("* * * * *", TimeZoneInfo.Local);
            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Send Unpaid Invoice Reminder Email Background Service Started.");
                await GetUnpaidInvoiceData();
                _logger.LogInformation("Send Unpaid Invoice Reminder Email Background Service Completed.");
            }
            _logger.LogInformation("ExecuteAsync Completed.");
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

                var invoices = await invoiceService.GetUnpaidInvoicesForEmailReminder();
                if (invoices?.Result == null) return false;

                var customerEmployeeAssociation = (await customerEmployeeAssociationService.Search()).Result;
                var approver = await appUserService.GetAccountAdmins();
                var approverEmail = approver.Result.FirstOrDefault().Email;
                foreach (var invoice in invoices?.Result)
                {
                    var paymentClearanceDays = invoice.Customer.PaymentClearanceDays;
                    if (invoice.InvoiceDate.AddDays(paymentClearanceDays.Value) <= DateTime.Today)
                    {                        
                       // var approverId = customerEmployeeAssociation.Where(c => c.CustomerId == invoice.CustomerId && !c.IsDeleted).FirstOrDefault().ApproverId;// Account admin must be similar for multiple employee association.                       
                        await SendFillTimesheetReminderEmail(invoice, approverEmail);
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

        private async Task<bool> SendFillTimesheetReminderEmail(InvoiceResponseDto invoice, string approverEmail)
        {
            try
            {
                var scope = _serviceProvider.CreateScope();
                var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var subject = "AddOptimization unpaid invoice reminder";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.UnpaidInvoiceReminder);
                var link = GetInvoiceLinkForCustomer(invoice.Id);
                _ = int.TryParse(invoice?.Customer?.PaymentClearanceDays.ToString(), out int clearanceDays);
                emailTemplate = emailTemplate
                                .Replace("[CustomerName]", invoice?.Customer?.ManagerName)
                                .Replace("[InvoiceNumber]", invoice?.InvoiceNumber.ToString())
                                .Replace("[InvoiceDate]", invoice?.InvoiceDate.Date.ToString("d"))
                                .Replace("[TotalAmountDue]", invoice?.DueAmount.ToString())
                                .Replace("[DueDate]", invoice?.InvoiceDate.AddDays(clearanceDays).Date.ToString("d"))
                                .Replace("[LinkToInvoice]", link);
                return await _emailService.SendEmail(invoice?.Customer?.ManagerEmail, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while sending customer approve timesheet reminder email.");
                _logger.LogException(ex);
                return false;
            }
        }
        public string GetInvoiceLinkForCustomer(long invoiceId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            var encryptedId = _protectionService.Encode(invoiceId.ToString());
            return $"{baseUrl}timesheet/approval/{encryptedId}";
        }
        #endregion
    }
}
