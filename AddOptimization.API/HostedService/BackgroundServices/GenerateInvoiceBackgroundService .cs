using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using Sgbj.Cron;

namespace AddOptimization.API.HostedService.BackgroundServices
{
    public class GenerateInvoiceBackgroundService : BackgroundService
    {
        #region Private Variables
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GenerateInvoiceBackgroundService> _logger;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public GenerateInvoiceBackgroundService(IConfiguration configuration, IEmailService emailService, ITemplateService templateService, IServiceProvider serviceProvider, ILogger<GenerateInvoiceBackgroundService> logger)
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
            _logger.LogInformation("ExecuteAsync Started.");
            using var timer = new CronTimer("0 8 * * *", TimeZoneInfo.Local);
            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Generate Invoice Background Service Started.");
                await GenerateInvoice();
                _logger.LogInformation("Generate Invoice Background Service Completed.");
            }
            _logger.LogInformation("ExecuteAsync Completed.");
        }
        #endregion

        #region Private Methods        
        private async Task<bool> GenerateInvoice()
        {
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

                        //Filter scheduler events which happened before the client association.
                        var events = schedulerEvents.Result
                            .Where(s => s.EventDetails != null && (s.EventDetails == null && s.EndDate <= association.CreatedAt)).ToList();
                        foreach (var item in events)
                        {
                            //Task.Run(() => SendFillTimesheetReminderEmail(item));
                        };
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while generating invoice for customers.");
                _logger.LogException(ex);
                return false;
            }
        }

        //private async Task<bool> SendFillTimesheetReminderEmail(SchedulerEventResponseDto schedulerEvent)
        //{
        //    try
        //    {
        //        var subject = "Add optimization timesheet submission reminder";
        //        var emailTemplate = _templateService.ReadTemplate(EmailTemplates.FillTimesheetReminder);
        //        var link = GetMyTimesheetLinkForEmployee();
        //        emailTemplate = emailTemplate
        //                        .Replace("[EmployeeName]", schedulerEvent?.UserName)
        //                        .Replace("[StartDate]", schedulerEvent?.StartDate.Date.ToString("d"))
        //                        .Replace("[EndDate]", schedulerEvent?.EndDate.Date.ToString("d"))
        //                        .Replace("[LinkToMyTimesheet]", link);
        //        return await _emailService.SendEmail(schedulerEvent?.ApplicationUser?.Email, subject, emailTemplate);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogInformation("An exception occurred while sending employee fill timesheet reminder email.");
        //        _logger.LogException(ex);
        //        return false;
        //    }
        //}

        //private string GetMyTimesheetLinkForEmployee()
        //{
        //    var baseUrl = (_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl);
        //    return $"{baseUrl}admin/timesheets/my-timesheets";
        //}
        #endregion
    }
}
