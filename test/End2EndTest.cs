using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Chirp.Infrastructure.Data;
using Xunit;
using Xunit.Abstractions;

namespace Chirp.E2E;

// Shared fixture
public class TestFixture : IDisposable
{
    private readonly CustomWebAppFactory? _factory;
    public HttpClient Client { get; }

    public TestFixture()
    {
        var external = Environment.GetEnvironmentVariable("TEST_BASEURL");
        if (!string.IsNullOrWhiteSpace(external))
        {
            Client = new HttpClient { BaseAddress = new Uri(external) };
        }
        else
        {
            // Use in-process hosting with SQLite in-memory database
            _factory = new CustomWebAppFactory();
            Client = _factory.CreateClient();
        }
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory?.Dispose();
    }

    // Custom factory that wires up EF Core to an in-memory SQLite database
    private sealed class CustomWebAppFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Ensure the app runs under the 'Testing' environment so Program.cs can skip migrations
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove the app's ChirpDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ChirpDbContext>));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                // Keep a single open connection for the lifetime of the factory
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<ChirpDbContext>(options =>
                    options.UseSqlite(_connection));

                // Build the service provider, create the database schema, and seed minimal test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();

                // EnsureCreated avoids migrations and the __EFMigrationsHistory table
                db.Database.EnsureCreated();

                // Seed only what's needed for tests to run
                if (!db.Authors.Any() && !db.Cheeps.Any())
                {
                    var helge = new Chirp.Core.Entities.Author { Name = "Helge", Email = "ropf@itu.dk" };
                    var adrian = new Chirp.Core.Entities.Author { Name = "Adrian", Email = "adho@itu.dk" };

                    db.Authors.AddRange(helge, adrian);
                    db.SaveChanges();

                    db.Cheeps.AddRange(
                        new Chirp.Core.Entities.Cheep
                        {
                            Text = "Hello, BDSA students!",
                            Timestamp = DateTime.UtcNow,
                            AuthorId = helge.AuthorId
                        },
                        new Chirp.Core.Entities.Cheep
                        {
                            Text = "Hej, velkommen til kurset.",
                            Timestamp = DateTime.UtcNow,
                            AuthorId = adrian.AuthorId
                        }
                    );
                    db.SaveChanges();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection?.Dispose();
                _connection = null;
            }
        }
    }
}

[CollectionDefinition("E2E")]
public class E2ECollection : ICollectionFixture<TestFixture> { }

[Collection("E2E")]
public class End2EndTests
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public End2EndTests(TestFixture fx, ITestOutputHelper output)
    {
        _client = fx.Client;
        _output = output;
    }

    [Fact(DisplayName = "Public timeline HTML contains Helge's cheep")]
    public async Task PublicTimeline_HtmlContainsHelgeCheep()
    {
        var resp = await _client.GetAsync("/");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await resp.Content.ReadAsStringAsync();
        html.Should().Contain("Helge");
        html.Should().Contain("Hello, BDSA students!");
    }

    [Fact(DisplayName = "Adrian's timeline HTML contains Adrian's cheep")]
    public async Task AdrianTimeline_HtmlContainsAdrianCheep()
    {
        var resp = await _client.GetAsync("/Adrian");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await resp.Content.ReadAsStringAsync();
        html.Should().Contain("Adrian");
        html.Should().Contain("Hej, velkommen til kurset.");
    }

    [Fact(DisplayName = "Public timeline returns OK and HTML content")]
    public async Task PublicTimeline_ReturnsHtml()
    {
        var resp = await _client.GetAsync("/");
        
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var contentType = resp.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("text/html");
        
        var html = await resp.Content.ReadAsStringAsync();
        html.Should().NotBeNullOrWhiteSpace();
        html.Should().Contain("Chirp!");
    }

    [Fact(DisplayName = "User timeline with pagination works")]
    public async Task UserTimeline_WithPagination()
    {
        var resp = await _client.GetAsync("/Helge?page=1");
        
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await resp.Content.ReadAsStringAsync();
        html.Should().Contain("Helge");
    }
}