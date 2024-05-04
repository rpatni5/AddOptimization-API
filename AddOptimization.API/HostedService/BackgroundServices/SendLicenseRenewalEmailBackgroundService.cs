using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;

namespace AddOptimization.API.HostedService.BackgroundServices
{
    public class SendLicenseRenewalEmailBackgroundService : BackgroundService
    {
        #region Private Variables
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SendLicenseRenewalEmailBackgroundService> _logger;
        private readonly IEmailService _emailService;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public SendLicenseRenewalEmailBackgroundService(IConfiguration configuration, IEmailService emailService, ITemplateService templateService, IServiceProvider serviceProvider, ILogger<SendLicenseRenewalEmailBackgroundService> logger)
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
            var durationValue = _configuration.ReadSection<BackgroundServiceSettings>(AppSettingsSections.BackgroundServiceSettings).RenewLicenseEmailTriggerDurationInSeconds;
            var period = TimeSpan.FromSeconds(durationValue);
            using PeriodicTimer timer = new PeriodicTimer(period);
            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Send License Renewal Email BackgroundTask Started.");
                await GetCustomersWithLicensesExpiringSoon();
                _logger.LogInformation("Send License Renewal Email BackgroundTask Completed.");
            }
        }
        #endregion

        #region Private Methods        
        private async Task<bool> GetCustomersWithLicensesExpiringSoon()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var _license = scope.ServiceProvider.GetRequiredService<ILicenseService>();
                var expirationThresholdValue = _configuration.ReadSection<BackgroundServiceSettings>(AppSettingsSections.BackgroundServiceSettings).ExpirationThresholdInDays;
                var expirationThreshold = DateTime.Today.AddDays(expirationThresholdValue);
                var licenses = await _license.GetAllLicenseForRenewalNotification(expirationThreshold);
                foreach (var item in licenses.Result)
                {
                    await SendCustomerLicenseRenewalEmail(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while getting customer license for renewal email.");
                _logger.LogException(ex);
                return false;
            }
        }

        private async Task<bool> SendCustomerLicenseRenewalEmail(LicenseDetailsDto license)
        {
            try
            {
                var subject = "Add optimization renew license";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.RenewLicense);
                emailTemplate = emailTemplate
                                .Replace("[CustomerName]", license.CustomerName)
                                .Replace("[LicenseKey]", license.LicenseKey)
                                .Replace("[NoOfDevices]", license.NoOfDevices.ToString())
                                .Replace("[ExpirationDate]", license.ExpirationDate.ToString());
                return await _emailService.SendEmail(license.CustomerEmail, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while sending customer license renewal email.");
                _logger.LogException(ex);
                return false;
            }
        }
        #endregion
    }
}
