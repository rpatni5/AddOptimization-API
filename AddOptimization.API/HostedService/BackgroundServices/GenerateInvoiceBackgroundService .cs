﻿using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
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
        private readonly ITemplateService _templateService;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public GenerateInvoiceBackgroundService(IConfiguration configuration, ITemplateService templateService, IServiceProvider serviceProvider, ILogger<GenerateInvoiceBackgroundService> logger)
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
                using var timer = new CronTimer("0 6 * * *", TimeZoneInfo.Utc);
                while (!stoppingToken.IsCancellationRequested &&
                       await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Generate Invoice Background Service Started.");
                    await GenerateInvoice();
                    _logger.LogInformation("Generate Invoice Background Service Completed.");
                }
                _logger.LogInformation("ExecuteAsync Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An exception occurred while executing GenerateInvoiceBackgroundService.");
                _logger.LogException(ex);
            }
        }
        #endregion

        #region Private Methods        
        private async Task<bool> GenerateInvoice()
        {
            try
            {
                _logger.LogInformation("GenerateInvoice Started.");
                using var scope = _serviceProvider.CreateScope();
                var schedulerEventService = scope.ServiceProvider.GetRequiredService<ISchedulerEventService>();
                var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();
                var invoiceRepository = scope.ServiceProvider.GetRequiredService<IGenericRepository<Invoice>>();

                var customerEmployeeAssociationService = scope.ServiceProvider.GetRequiredService<ICustomerEmployeeAssociationService>();
                var expirationThresholdValue = _configuration.ReadSection<BackgroundServiceSettings>(AppSettingsSections.BackgroundServiceSettings).ExpirationThresholdInDays;


                var customerEmployeeAssociation = (await customerEmployeeAssociationService.Search()).Result;
                var customers = customerEmployeeAssociation.Select(c => c.CustomerId).Distinct().ToList();
                foreach (var id in customers)

                {

                    var associatedEmployees = customerEmployeeAssociation.Where(c => c.CustomerId == id && !c.IsDeleted).ToList();

                    var months = MonthDateRangeHelper.GetMonthDateRanges();

                    foreach (var month in months)
                    {
                        var filteredAssociations = associatedEmployees.Where(s => s.IsAutoInvoicingEnabled && ((s.CreatedAt.Value.Month <= month.StartDate.Month && s.CreatedAt.Value.Year == month.StartDate.Year) || s.CreatedAt.Value.Date < month.StartDate.Date)).ToList();
                        bool allTimesheetApprovedForMonth = await schedulerEventService.IsTimesheetApproved(id, filteredAssociations.Select(x => x.EmployeeId).ToList(), month);
                        if (allTimesheetApprovedForMonth && filteredAssociations.Any())
                        {
                            //check invoice already exist of not
                            //code pending
                            var invoice = (await invoiceRepository.QueryAsync(i => i.CustomerId == id && i.InvoiceDate.Month == month.StartDate.Month && i.InvoiceDate.Year == month.StartDate.Year && i.MetaData == "Timesheet")).ToList();
                            if (!invoice.Any())
                            {
                                await invoiceService.GenerateInvoice(id, month, filteredAssociations);
                            }
                            //create invoice for customer {customer.Id} for month {month}
                        }
                    }

                };
                _logger.LogInformation("GenerateInvoice Completed.");
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
