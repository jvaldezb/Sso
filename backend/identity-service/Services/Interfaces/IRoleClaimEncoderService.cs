using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using identity_service.Dtos.Role;

namespace identity_service.Services.Interfaces;

public interface IRoleClaimEncoderService
{
    Task<BigInteger> EncodeAsync(List<MenuRoleRwxDto> permissions);
    Task<List<MenuRoleRwxDto>> DecodeAsync(List<MenuRoleBitPositionDto> menuRoleDtos, string value);    
}
