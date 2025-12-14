using System;
using AutoMapper;
using identity_service.Dtos.Role;
using identity_service.Dtos.User;
using identity_service.Models;

namespace identity_service.Mappers;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        // Map ApplicationRole to RoleDto
        CreateMap<ApplicationRole, RoleDto>();

        // Map RoleDto to ApplicationRole
        CreateMap<RoleDto, ApplicationRole>();

        // Map CreateRoleDto to ApplicationRole
        CreateMap<CreateRoleDto, ApplicationRole>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()))
            .ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.SystemId));

        // Map UpdateRoleDto to ApplicationRole
        CreateMap<UpdateRoleDto, ApplicationRole>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()))
            .ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.SystemId));

        // Map ApplicationRole to UserRoleDto
        CreateMap<ApplicationRole, UserRoleDto>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.SystemId));    
    }
}
