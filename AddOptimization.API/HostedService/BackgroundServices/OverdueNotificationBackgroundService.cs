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
    public class OverdueNotificationBackgroundService: BackgroundService
    {

        #region Private Variables
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueNotificationBackgroundService> _logger;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly CustomDataProtectionService _protectionService;
        #endregion

        #region Constructor
        public OverdueNotificationBackgroundService(IConfiguration configuration,
            ITemplateService templateService,
            IServiceProvider serviceProvider,
            CustomDataProtectionService protectionService,
            ILogger<OverdueNotificationBackgroundService> logger)
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
            try
            {
                _logger.LogInformation("ExecuteAsync Started.");
                using var timer = new CronTimer("0 0 */5 * *", TimeZoneInfo.Utc);
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
                var applicationUserService = scope.ServiceProvider.GetRequiredService<IApplicationUserService>();
                var invoiceStatusService = scope.ServiceProvider.GetRequiredService<IInvoiceStatusService>();
                var companyService = scope.ServiceProvider.GetRequiredService<ICompanyService>();
                var invoiceHistoryService = scope.ServiceProvider.GetRequiredService<IGenericRepository<InvoiceHistory>>();
                var invoices =( await invoiceService.GetUnpaidInvoicesForEmailReminder()).Result;
                if (invoices== null) return false;

                var companyInfoResult = (await companyService.GetCompanyInformation()).Result;
                var customerEmployeeAssociation = (await customerEmployeeAssociationService.Search()).Result;
                var applicationUser = (await applicationUserService.GetAccountAdmins()).Result;
                var eventStatus = (await invoiceStatusService.Search()).Result;
                var invoiceStatusId = eventStatus.FirstOrDefault(x => x.StatusKey == InvoiceStatusEnum.SEND_TO_CUSTOMER.ToString()).Id;
                foreach (var invoice in invoices)
                {
                    var paymentClearanceDays = invoice.PaymentClearanceDays;
                    if (invoice.ExpiryDate.HasValue && invoice.ExpiryDate.Value < DateTime.Today
                        && invoice?.DueAmount > 0 && invoice.InvoiceStatusId == invoiceStatusId)
                    {
                        await invoiceService.SendOverdueNotificationToAccountAdmin(invoices, applicationUser);

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
        #endregion
    }
}