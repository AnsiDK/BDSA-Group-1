using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Chirp.IntegratedTests;


public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "CreateCheep_ShouldReturnCreatedCheep")]
    public async Task CreateCheep_ShouldReturnCreatedCheep()
    {
        // Act
        var respone = await _client.GetAsync("/");

        // Assert
        respone.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await respone.Content.ReadAsStringAsync();
        html.Should().Contain("Helge");
        html.Should().Contain("Hello, BDSA students!");
    }

    /*[Fact(DisplayName = "GetCheeps_ShouldReturnListOfCheeps")]
    public async Task GetCheeps_ShouldReturnListOfCheeps()
    {
        // Act
        var response = await _client.GetAsync("/api/cheeps");
        var cheeps = await response.Content.ReadFromJsonAsync<CheepResponse[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        cheeps.Should().NotBeNull();
        cheeps!.Should().NotBeEmpty();
    }*/
}