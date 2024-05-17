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
                d.Birthday = s.Birthday != DateTime.MinValue ? s.Birthday.ToString("yyyy-MM-dd") : null;
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
                d.BirthDay = string.IsNullOrEmpty(s.Birthday) ? s.Birthday : (DateTime.Parse(s.Birthday)).ToString("yyyy-MM-dd");
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


            CreateMap<ClientRequestDto, Client>().AfterMap((s, d) =>
            {
                d.Organization = s.Company;
            });

            CreateMap<Client, ClientResponseDto>().AfterMap((s, d) =>
            {
                d.CreatedAt = s.CreatedAt?.Date;
                d.CreatedBy = s.CreatedByUser?.FullName;
                d.UpdatedAt = s.UpdatedAt?.Date;
                d.UpdatedBy = s.UpdatedByUser?.FullName;
            });

            CreateMap<Client, ClientResponseDto>().AfterMap((s, d) =>
            {
                d.Company = s.Organization;
            });


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
                d.ClientName = s.Client != null ? $"{s.Client.FirstName} {s.Client.LastName}" : string.Empty;
                d.AdminStatusName = s.AdminStatus != null ? s.AdminStatus.Name : string.Empty;
                d.UserStatusName = s.UserStatus != null ? s.UserStatus.Name : string.Empty;
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
                d.UserName=s.ApplicationUser!=null ? s.ApplicationUser.FullName : string.Empty;

            });
            CreateMap<AbsenceRequestRequestDto, AbsenceRequest>();


            CreateMap<LeaveStatuses, LeaveStatusesDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
            });
            CreateMap<LeaveStatusesDto, LeaveStatuses>();

            CreateMap<ClientEmployeeAssociationDto, ClientEmployeeAssociation>();

            CreateMap<ClientEmployeeAssociation, ClientEmployeeAssociationDto>().AfterMap((s, d) =>
            {
                d.ApproverName = s.Approver != null ? s.Approver.FullName : string.Empty;
                d.ClientName = s.Client != null ? $"{s.Client.FirstName} {s.Client.LastName}" : string.Empty;
                d.EmployeeName = s.ApplicationUser != null ? s.ApplicationUser.FullName : string.Empty;
                d.CreatedAt = s.CreatedAt?.Date;
                d.CreatedBy = s.CreatedByUser?.FullName;
                d.UpdatedAt = s.UpdatedAt?.Date;
                d.UpdatedBy = s.UpdatedByUser?.FullName;
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

        }
    }
}


