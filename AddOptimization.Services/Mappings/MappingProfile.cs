using AutoMapper;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Dto;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AddOptimization.Services.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            CreateMap<ApplicationUser, UserSummaryDto>();
            CreateMap<ApplicationUser, ApplicationUserDto>();
            CreateMap<UserCreateDto, ApplicationUser>().ForMember(dst => dst.Password, opt => opt.Ignore());
            CreateMap<RoleCreateDto, Role>();
            CreateMap<Role, RoleDto>();
            CreateMap<Screen, ScreenDto>();
            CreateMap<ScreenDto, Screen>();
            CreateMap<Screen, ScreenCreateDto>();

            CreateMap<Customer, CustomerDetailsDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<CustomerCreateDto, Customer>().ForMember(c => c.Addresses, opt => opt.Ignore()).AfterMap((s, d) =>
            {
                d.Organizations = s.Company;
            });
            CreateMap<CustomerStatus, CustomerStatusDto>();
            CreateMap<ScreenCreateDto, Screen>().AfterMap((s, d) =>
            {
                d.ScreenKey = s.Name.Trim().Replace(' ', '_').ToLower();
            });
            CreateMap<Field, FieldDto>();
            CreateMap<FieldDto, Field>();

            CreateMap<Field, FieldCreateDto>();
            CreateMap<FieldCreateDto, Field>();

            CreateMap<Customer, CustomerDto>().AfterMap((s, d) =>
            {
                d.Company = s.Organizations;
                d.CustomerStatusName = s.CustomerStatus?.Name;
                d.BillingAddressString = s.BillingAddress == null ? null : $"{s.BillingAddress.Address1},{s.BillingAddress.Zip},{s.BillingAddress.City}";
            });
            CreateMap<CustomerDto, Customer>().AfterMap((s, d) =>
            {
                d.Organizations = s.Company;
            });
            CreateMap<Customer, CustomerDetailsDto>();
            CreateMap<CustomerStatus, CustomerStatusDto>();

            CreateMap<AddressCreateDto, Address>().AfterMap((s, d) =>
            {
                d.Country = s.Country;
                d.CountryCode = s.CountryCode;
                d.ProvinceCode = s.Province;
            });
            CreateMap<Address, AddressDto>().AfterMap((s, d) =>
            {
                d.CreatedAt = s.CreatedAt?.Date;
            });

            CreateMap<LicenseCreateDto, License>();
            CreateMap<LicenseUpdateDto, License>();
            CreateMap<License, LicenseDetailsDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser?.FullName;
            });
            CreateMap<LicenseDevice, LicenseDeviceDto>().AfterMap((s, d) =>
            {
                d.CreatedAt = s.CreatedAt?.Date;
                d.CreatedBy = s.CreatedByUser?.FullName;
            });
            CreateMap<LicenseDeviceManagementDto, LicenseDevice>();

            CreateMap<GuiVersion, GuiVersionResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedAt = s.CreatedAt?.Date;
                d.CreatedBy = s.CreatedByUser?.FullName;
            });


            CreateMap<PublicHoliday, PublicHolidayResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CountryName = s.Country?.CountryName ?? string.Empty;
            });
            CreateMap<PublicHolidayRequestDto, PublicHoliday>();

            CreateMap<Country, CountryDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
            });
            CreateMap<CountryDto, Country>();

            CreateMap<SchedulerEvent, SchedulerEventDetailsDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                // d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;

            });
            CreateMap<SchedulerEventDetailsDto, SchedulerEvent>();



            CreateMap<SchedulerStatus, SchedulerStatusDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;

            });
            CreateMap<SchedulerStatusDto, SchedulerStatus>();



            CreateMap<SchedulerEventType, SchedulerEventTypeDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;

            });
            CreateMap<SchedulerEventTypeDto, SchedulerEventType>();


            CreateMap<SchedulerEventRequestDto, SchedulerEvent>().AfterMap((s, d) =>
            {

            });

            CreateMap<SchedulerEvent, SchedulerEventResponseDto>().AfterMap((s, d) =>
            {
                d.ApprovarName = s.Approvar != null ? s.Approvar.FullName : string.Empty;
                d.UserName = s.ApplicationUser != null ? s.ApplicationUser.FullName : string.Empty;
                d.CustomerName = s.Customer != null ? s.Customer.Organizations : string.Empty;
                d.ManagerName = s.Customer != null ? s.Customer.ManagerName : string.Empty;
                d.AdminStatusName = s.AdminStatus != null ? s.AdminStatus.Name : string.Empty;
                d.AdminStatusKey = s.AdminStatus != null ? s.AdminStatus.StatusKey : string.Empty;
                d.UserStatusName = s.UserStatus != null ? s.UserStatus.Name : string.Empty;
                d.UserStatusKey = s.UserStatus != null ? s.UserStatus.StatusKey : string.Empty;
            });


            CreateMap<SchedulerEventDetailsDto, SchedulerEventDetails>().AfterMap((s, d) =>
            {

            });
            CreateMap<SchedulerEventDetails, SchedulerEventDetailsDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;

            });

            CreateMap<AbsenceRequest, AbsenceRequestResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.LeaveStatusName = s.LeaveStatuses != null ? s.LeaveStatuses.Name : string.Empty;
                d.UserName = s.ApplicationUser != null ? s.ApplicationUser.FullName : string.Empty;

            });
            CreateMap<AbsenceRequestRequestDto, AbsenceRequest>();


            CreateMap<LeaveStatuses, LeaveStatusesDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
            });
            CreateMap<LeaveStatusesDto, LeaveStatuses>();

            CreateMap<CustomerEmployeeAssociationDto, CustomerEmployeeAssociation>();

            CreateMap<CustomerEmployeeAssociation, CustomerEmployeeAssociationDto>().AfterMap((s, d) =>
            {
                d.ApproverName = s.Approver != null ? s.Approver.FullName : string.Empty;
                d.CustomerName = s.Customer != null ? s.Customer.Organizations : string.Empty;
                d.ManagerName = s.Customer != null ? s.Customer.ManagerName : string.Empty;
                d.EmployeeName = s.ApplicationUser != null ? s.ApplicationUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.CreatedBy = s.CreatedByUser?.FullName;
                d.UpdatedAt = s.UpdatedAt?.Date;
                d.UpdatedBy = s.UpdatedByUser?.FullName;
                d.HasContract = s.Contracts != null && s.Contracts.Count > 0;
                d.PublicHolidayCountry = s.Country != null ? s.Country.CountryName : string.Empty;
            });

            CreateMap<HolidayAllocation, HolidayAllocationResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.UserName = s.ApplicationUser != null ? s.ApplicationUser.FullName : string.Empty;

            });
            CreateMap<HolidayAllocationRequestDto, HolidayAllocation>();

            CreateMap<Product, ProductResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
            });
            CreateMap<ProductRequestDto, Product>();

            CreateMap<Employee, EmployeeDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.UserName = s.ApplicationUser != null ? s.ApplicationUser.FullName : string.Empty;
                d.Email = s.ApplicationUser != null ? s.ApplicationUser.Email : string.Empty;
                d.FirstName = s.ApplicationUser != null ? s.ApplicationUser.FirstName : string.Empty;
                d.LastName = s.ApplicationUser != null ? s.ApplicationUser.LastName : string.Empty;
                d.Password = s.ApplicationUser != null ? s.ApplicationUser.Password : string.Empty;
                d.isActive = s.ApplicationUser != null ? s.ApplicationUser.IsActive : false;
                d.CountryName = s.Country != null ? s.Country.CountryName : string.Empty;
                d.IdentityName = s.EmployeeIdentity != null ? s.EmployeeIdentity.Name : string.Empty;

            });
            CreateMap<EmployeeDto, Employee>();
            CreateMap<EmployeeIdentity, EmployeeIdentityDto>();

            CreateMap<Company, CompanyDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
                d.CountryName = s.CountryName != null ? s.CountryName.CountryName : string.Empty;
            });
            CreateMap<CompanyDto, Company>();

            CreateMap<Quote, QuoteResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
                d.CustomerName = s.Customer != null ? s.Customer.Organizations : string.Empty;
            });
            CreateMap<QuoteRequestDto, Quote>();

            CreateMap<QuoteStatuses, QuoteStatusDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<QuoteStatusDto, QuoteStatuses>();

            CreateMap<QuoteSummary, QuoteSummaryDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<QuoteSummaryDto, QuoteSummary>();

            CreateMap<SchedulerEventHistory, SchedulerEventHistoryDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt;
                d.UpdatedAt = s.UpdatedAt?.Date;

            });
            CreateMap<SchedulerEventHistoryDto, SchedulerEventHistory>();


            CreateMap<Invoice, InvoiceResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
                d.CustomerName = s.Customer != null ? s.Customer.Organizations : string.Empty;
            });
            CreateMap<InvoiceRequestDto, Invoice>();

            CreateMap<InvoiceStatus, InvoiceStatusDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<InvoiceStatusDto, InvoiceStatus>();

            CreateMap<InvoiceDetail, InvoiceDetailDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<InvoiceDetailDto, InvoiceDetail>();

            CreateMap<PaymentStatus, PaymentStatusDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<PaymentStatusDto, PaymentStatus>();

            CreateMap<ExternalInvoice, ExternalInvoiceResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
            });
            CreateMap<ExternalInvoiceRequestDto, ExternalInvoice>();

            CreateMap<ExternalInvoiceDetail, ExternalInvoiceDetailDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<ExternalInvoiceDetailDto, InvoiceDetail>();

            CreateMap<InvoicePaymentHistory, InvoicePaymentHistoryDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
            });
            CreateMap<InvoicePaymentHistoryDto, InvoicePaymentHistory>();

            CreateMap<InvoicePaymentHistory, InvoiceAmountPaymentDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<InvoiceAmountPaymentDto, InvoicePaymentHistory>();


            CreateMap<ExternalInvoicePaymentHistory, ExternalInvoicePaymentHistoryDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
            });
            CreateMap<ExternalInvoicePaymentHistoryDto, ExternalInvoicePaymentHistory>();

            CreateMap<ExternalInvoicePaymentHistory, ExternalInvoiceAmountDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<ExternalInvoiceAmountDto, ExternalInvoicePaymentHistory>();

            CreateMap<SettingDto, Setting>();
            CreateMap<Setting, SettingDto>();

            CreateMap<InvoicingPaymentMode, InvoicingPaymentModeDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<InvoicingPaymentModeDto, InvoicingPaymentMode>();

            CreateMap<EmployeeContract, EmployeeContractResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
                d.CustomerName = s.Customer != null ? s.Customer.Organizations : string.Empty;
                d.EmployeeName = s.ApplicationUser != null ? s.ApplicationUser.FullName : string.Empty;
                d.InvoicingAndPaymentMode = s.InvoicingPaymentMode != null ? s.InvoicingPaymentMode.Name : string.Empty;
                d.ProjectFeePaymentMode = s.ProjectFeePaymentMode != null ? s.ProjectFeePaymentMode.Name : string.Empty;
                d.IdentityName = s.EmployeeIdentity != null ? s.EmployeeIdentity.Name : string.Empty;

            });
            CreateMap<EmployeeContractRequestDto, EmployeeContract>();

            CreateMap<InvoiceCreditNotes, InvoiceCreditNoteDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
            });
            CreateMap<InvoiceCreditNoteDto, InvoiceCreditNotes>();

            CreateMap<InvoiceCreditNotes, InvoiceCreditPaymentDto>().AfterMap((s, d) =>
            {

            });
            CreateMap<InvoiceCreditPaymentDto, InvoiceCreditNotes>();
            CreateMap<TemplateFolder, TemplateFolderDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
            });
            CreateMap<TemplateFolderDto, TemplateFolder>();

            CreateMap<Template, TemplateDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
            });
            CreateMap<TemplateDto, Template>();

            CreateMap<SavedSearch, SavedSearchDto>().AfterMap((s, d) =>
            {
            });
            CreateMap<SavedSearchDto, SavedSearch>();
            CreateMap<Notification, NotificationDto>().AfterMap((s, d) => d.Meta = s.Meta ?? "{}");
            CreateMap<string, EntryDataDto>().ConvertUsing(json => JsonSerializer.Deserialize<EntryDataDto>(json, jsonOptions));


            CreateMap<TemplateEntries, TemplateEntryDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
                d.EntryData = s.EntryData == null ? new EntryDataDto() : JsonSerializer.Deserialize<EntryDataDto>(s.EntryData, jsonOptio);
            });

            CreateMap<NotificationDto, Notification>();
            CreateMap<NotificationUserDto, ApplicationUser>();
            CreateMap<ApplicationUser, NotificationUserDto>();
            CreateMap<TemplateEntryDto, TemplateEntries>().AfterMap((s, d) =>
            {
                d.EntryData = s.EntryData != null ? JsonSerializer.Serialize(s.EntryData, jsonOptions) : string.Empty;
            });

            CreateMap<CreditCardDto, EntryDataDto>();

            CreateMap<Group, GroupDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
            });
            CreateMap<GroupDto, Group>();

            CreateMap<SharedEntry, SharedEntryResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
            });
            CreateMap<SharedEntryRequestDto, SharedEntry>();

            CreateMap<Group, GroupDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
            });
            CreateMap<CombineGroupModelDto, Group>();

            CreateMap<GroupMember, GroupMemberDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.UpdatedAt = s.UpdatedAt?.Date;
            });
            CreateMap<CombineGroupModelDto, GroupMember>();
        }
    }
}