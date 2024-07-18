using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using Sgbj.Cron;
using System.Text;

namespace AddOptimization.API.HostedService.BackgroundServices
{
    public class LicenseRenewalEmailBackgroundService : BackgroundService
    {
        #region Private Variables
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LicenseRenewalEmailBackgroundService> _logger;
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public LicenseRenewalEmailBackgroundService(IConfiguration configuration, ITemplateService templateService, IServiceProvider serviceProvider, ILogger<LicenseRenewalEmailBackgroundService> logger)
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
            _logger.LogInformation("ExecuteAsync Started.");
            using var timer = new CronTimer("0 7 * * *", TimeZoneInfo.Utc);
            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Send License Renewal Email BackgroundTask Started.");
                await GetCustomersWithLicensesExpiringSoon();
                _logger.LogInformation("Send License Renewal Email BackgroundTask Completed.");
            }
            _logger.LogInformation("ExecuteAsync Completed.");
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
                var scope = _serviceProvider.CreateScope();
                var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var subject = "AddOptimization renew license";
                var emailTemplate = _templateService.ReadTemplate(EmailTemplates.RenewLicense);
                string[] tableHeaders = { "S.No", "LicenseKey", "NoOfDevices", "ExpirationDate" };
                var table = GenerateHtmlTable(tableHeaders, license);
                emailTemplate = emailTemplate
                                .Replace("[CustomerLicensesData]", table)
                                .Replace("[CustomerName]", license.FirstOrDefault().CustomerName);
                return await _emailService.SendEmail(license.FirstOrDefault().CustomerEmail, subject, emailTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while sending customer license renewal email.");
                _logger.LogException(ex);
                return false;
            }
        }

        public static string GenerateHtmlTable(string[] headers, List<LicenseDetailsDto> license)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<table class=\"body-action\" align=\"center\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\">");
            sb.AppendLine("<tr>");
            // Generate table headers
            foreach (string header in headers)
            {
                switch (header)
                {
                    case "S.No":
                        sb.AppendFormat("<th width=\"1\">{0}</th>", header);
                        break;
                    case "LicenseKey":
                        sb.AppendFormat("<th width=\"97\">{0}</th>", "License Key");
                        break;
                    case "NoOfDevices":
                        sb.AppendFormat("<th width=\"1\">{0}</th>", "No Of Devices");
                        break;
                    case "ExpirationDate":
                        sb.AppendFormat("<th width=\"1\">{0}</th>", "Expiration Date");
                        break;
                    default:
                        break;
                }
            }
            sb.AppendLine("</tr>");

            // Generate table data rows
            var sNoCount = 1;
            foreach (var rowData in license)
            {
                sb.AppendLine("<tr>");
                foreach (var cellData in headers)
                {
                    if (cellData == "S.No")
                    {
                        sb.AppendFormat("<td>{0}</td>", sNoCount);
                    }
                    if (cellData == nameof(rowData.LicenseKey))
                    {
                        sb.AppendFormat("<td>{0}</td>", rowData.LicenseKey);
                    }
                    if (cellData == nameof(rowData.NoOfDevices))
                    {
                        sb.AppendFormat("<td>{0}</td>", rowData.NoOfDevices);
                    }
                    if (cellData == nameof(rowData.ExpirationDate))
                    {
                        sb.AppendFormat("<td>{0}</td>", rowData.ExpirationDate);
                    }
                }
                sb.AppendLine("</tr>");
                sNoCount++;
            }

            sb.AppendLine("</table>");
            return sb.ToString();
        }
        #endregion
    }
}
