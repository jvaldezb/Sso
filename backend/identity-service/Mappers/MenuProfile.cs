using System;
using AutoMapper;
using identity_service.Dtos.Role;
using identity_service.Models;

namespace identity_service.Mappers;

public class MenuProfile : Profile
{
    public MenuProfile()
    {
        // Map MenuRole to MenuRoleDto
        CreateMap<Menu, MenuRoleDto>().ReverseMap();
        CreateMap<MenuRoleDto, MenuRoleRwxDto>().ReverseMap();
        CreateMap<MenuRoleBitPositionDto, MenuRoleRwxDto>().ReverseMap();
        CreateMap<Menu, MenuRoleRwxDto>().ReverseMap();
        CreateMap<Menu, MenuRoleRwxResponseDto>().ReverseMap();
        CreateMap<MenuRoleRwxDto, MenuRoleRwxRequestDto>().ReverseMap();
        CreateMap<Menu, MenuRoleBitPositionDto>().ReverseMap();
    }
}
