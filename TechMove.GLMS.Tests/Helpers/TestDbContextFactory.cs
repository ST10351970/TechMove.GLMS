using Microsoft.EntityFrameworkCore;
using TechMove.GLMS.Core.Data;

namespace TechMove.GLMS.Tests.Helpers;

/// <summary>
/// ApplicationDbContext backed by EF Core's InMemory provider.
/// Each test gets a fresh database name (Guid) so tests are isolated.
/// </summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}