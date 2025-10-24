using System;
using System.IO;
using System.Linq;
using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace Chirp.LocalServer;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _host;
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"chirp_integration_{Guid.NewGuid():N}.db");

    public TestWebApplicationFactory()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChirpDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ChirpDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_dbPath}");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
            db.Database.EnsureDeleted();
            db.Database.Migrate();
            DbInitializer.SeedDatabase(db);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _host = base.CreateHost(builder);
        return _host;
    }

    public IHost GetHost() => _host ?? throw new InvalidOperationException("Host has not been created yet.");

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
