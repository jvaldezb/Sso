using System;
using AutoMapper;
using identity_service.Dtos.User;
using identity_service.Models;

namespace identity_service.Mappers;

public class UserProfile: Profile
{
    public UserProfile()
    {
        // Mapeo de User a UserDto y viceversa        
        CreateMap<ApplicationUser, UserResponseDto>().ReverseMap();
        CreateMap<ApplicationUser, UserForCreateDto>().ReverseMap();
    }
}
