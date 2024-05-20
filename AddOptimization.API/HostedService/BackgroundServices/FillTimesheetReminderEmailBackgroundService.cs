using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;

namespace AddOptimization.API.HostedService.BackgroundServices
{
    public class FillTimesheetReminderEmailBackgroundService : BackgroundService
    {
        #region Private Variables
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LicenseRenewalEmailBackgroundService> _logger;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public FillTimesheetReminderEmailBackgroundService(IConfiguration configuration, IEmailService emailService, ITemplateService templateService, IServiceProvider serviceProvider, ILogger<LicenseRenewalEmailBackgroundService> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _emailService = emailService;
            _templateService = templateService;

        }
        #endregion

        #region Protected Methods
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
//#if DEBUG
//            return;
//#endif
            var durationValue = _configuration.ReadSection<BackgroundServiceSettings>(AppSettingsSections.BackgroundServiceSettings).FillTimesheetReminderEmailTriggerDurationInSeconds;
            var period = TimeSpan.FromSeconds(durationValue);
            using PeriodicTimer timer = new PeriodicTimer(period);
            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Send Fill Timesheet Reminder Email Background Service Started.");
                await GetNotFilledTimesheetData();
                _logger.LogInformation("Send Fill Timesheet Reminder Email Background Service Completed.");
            }
        }
        #endregion

        #region Private Methods        
        private async Task<bool> GetNotFilledTimesheetData()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var schedulerEventService = scope.ServiceProvider.GetRequiredService<ISchedulerEventService>();
                var customerEmployeeAssociationService = scope.ServiceProvider.GetRequiredService<ICustomerEmployeeAssociationService>();
                var expirationThresholdValue = _configuration.ReadSection<BackgroundServiceSettings>(AppSettingsSections.BackgroundServiceSettings).ExpirationThresholdInDays;
                var customerEmployeeAssociation = await customerEmployeeAssociationService.Search();
                var result = customerEmployeeAssociation.Result.GroupBy(c => c.Id).ToList();
                foreach (var client in result)
                {
                    foreach (var employee in client)
                    {
                        var schedulerEvents = await schedulerEventService.GetSchedulerEventsForEmailReminder(employee.Id, employee.EmployeeId);
                        if (schedulerEvents?.Result == null) continue;

                        //Filter scheduler events which happened before the client association.
                        var events = schedulerEvents.Result.Where(s => s.StartDate <= employee.CreatedAt).ToList();
                        foreach (var item in events)
                        {
                            Task.Run(() => SendFillTimesheetReminderEmail(item));
                        };
                    }
                }
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
                var subject = "Add optimization timesheet submission reminder";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.FillTimesheetReminder);
                var link = GetMyTimesheetLinkForEmployee();
                emailTemplate = emailTemplate
                                .Replace("[EmployeeName]", schedulerEvent?.UserName)
                                .Replace("[StartDate]", schedulerEvent?.StartDate.Date.ToString("d"))
                                .Replace("[EndDate]", schedulerEvent?.EndDate.Date.ToString("d"))
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
