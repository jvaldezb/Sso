using System;
using identity_service.Models;

namespace identity_service.Services.Interfaces;

public interface ITokenGenerator
{
    /// <summary>
    /// Genera JWT central (Fase A). Debe devolver (token, expires, jti).
    /// </summary>
    (string Token, DateTime Expires) GenerateCentralToken(ApplicationUser user, UserSession session, IEnumerable<string> systems, string scope, int minutesValid);

    /// <summary>
    /// Genera JWT para sistema (Fase B). Debe devolver (token, expires, jti).
    /// </summary>
    (string Token, DateTime Expires) GenerateSystemToken(ApplicationUser user, UserSession session, IEnumerable<string> roles, string systemName, string scope, int minutesValid);
}
