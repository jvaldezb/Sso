using System;
using identity_service.Data;
using identity_service.Models;
using Microsoft.EntityFrameworkCore;

namespace identity_service.UnitTests.Fixtures;

public class TestDbContext: AppDbContext
{
    public TestDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignorar la propiedad problematica SOLO en tests
        modelBuilder.Entity<AuthAuditLog>()
            .Ignore(x => x.Details);
    }
}