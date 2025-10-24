using System.Net;
using System.Text.RegularExpressions;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Chirp.LocalServer;

namespace Chirp.test;

// Note: These integration tests were for the old WebApi project which had REST API endpoints.
// The new Razor Pages application doesn't expose API endpoints - it serves HTML pages.
// For web app testing, use End2EndTests.cs which tests the HTML pages.

public class IntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public IntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
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

    [Fact]
    public async Task AuthorTimeline_WithFortyCheeps_PaginatesInto32And8()
    {
        const string authorName = "IntegrationTestAuthor";
        await EnsureAuthorHasCheepsAsync(authorName, 40);

        var totalCheeps = await GetCheepCountAsync(authorName);
        totalCheeps.Should().Be(40);

        var firstPageResponse = await _client.GetAsync($"/{authorName}?page=1");
        firstPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstPageHtml = await firstPageResponse.Content.ReadAsStringAsync();

        CountCheepsInTimeline(firstPageHtml).Should().Be(32);
        firstPageHtml.Should().Contain(authorName);

        var secondPageResponse = await _client.GetAsync($"/{authorName}?page=2");
        secondPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondPageHtml = await secondPageResponse.Content.ReadAsStringAsync();

        CountCheepsInTimeline(secondPageHtml).Should().Be(8);
        secondPageHtml.Should().Contain(authorName);
    }

    private async Task EnsureAuthorHasCheepsAsync(string authorName, int cheepCount)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();

        var author = await db.Authors.SingleOrDefaultAsync(a => a.Name == authorName);
        if (author is null)
        {
            author = new Author
            {
                Name = authorName,
                Email = $"{authorName.ToLowerInvariant()}@example.com"
            };
            db.Authors.Add(author);
            await db.SaveChangesAsync();
        }

        var existingCheeps = await db.Cheeps
            .Where(c => c.AuthorId == author.AuthorId)
            .ToListAsync();

        if (existingCheeps.Count > 0)
        {
            db.Cheeps.RemoveRange(existingCheeps);
            await db.SaveChangesAsync();
        }

        var baseTimestamp = DateTime.UtcNow;
        var newCheeps = Enumerable.Range(0, cheepCount)
            .Select(i => new Cheep
            {
                AuthorId = author.AuthorId,
                Author = author,
                Text = $"Integration cheep {i:D2}",
                Timestamp = baseTimestamp.AddMinutes(-i)
            });

        await db.Cheeps.AddRangeAsync(newCheeps);
        await db.SaveChangesAsync();
    }

    private async Task<int> GetCheepCountAsync(string authorName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();

        return await db.Cheeps.CountAsync(c => c.Author.Name == authorName);
    }

    private static int CountCheepsInTimeline(string html)
    {
        var listStart = html.IndexOf("id=\"messagelist\"", StringComparison.Ordinal);
        if (listStart < 0)
        {
            return 0;
        }

        var listEnd = html.IndexOf("</ul>", listStart, StringComparison.OrdinalIgnoreCase);
        if (listEnd < 0)
        {
            listEnd = html.Length;
        }

        var snippet = html.Substring(listStart, listEnd - listStart);
        return Regex.Matches(snippet, "<li", RegexOptions.IgnoreCase).Count;
    }
}
