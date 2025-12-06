using System;
using Microsoft.EntityFrameworkCore;
using identity_service.Data;

namespace identity_service.UnitTests.Fixtures;

public static class InMemoryDbFixture
{
    public static TestDbContext CreateNewContext()
    {
        var dbName = $"test_db_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var ctx = new TestDbContext(options);
        return ctx;
    }
}
