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
            CreateMap<Customer, CustomerDto>().AfterMap((s, d) =>
            {
                d.Company = s.Organizations;
                d.CustomerStatusName = s.CustomerStatus?.Name;
                d.BillingAddressString = s.BillingAddress == null ? null : $"{s.BillingAddress.Address1},{s.BillingAddress.Zip},{s.BillingAddress.City}";
            });
            CreateMap<Customer, CustomerDetailsDto>().AfterMap((s, d) => {
            
            });
            CreateMap<CustomerCreateDto, Customer>().ForMember(c => c.Addresses, opt => opt.Ignore()).AfterMap((s,d) => {
                d.Organizations = s.Company;
            });
            CreateMap<CustomerStatus, CustomerStatusDto>();
            CreateMap<ScreenCreateDto, Screen>().AfterMap((s, d) =>
            {
                d.ScreenKey = s.Name.Trim().Replace(' ', '_').ToLower();
            });
            CreateMap<Field, FieldDto>(); 
            CreateMap<FieldDto, Field>();

            CreateMap<Customer, CustomerDto>().AfterMap((s, d) =>
            {
                d.Company = s.Organizations;
                d.BirthDay = string.IsNullOrEmpty(s.Birthday) ? s.Birthday : ( DateTime.Parse(s.Birthday)).ToString("yyyy-MM-dd");
                d.CustomerStatusName = s.CustomerStatus?.Name;
                d.BillingAddressString = s.BillingAddress == null ? null : $"{s.BillingAddress.Address1},{s.BillingAddress.Zip},{s.BillingAddress.City}";
            });
            CreateMap<Customer, CustomerDetailsDto>();
            CreateMap<CustomerCreateDto, Customer>()
                .ForMember(c => c.Addresses, opt => opt.Ignore());
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
            CreateMap<License,LicenseDetailsDto>();
            CreateMap<LicenseDevice, LicenseDeviceDto>();

        }
    }
}
