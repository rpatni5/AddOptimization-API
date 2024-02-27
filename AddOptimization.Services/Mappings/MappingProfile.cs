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
            CreateMap<ScreenCreateDto, Screen>().AfterMap((s, d) =>
            {
                d.ScreenKey = s.Name.Trim().Replace(' ', '_').ToLower();
            });
            CreateMap<Field, FieldDto>(); 
            CreateMap<FieldDto, Field>(); 
        }
    }
}
