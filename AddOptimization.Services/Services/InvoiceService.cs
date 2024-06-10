using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using Stripe;

namespace AddOptimization.Services.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IGenericRepository<Invoice> _invoiceRepository;
        private readonly IGenericRepository<Customer> _customer;
        private readonly IGenericRepository<CustomerEmployeeAssociation> _customerEmployeeAssociation;
        private readonly IGenericRepository<SchedulerEvent> _schedulersRepository;
        private readonly IGenericRepository<SchedulerEventDetails> _schedulersDetailsRepository;
        private readonly ISchedulersStatusService _schedulersStatusService;
        private readonly ISchedulerEventTypeService _schedulerEventTypeService;


        private readonly ILogger<InvoiceService> _logger;
        public InvoiceService(IGenericRepository<Invoice> invoiceRepository,
            IGenericRepository<Customer> customer,
            IGenericRepository<CustomerEmployeeAssociation> customerEmployeeAssociation,
            IGenericRepository<SchedulerEvent> schedulersRepository,
            IGenericRepository<SchedulerEventDetails> schedulersDetailsRepository,
            ISchedulersStatusService schedulersStatusService,
            ISchedulerEventTypeService schedulerEventTypeService,
            ILogger<InvoiceService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _customerEmployeeAssociation = customerEmployeeAssociation;
            _customer = customer;
            _schedulersRepository = schedulersRepository;
            _schedulerEventTypeService = schedulerEventTypeService;
            _schedulersStatusService = schedulersStatusService;
            _schedulersDetailsRepository = schedulersDetailsRepository;
            _logger = logger;
        }

        public async Task<ApiResult<List<InvoiceResponseDto>>> GenerateInvoice(Guid customerId, MonthDateRange month, List<CustomerEmployeeAssociationDto> associatedEmployees)
        {
            try
            {
                //first verify that invoice is already not created for this customer for this month
                var events = (await _schedulersRepository.QueryAsync(x => x.CustomerId == customerId && x.StartDate.Month == month.StartDate.Month)).ToList();

                //(first table) Invoice no, invoice date, customer address, created date, created by (null),company address, payment status,invoice status, company bank details etc
                //insert records

                //(second table) description, qty, unit price,vat, totalpriceincludingvat, totalpriceexcludingvat

                foreach (var employee in associatedEmployees)
                {
                    // first mon-fri billing for this employee excluding public holiday
                    // 1. filter public holiday for this customer based on countryid of this customer for this month
                    // 2. get the scheduler event details of this employee for a particular month, date should be mon-fri only excluding public holiday
                    // 3. filter based on event staus id so taht only timesheet info will come


                    //then sat sun for this employee
                    //1. get sat sun events for this employee for a particular month based on eventid
                    


                    //then overtime for this employee

                    //then public holiday for this employee

                }


                return ApiResult<List<InvoiceResponseDto>>.Success("");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
    }
}
