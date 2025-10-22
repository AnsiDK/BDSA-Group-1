using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Chirp.E2E;

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
    private readonly ITestOutputHelper _output;

    public End2EndTests(TestFixture fx, ITestOutputHelper output)
    {
        _client = fx.Client;
        _output = output;
    }

    [Fact(DisplayName = "Public timeline HTML contains Helge's cheep")]
    public async Task PublicTimeline_HtmlContainsHelgeCheep()
    {
        var resp = await _client.GetAsync("/?page=21");
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