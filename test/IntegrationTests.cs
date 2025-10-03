using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Chirp.IntegratedTests;
public record CheepCreateRequest(string Author, string Message);
public record CheepResponse(string Author, string Message, long? Timestamp);

public class IntegrationTests : IClassFixture<WebApplicationFactory<Chirp.WebApi.Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Chirp.WebApi.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCheep_ShouldReturnCreatedCheep()
    {
        // Arrange
        var request = new CheepCreateRequest("TestAuthor", "Hello, Chirp!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/cheeps", request);
        var createdCheep = await response.Content.ReadFromJsonAsync<CheepResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        createdCheep.Should().NotBeNull();
        createdCheep!.Author.Should().Be(request.Author);
        createdCheep.Message.Should().Be(request.Message);
        createdCheep.Timestamp.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCheeps_ShouldReturnListOfCheeps()
    {
        // Act
        var response = await _client.GetAsync("/api/cheeps");
        var cheeps = await response.Content.ReadFromJsonAsync<CheepResponse[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        cheeps.Should().NotBeNull();
        cheeps!.Should().NotBeEmpty();
    }
}
