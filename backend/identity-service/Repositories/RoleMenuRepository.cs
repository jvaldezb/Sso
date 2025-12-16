using System;
using identity_service.Data;
using identity_service.Models;
using identity_service.Repositories.Interfaces;

namespace identity_service.Repositories;

public class RoleMenuRepository : BaseRepository<RoleMenu>, IRoleMenuRepository
{
    public RoleMenuRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
