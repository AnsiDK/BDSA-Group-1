using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Chirp.E2E;

// DTOs
public record CheepCreateRequest(string Author, string Message);
public record CheepResponse(string Author, string Message, long? Timestamp);

// Shared fixture
public class TestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    public HttpClient Client { get; }

    public TestFixture()
{
    // Full path to the example database
    var exampleDb = Path.GetFullPath(Path.Combine("..","..", "data", "chirp_cli_db.db"));

    // Make sure the file exists
    if (!File.Exists(exampleDb))
        throw new FileNotFoundException("Cannot find example database", exampleDb);

    // Tell the app to use it
    Environment.SetEnvironmentVariable("CHIRPDBPATH", exampleDb);

    // Create the test server
    _factory = new WebApplicationFactory<Program>();
    Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("http://localhost")
    });
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
public class End2EndTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private const string CheepsListRoute = "/cheeps";
    private const string SingleCheepPostRoute = "/cheep";

    public End2EndTests(TestFixture fx, ITestOutputHelper output)
    {
        _client = fx.Client;
        _output = output;
    }

    [Fact(DisplayName = "Create a cheep then list contains it")]
    public async Task Create_Then_List()
    {
        var marker = Guid.NewGuid().ToString("N");
        var author = $"e2e_{marker}";
        var message = $"Hello E2E {marker}";

        _output.WriteLine($"POST author={author} message={message}");

        var postResp = await _client.PostAsJsonAsync(SingleCheepPostRoute,
            new CheepCreateRequest(author, message));
        _output.WriteLine($"POST Status={(int)postResp.StatusCode} Body={await postResp.Content.ReadAsStringAsync()}");

        postResp.StatusCode.Should().Be(HttpStatusCode.Created);
        _output.WriteLine("POST Location: " + postResp.Headers.Location);

        var listResp = await _client.GetAsync(CheepsListRoute);
        listResp.EnsureSuccessStatusCode();

        var rawList = await listResp.Content.ReadAsStringAsync();
        _output.WriteLine("RAW LIST JSON: " + rawList);

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

    [Fact(DisplayName = "Public time HTML contains Helge's cheep")]
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
}