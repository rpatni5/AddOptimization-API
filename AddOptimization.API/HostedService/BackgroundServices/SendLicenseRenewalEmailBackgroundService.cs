using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
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
                var groupedResult = licenses.Result.GroupBy(c => c.CustomerId);
                foreach (var group in groupedResult)
                {
                    var licensesDtoCollection = new List<LicenseDetailsDto>();
                    licensesDtoCollection.AddRange(group);
                    await SendCustomerLicenseRenewalEmail(licensesDtoCollection);                    
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

        private async Task<bool> SendCustomerLicenseRenewalEmail(List<LicenseDetailsDto> license)
        {
            try
            {
                var subject = "Add optimization renew license";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.RenewLicense);
                //TO DO: Dynamic HTML table records generation code
                emailTemplate = emailTemplate
                                .Replace("[CustomerName]", license.FirstOrDefault().CustomerName)
                                .Replace("[LicenseKey]", license.FirstOrDefault().LicenseKey)
                                .Replace("[NoOfDevices]", license.FirstOrDefault().NoOfDevices.ToString())
                                .Replace("[ExpirationDate]", license.FirstOrDefault().ExpirationDate.ToString());
                return await _emailService.SendEmail(license.FirstOrDefault().CustomerEmail, subject, emailTemplate);
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
