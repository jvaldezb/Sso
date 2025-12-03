using System;

namespace identity_service.Dtos.User;

public class LogoutRequestDto
{
    public string? JwtId { get; set; }   // Si es null entonces cierra todas las sesiones
}
