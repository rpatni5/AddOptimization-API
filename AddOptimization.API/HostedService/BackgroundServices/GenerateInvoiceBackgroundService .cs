using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Models;
using GraphQL.Types;
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
        private readonly ICustomerService _customerService;
        #endregion

        #region Constructor
        public GenerateInvoiceBackgroundService(IConfiguration configuration, IEmailService emailService, ITemplateService templateService, IServiceProvider serviceProvider, ILogger<GenerateInvoiceBackgroundService> logger, ICustomerService customerService)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _emailService = emailService;
            _templateService = templateService;
            _customerService = customerService;
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
                var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                var customerEmployeeAssociationService = scope.ServiceProvider.GetRequiredService<ICustomerEmployeeAssociationService>();
                var expirationThresholdValue = _configuration.ReadSection<BackgroundServiceSettings>(AppSettingsSections.BackgroundServiceSettings).ExpirationThresholdInDays;

                var customers = (await _customerService.GetAllCustomers()).Result;

                foreach (var customer in customers)
                {
                    var customerEmployeeAssociation = (await customerEmployeeAssociationService.Search()).Result;

                    var associatedEmployees = customerEmployeeAssociation.Where(c => c.CustomerId == c.CustomerId && !c.IsDeleted).ToList();

                    var months = MonthDateRangeHelper.GetMonthDateRanges();

                    foreach (var month in months)
                    {
                        var filteredAssociations = associatedEmployees.Where(s => month.StartDate.Month >= s.CreatedAt.Value.Month).ToList();

                        bool allTimesheetAproovedForMonth = await schedulerEventService.IsTimesheetApproved(customer.Id, filteredAssociations.Select(x => x.EmployeeId).ToList(), month);
                        if (allTimesheetAproovedForMonth)
                        {
                            //create timehseet for customer {customer.Id} for month {month}
                            await invoiceService.GenerateInvoice(customer.Id, month, filteredAssociations);
                        }
                    }

                };

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while generating invoice for customers.");
                _logger.LogException(ex);
                return false;
            }
        }

        #endregion
    }
}
