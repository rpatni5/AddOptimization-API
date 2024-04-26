using AutoMapper;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Dto;
using System.Text.Json;

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


            CreateMap<PublicHoliday, PublicHolidayDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
            });
            CreateMap<PublicHolidayDto, PublicHoliday>();

            CreateMap<Country, CountryDto>().AfterMap((s, d) =>
            {
                d.CreatedBy = s.CreatedByUser != null ? s.CreatedByUser.FullName : string.Empty;
                d.UpdatedBy = s.UpdatedByUser != null ? s.UpdatedByUser.FullName : string.Empty;
            });
            CreateMap<CountryDto, Country>();
        }
    }
}
