using System;
using AutoMapper;
using identity_service.Dtos.RefreshToken;
using identity_service.Models;

namespace identity_service.Mappers;

public class RefreshTokenProfile: Profile
{
    public RefreshTokenProfile()
    {
        CreateMap<RefreshToken, RefreshTokenDto>()
        .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
        .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => src.ExpiresAt))
        .ForMember(dest => dest.RevokedAt, opt => opt.MapFrom(src => src.RevokedAt));
        
    }
}
