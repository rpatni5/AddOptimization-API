using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;

namespace AddOptimization.Services.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IGenericRepository<Invoice> _invoiceRepository;
        private readonly IGenericRepository<Customer> _customer;
        private readonly IGenericRepository<CustomerEmployeeAssociation> _customerEmployeeAssociation;
        private readonly IGenericRepository<SchedulerEvent> _schedulerEventRepository;
        private readonly IGenericRepository<SchedulerEventDetails> _schedulersDetailsRepository;
        private readonly ISchedulersStatusService _schedulersStatusService;
        private readonly ISchedulerEventTypeService _schedulerEventTypeService;


        private readonly ILogger<InvoiceService> _logger;
        public InvoiceService(IGenericRepository<Invoice> invoiceRepository,
            IGenericRepository<Customer> customer,
            IGenericRepository<CustomerEmployeeAssociation> customerEmployeeAssociation,
            IGenericRepository<SchedulerEvent> schedulerEventRepository,
            IGenericRepository<SchedulerEventDetails> schedulersDetailsRepository,
            ISchedulersStatusService schedulersStatusService,
            ISchedulerEventTypeService schedulerEventTypeService,
            ILogger<InvoiceService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _customerEmployeeAssociation = customerEmployeeAssociation;
            _customer = customer;
            _schedulerEventRepository = schedulerEventRepository;
            _schedulerEventTypeService = schedulerEventTypeService;
            _schedulersStatusService = schedulersStatusService;
            _schedulersDetailsRepository = schedulersDetailsRepository;
            _logger = logger;
        }

        public async Task<ApiResult<List<InvoiceResponseDto>>> GenerateInvoice()
        {
            try
            {
                var a = new List<InvoiceResponseDto>();
                var customers = await _customer.QueryAsync(c => c.CustomerStatus.Name == CustomerStatuses.Active);
                foreach (var customer in customers.ToList())
                {
                    var vat = customer.VAT;
                    var employeeAssociation = (await _customerEmployeeAssociation.QueryAsync(c => c.CustomerId == c.CustomerId && !c.IsDeleted)).ToList();
                    foreach (var ceItem in employeeAssociation)
                    {
                        var daily = ceItem.DailyWeightage;
                        var overTime = ceItem.Overtime;
                        var publicHoliday = ceItem.PublicHoliday;
                        var saturday = ceItem.Saturday;
                        var sunday = ceItem.Sunday;

                        var schedulerEvents = (await _schedulerEventRepository.QueryAsync(c => c.UserId == ceItem.EmployeeId && c.CustomerId == ceItem.CustomerId && !c.IsDraft && !c.IsDeleted)).ToList();
                        foreach (var schedulerEvent in schedulerEvents)// Months
                        {
                            //Days
                            var eventDetails = (await _schedulersDetailsRepository.QueryAsync(c => c.SchedulerEventId == schedulerEvent.Id && !c.IsDeleted)).ToList();
                            var grouped = eventDetails.GroupBy(c => c.EventTypeId);

                        };

                    };
                };
                return ApiResult<List<InvoiceResponseDto>>.Success(a);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}
