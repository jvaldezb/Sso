using System;
using AutoMapper;

namespace identity_service.Mappers;

public class UserSessionProfile: Profile
{
    public UserSessionProfile()
    {
        // Mapeo de UserSession a UserSessionDto y viceversa
        CreateMap<Models.UserSession, Dtos.UserSession.UserSessionDto>().ReverseMap();
    }

}
