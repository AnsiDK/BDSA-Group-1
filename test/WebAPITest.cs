using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Chirp.WebApi
{
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