using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using Sgbj.Cron;
using System.Globalization;

namespace AddOptimization.API.HostedService.BackgroundServices
{
    public class FillTimesheetReminderEmailBackgroundService : BackgroundService
    {
        #region Private Variables
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LicenseRenewalEmailBackgroundService> _logger;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public FillTimesheetReminderEmailBackgroundService(IConfiguration configuration, ITemplateService templateService, IServiceProvider serviceProvider, ILogger<LicenseRenewalEmailBackgroundService> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _templateService = templateService;

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
                using var timer = new CronTimer("0 4 * * *", TimeZoneInfo.Utc);
                while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Send Fill Timesheet Reminder Email Background Service Started.");
                    await GetNotFilledTimesheetData();
                    _logger.LogInformation("Send Fill Timesheet Reminder Email Background Service Completed.");
                }
                _logger.LogInformation("ExecuteAsync Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while executing FillTimesheetReminderEmailBackgroundService.");
                _logger.LogException(ex);
            }
        }
        #endregion

        #region Private Methods        
        private async Task<bool> GetNotFilledTimesheetData()
        {
            _logger.LogInformation("GetNotFilledTimesheetData Started.");
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var schedulerEventService = scope.ServiceProvider.GetRequiredService<ISchedulerEventService>();
                var customerEmployeeAssociationService = scope.ServiceProvider.GetRequiredService<ICustomerEmployeeAssociationService>();
                var expirationThresholdValue = _configuration.ReadSection<BackgroundServiceSettings>(AppSettingsSections.BackgroundServiceSettings).ExpirationThresholdInDays;
                var customerEmployeeAssociation = await customerEmployeeAssociationService.Search();
                var result = customerEmployeeAssociation.Result.GroupBy(c => c.CustomerId).ToList();
                foreach (var clientAssociation in result)
                {
                    foreach (var association in clientAssociation)
                    {
                        var schedulerEvents = await schedulerEventService.GetSchedulerEventsForEmailReminder(association.CustomerId, association.EmployeeId);
                        if (schedulerEvents?.Result == null) continue;

                        //Filter scheduler events which happened before current month of the client employee association.

                        var events = schedulerEvents.Result
                            .Where(s => (association.CreatedAt.Value.Month <= s.StartDate.Month && association.CreatedAt.Value.Year == s.StartDate.Year) || association.CreatedAt.Value.Date < s.StartDate.Date).ToList();
                        foreach (var item in events)
                        {
                            await SendFillTimesheetReminderEmail(item);
                        };
                    }
                }
                _logger.LogInformation("GetNotFilledTimesheetData Completed.");
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
                var subject = "AddOptimization timesheet submission reminder.";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.FillTimesheetReminder);
                var link = GetMyTimesheetLinkForEmployee();
                emailTemplate = emailTemplate
                                .Replace("[EmployeeName]", schedulerEvent?.UserName)
                                .Replace("[StartDate]", schedulerEvent?.StartDate.Date.ToString("dd/M/yyyy", CultureInfo.InvariantCulture))
                                .Replace("[EndDate]", schedulerEvent?.EndDate.Date.ToString("dd/M/yyyy", CultureInfo.InvariantCulture))
                                .Replace("[LinkToMyTimesheet]", link);
                return await _emailService.SendEmail(schedulerEvent?.ApplicationUser?.Email, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while sending employee fill timesheet reminder email.");
                _logger.LogException(ex);
                return false;
            }
        }

        private string GetMyTimesheetLinkForEmployee()
        {
            var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
            return $"{baseUrl}admin/timesheets/my-timesheets";
        }
        #endregion
    }
}
