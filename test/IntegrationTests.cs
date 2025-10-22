using System.Net;
using FluentAssertions;
using Xunit;
using Chirp.E2E;

namespace Chirp.IntegratedTests;

// Note: These integration tests were for the old WebApi project which had REST API endpoints.
// The new Razor Pages application doesn't expose API endpoints - it serves HTML pages.
// For web app testing, use End2EndTests.cs which tests the HTML pages.

// Reuse the same in-memory SQLite web app fixture as E2E tests
[Collection("E2E")]
public class IntegrationTests
{
    private readonly HttpClient _client;

    public IntegrationTests(TestFixture fx)
    {
        _client = fx.Client;
    }

    [Fact]
    public async Task GetHomePage_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserTimeline_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/Helge");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
