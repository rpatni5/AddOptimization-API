﻿using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using AddOptimization.Utilities.Services;
using NPOI.SS.Formula.Functions;
using Sgbj.Cron;
using System.Text;

namespace AddOptimization.API.HostedService.BackgroundServices
{
    public class PendingTimesheetReminderToCustomerBackgroundService : BackgroundService
    {
        #region Private Variables
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LicenseRenewalEmailBackgroundService> _logger;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly CustomDataProtectionService _protectionService;

        #endregion

        #region Constructor
        public PendingTimesheetReminderToCustomerBackgroundService(IConfiguration configuration,
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
                using var timer = new CronTimer("0 10 */2 * *", TimeZoneInfo.Utc);
                while (!stoppingToken.IsCancellationRequested &&
                       await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Send Approve Pending Timesheet Reminder Email Background Service Started.");
                    await GetNotApprovedTimesheetData();
                    _logger.LogInformation("Send Approve Pending Timesheet Reminder Email Background Service Completed.");
                }
                _logger.LogInformation("ExecuteAsync Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while executing PendingTimesheetReminderToCustomerBackgroundService.");
                _logger.LogException(ex);
            }
        }
        #endregion

        #region Private Methods        
        private async Task<bool> GetNotApprovedTimesheetData()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var schedulerEventService = scope.ServiceProvider.GetRequiredService<ISchedulerEventService>();
                var schedulerEvents = await schedulerEventService.GetSchedulerEventsForApproveEmailReminder();
                if (schedulerEvents?.Result == null) return false;

                foreach (var item in schedulerEvents?.Result)
                {
                    await SendFillTimesheetReminderEmail(item);
                };
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while getting scheduler event data for fill timesheet reminder email.");
                _logger.LogException(ex);
                return false;
            }
        }

        private async Task<bool> SendFillTimesheetReminderEmail(SchedulerEventResponseDto schedulerEvent)
        {
            try
            {
                var scope = _serviceProvider.CreateScope();
                var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var subject = "AddOptimization approve timesheet reminder.";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.ApproveTimesheetReminder);
                var link = GetMyTimesheetLinkForCustomer(schedulerEvent.Id);
                emailTemplate = emailTemplate
                                .Replace("[AdministrationContactName]", schedulerEvent?.Customer.AdministrationContactName)
                                .Replace("[EmployeeName]", schedulerEvent?.ApplicationUser.FullName)
                                .Replace("[StartDate]", LocaleHelper.FormatDate(schedulerEvent.StartDate.Date))
                                .Replace("[EndDate]", LocaleHelper.FormatDate(schedulerEvent.EndDate.Date))
                                .Replace("[LinkToApproveTimesheet]", link);
                return await _emailService.SendEmail(schedulerEvent?.Customer?.AdministrationContactEmail, subject, emailTemplate);
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
