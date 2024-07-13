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
            using var timer = new CronTimer("0 8 */2 * *", TimeZoneInfo.Local);
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

                foreach (var invoice in invoices?.Result)
                {
                    var customerEmployeeAssociation = (await customerEmployeeAssociationService.Search()).Result;
                    var approverId = customerEmployeeAssociation.Where(c => c.CustomerId == invoice.CustomerId && !c.IsDeleted).FirstOrDefault().ApproverId;// Account admin must be similar for multiple employee association.
                    var approver = appUserService.GetAccountAdmins();
                    await SendFillTimesheetReminderEmail(invoice);
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

        private async Task<bool> SendFillTimesheetReminderEmail(InvoiceResponseDto invoice)
        {
            try
            {
                return true;
                //var scope = _serviceProvider.CreateScope();
                //var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                //var subject = "AddOptimization approve timesheet reminder";
                //var emailTemplate = _templateService.ReadTemplate(EmailTemplates.ApproveTimesheetReminder);
                //var link = GetMyTimesheetLinkForCustomer(invoice.Id);
                //emailTemplate = emailTemplate
                //                .Replace("[CustomerName]", invoice?.Customer.Name)
                //                .Replace("[EmployeeName]", invoice?.ApplicationUser.FullName)
                //                .Replace("[StartDate]", invoice?.StartDate.Date.ToString("d"))
                //                .Replace("[EndDate]", invoice?.EndDate.Date.ToString("d"))
                //                .Replace("[LinkToApproveTimesheet]", link);
                //return await _emailService.SendEmail(invoice?.Customer?.ManagerEmail, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while sending customer approve timesheet reminder email.");
                _logger.LogException(ex);
                return false;
            }
        }
        public string GetMyTimesheetLinkForCustomer(Guid schedulerEventId)
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            var encryptedId = _protectionService.Encode(schedulerEventId.ToString());
            return $"{baseUrl}timesheet/approval/{encryptedId}";
        }
        #endregion
    }
}
