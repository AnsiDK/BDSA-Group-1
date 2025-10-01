using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Chirp.WebApi
{
    public class Cheep
    {
        public string Author { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }

    public class YourDBContext : DbContext
    {
        public YourDBContext(DbContextOptions<YourDBContext> options) : base(options) { }

        public DbSet<Cheep> Cheeps { get; set; }
        }

    public class TestWebApplicationFactory : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing database context registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<YourDBContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add a new database context using an in-memory database for testing
                services.AddDbContext<YourDBContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context (YourDBContext)
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<YourDBContext>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();

                    // Seed the database with test data if needed
                    db.Cheeps.AddRange(new[]
                    {
                        new Cheep { Author = "TestUser1", Message = "Hello World!", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                        new Cheep { Author = "TestUser2", Message = "Another Cheep", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
                    });
                    db.SaveChanges();
                }
            });
        }
    }
    
    public class WebApiTests : IClassFixture<WebApplicationFactory<Chirp.WebApi.Program>>
    {
        private readonly HttpClient _client;

        public WebApiTests(WebApplicationFactory<Chirp.WebApi.Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_Home_ReturnsSuccess()
        {
            // Arrange
            var response = await _client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_NonExistingEndpoint_ReturnsNotFound()
        {
            // Arrange
            var response = await _client.GetAsync("/api/doesnotexist");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_Cheeps_ReturnsSuccessAndJson()
        {
            // Arrange
            var response = await _client.GetAsync("/cheeps");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Post_Chirp_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/cheep", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task Get_Cheeps_WithLimit_ReturnsLimitedResults()
        {
            // Arrange
            var limit = 2;

            // Act
            var response = await _client.GetAsync($"/cheeps?limit={limit}");

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var cheeps = System.Text.Json.JsonSerializer.Deserialize<List<Cheep>>(json);

            Assert.NotNull(cheeps);
            Assert.True(cheeps.Count <= limit, $"Expected at most {limit} cheeps, but got {cheeps.Count}");
        }
    }
}