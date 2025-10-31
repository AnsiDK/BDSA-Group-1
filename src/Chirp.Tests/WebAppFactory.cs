using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chirp.Tests;

public class WebAppFactory : WebApplicationFactory<Chirp.Web.Program>, IDisposable
{
    private readonly SqliteConnection _connection;

    public WebAppFactory()
    {
        // Keep the same in-memory database alive for the factory’s lifetime
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChirpDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Use the shared connection
            services.AddDbContext<ChirpDbContext>(o => o.UseSqlite(_connection));

            // Build the service provider and seed the database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
            db.Database.EnsureCreated();  // Creates tables if they don't exist
            DbInitializer.SeedDatabase(db);  // Seed once
        });
    }

    // Properly dispose of the shared connection
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
