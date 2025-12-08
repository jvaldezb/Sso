using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AutoMapper;
using identity_service.Data;
using identity_service.Dtos.Role;
using identity_service.Models;
using identity_service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Services;

/// <summary>
/// Encodes/decodes RWX-like permissions into a compressed BigInteger using
/// Menu.Module as module name and Menu.BitPosition as the bit index.
/// Each module uses 3 bits by default.
/// </summary>
public class RoleClaimEncoderService : IRoleClaimEncoderService
{
    private const int BITS_PER_MODULE = 3;
    private static readonly BigInteger MASK = (BigInteger)((1 << BITS_PER_MODULE) - 1);
    private readonly IMapper _mapper;

    public RoleClaimEncoderService(IMapper mapper)
    {        
        _mapper = mapper;
    }

    public async Task<BigInteger> EncodeAsync(List<MenuRoleRwxDto> permissions)
    {
        var total = BigInteger.Zero;

        foreach (var m in permissions)
        {
            if (!m.BitPosition.HasValue || !m.RwxValue.HasValue)
                continue;

            var shift = BITS_PER_MODULE * m.BitPosition.Value;
            var perm = (BigInteger)(m.RwxValue.Value & (int)MASK);
            total |= perm << shift;
        }

        return total;
    }

    public async Task<List<MenuRoleRwxDto>> DecodeAsync(List<MenuRoleBitPositionDto> menus, string value)
    {        
        var menuRoleDecodedDtos = new List<MenuRoleRwxDto>();

        if (string.IsNullOrWhiteSpace(value))
            return menuRoleDecodedDtos;

        if (!BigInteger.TryParse(value, out var total))
            total = BigInteger.Zero;

        menuRoleDecodedDtos = _mapper.Map<List<MenuRoleRwxDto>>(menus);

        foreach (var m in menus)
        {
            var idx = m.BitPosition;
            var shift = BITS_PER_MODULE * idx;
            var extracted = (int)((total >> shift) & MASK);
            var mrd = menuRoleDecodedDtos.First(x => x.Id == m.Id);
            mrd.RwxValue = extracted;            
        }

        return menuRoleDecodedDtos;
    }
}
