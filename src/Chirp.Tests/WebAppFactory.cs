using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System;

namespace Chirp.Tests;

public class WebAppFactory : WebApplicationFactory<Chirp.Web.Program>, IDisposable
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove app's DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChirpDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // create a single persistent in-memory SQLite connection
            _connection = new SqliteConnection("Data Source=:memory:;");
            _connection.Open();

            services.AddDbContext<ChirpDbContext>(o => o.UseSqlite(_connection));

            // Ensure database and seed
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
            db.Database.EnsureCreated();
            DbInitializer.SeedDatabase(db);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            try
            {
                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }
            catch (Exception) { /* swallow on dispose */ }
        }
    }
}