using System;
using identity_service.Models;

namespace identity_service.Repositories.Interfaces;

public interface IUserSessionRepository: IBaseRepository<UserSession>
{
    Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(string userId);
}
