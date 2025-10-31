using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chirp.Tests;

public class WebAppFactory : WebApplicationFactory<Chirp.Web.Program>, IDisposable
{
    private readonly SqliteConnection _connection;

    public WebAppFactory()
    {
        // Randomized, shared in-memory database name to avoid collisions in parallel test runs
        var dbName = $"ChirpTestDb_{Guid.NewGuid()}";
        _connection = new SqliteConnection($"Data Source={dbName};Mode=Memory;Cache=Shared");
        _connection.Open(); // must remain open for database to persist
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration from the app
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChirpDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Register ChirpDbContext with the shared in-memory SQLite connection
            services.AddDbContext<ChirpDbContext>(options => options.UseSqlite(_connection));

            // Build the service provider and initialize the database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();

            db.Database.EnsureCreated();
            DbInitializer.SeedDatabase(db);
        });

        // Optionally silence all logging output to keep test runs clean
        builder.ConfigureLogging(logging => logging.ClearProviders());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
