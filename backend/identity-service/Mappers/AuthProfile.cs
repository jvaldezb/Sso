using System;
using AutoMapper;
using identity_service.Dtos.Auth;
using identity_service.Models;

namespace identity_service.Mappers;

public class AuthProfile: Profile
{
    public AuthProfile()
    {
        CreateMap<ApplicationUser, LoginDocumentDto>().ReverseMap();
        CreateMap<Menu, MenuDto>().ReverseMap();
    }

}
