using System;
using System.Collections.Generic;
using DocoptNet;
using SimpleDB;
using Microsoft.AspNetCore.Builder;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Chirp.CLI;

public partial class Program
{
    private const string usage = @"
        Chirp CLI.

        Usage:
        chirp cheep <message> [--author=<name>] [--api=<url>]
        chirp [--api=<url>]

        Options:
        --api=<url>   API base URL [default: https://bdsagroup1chirpremotedb1-axhbcyh6b2h9c5fe.norwayeast-01.azurewebsites.net/]
        --author=<name>  Author name [default: <system user>]
        -h --help     Show this screen.
    ";

    public static async Task<int> Main(string[] args)
    {
        var arguments = new Docopt().Apply(usage, args, exit: true);

        // Resolve solution root and data directory
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var dataDir = Path.Combine(solutionRoot, "data");
        Directory.CreateDirectory(dataDir);
        var csvPath = Path.Combine(dataDir, "chirp_cli_db.csv");

        // Ensure CSV singleton is initialized once with the shared path
        var repo = CSVDatabase.Create(csvPath);

        var apiBase = arguments["--api"]?.ToString() ?? "http://localhost:5146";

        // Address nullable warning
        if (apiBase == null)
        {
            throw new InvalidOperationException("API base URL cannot be null.");
        }

        var useApi = true;
        using var http = new HttpClient { BaseAddress = new Uri(apiBase) };

        if (arguments["cheep"].IsTrue)
        {
            var message = arguments["<message>"]?.ToString() ?? "";
            var authorRaw = arguments["--author"]?.ToString();
            var author = string.IsNullOrWhiteSpace(authorRaw) || authorRaw == "<system user>"
                ? Environment.UserName
                : authorRaw;

            if (useApi)
            {
                try
                {
                    var resp = await http.PostAsJsonAsync("/cheep", new
                    {
                        Author = author,
                        Message = message,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    });
                    if (!resp.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"API Error: {resp.StatusCode} {resp.ReasonPhrase}. Falling back to CSV.");
                        useApi = false;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"API unreachable ({ex.Message}). Falling back to CSV.");
                    useApi = false;
                }
            }

            if (!useApi)
            {
                var cheep = new Cheep
                {
                    Author = author,
                    Message = message,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                IDatabaseRepository<Cheep>? db = CSVDatabase.getInstance();
                if (db != null)
                {
                    db.Store(cheep);
                    Console.WriteLine("Cheep stored in CSV database.");
                }
            }
        }
        else
        {
            List<Cheep>? cheeps = null;

            if (useApi)
            {
                var result = await RetreiveCheeps(http, 10);
                cheeps = result.Cheeps;
                useApi = result.UseApi;
                if (cheeps != null)
                {
                    UserInterface.DisplayMessage(cheeps);

                }
                else
                {
                    System.Console.WriteLine("No cheeps retrieved from API.");
                }
            }

            if (!useApi)
            {
                IDatabaseRepository<Cheep>? db = CSVDatabase.getInstance();
                if (db != null)
                {
                    var records = db.Read(10);
                    UserInterface.DisplayMessage(records.ToList());
                }
            }
            else if (cheeps != null)
            {
                UserInterface.DisplayMessage(cheeps);
            }
        }
        return 0;
    }

    public static async Task<(List<Cheep>? Cheeps, bool UseApi)> RetreiveCheeps(HttpClient http, int limit = 10)
    {
        try
        {
            var resp = await http.GetAsync($"/cheeps?limit={limit}");
            if (resp.IsSuccessStatusCode)
            {
                var cheeps = await resp.Content.ReadFromJsonAsync<List<Cheep>>();
                if (cheeps != null)
                {
                    Console.WriteLine("Displaying cheeps");
                    UserInterface.DisplayMessage(cheeps);
                    return (cheeps, true);
                }
            }
            else
            {
                Console.WriteLine($"API Error: {resp.StatusCode} {resp.ReasonPhrase}. Falling back to CSV.");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API unreachable ({ex.Message}). Falling back to CSV.");
        }

        return (null, false); // Return null cheeps and false for useApi if API fails
    }
}