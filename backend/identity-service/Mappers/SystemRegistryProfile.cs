using System;
using AutoMapper;
using identity_service.Dtos.SystemRegistry;
using identity_service.Models;

namespace identity_service.Mappers;

public class SystemRegistryProfile : Profile
{
    public SystemRegistryProfile()
    {
        // Map SystemRegistry to SystemRegistryDto
        CreateMap<SystemRegistry, SystemRegistryDto>();

        // Map SystemRegistryDto to SystemRegistry
        CreateMap<SystemRegistryDto, SystemRegistry>();

        // Map CreateSystemRegistryDto to SystemRegistry
        CreateMap<CreateSystemRegistryDto, SystemRegistry>();

        // Map UpdateSystemRegistryDto to SystemRegistry
        CreateMap<UpdateSystemRegistryDto, SystemRegistry>();
    }
}
