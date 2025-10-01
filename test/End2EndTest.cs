using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Chirp.E2E;

// DTOs
public record CheepCreateRequest(string Author, string Message);
public record CheepResponse(string Author, string Message, long? Timestamp);

// Shared fixture
public class TestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program>? _factory;
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
            // Use in-process hosting
            _factory = new WebApplicationFactory<Program>();
            Client = _factory.CreateClient();
        }
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory?.Dispose();
    }
}

[CollectionDefinition("E2E")]
public class E2ECollection : ICollectionFixture<TestFixture> { }

[Collection("E2E")]
public class End2EndTests
{
    private readonly HttpClient _client;
    private const string CheepsListRoute = "/cheeps";
    private const string SingleCheepPostRoute = "/cheep";

    public End2EndTests(TestFixture fx) => _client = fx.Client;

    [Fact(DisplayName = "Create a cheep then list contains it")]
    public async Task Create_Then_List()
    {
        var marker = Guid.NewGuid().ToString("N");
        var author = $"e2e_{marker}";
        var message = $"Hello E2E {marker}";

        var postResp = await _client.PostAsJsonAsync(SingleCheepPostRoute,
            new CheepCreateRequest(author, message));
        
        postResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResp = await _client.GetAsync(CheepsListRoute);
        listResp.EnsureSuccessStatusCode();
        var items = await listResp.Content.ReadFromJsonAsync<CheepResponse[]>();
        items.Should().NotBeNull();
        items!.Any(c => c.Author == author && c.Message == message).Should().BeTrue();
    }

    [Theory(DisplayName = "Invalid payload returns 400")]
    [InlineData("", "msg")]
    [InlineData("author", "")]
    [InlineData("", "")]
    public async Task Invalid_Payload(string author, string message)
    {
        var resp = await _client.PostAsJsonAsync(SingleCheepPostRoute,
            new CheepCreateRequest(author, message));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "List cheeps returns OK and JSON body")]
    public async Task List_Cheeps()
    {
        var resp = await _client.GetAsync(CheepsListRoute);
        
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var raw = await resp.Content.ReadAsStringAsync();
        raw.Should().NotBeNullOrWhiteSpace();
    }
}