using System;
using System.Collections.Generic;
using DocoptNet;
using SimpleDB;
using Microsoft.AspNetCore.Builder;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using DocoptNet;

class Program
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

    static async Task<int> Main(string[] args)
    {
        var arguments = new Docopt().Apply(usage, args, exit: true);

        // Resolve solution root and data directory
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var dataDir = Path.Combine(solutionRoot, "data");
        Directory.CreateDirectory(dataDir);
        var csvPath = Path.Combine(dataDir, "chirp_cli_db.csv");

        // Ensure CSV singleton is initialized once with the shared path
        var repo = CSVDatabase.Create(csvPath);

        var apiBase = arguments["--api"]?.ToString() ?? "https://bdsagroup1chirpremotedb1-axhbcyh6b2h9c5fe.norwayeast-01.azurewebsites.net/"; //"http://localhost:5146";

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
                try
                {
                    var resp = await http.GetAsync("/cheeps?limit=10");
                    Console.WriteLine(resp);
                    if (resp.IsSuccessStatusCode)
                    {
                        cheeps = await resp.Content.ReadFromJsonAsync<List<Cheep>>();
                        UserInterface.DisplayMessage(cheeps);
                    }
                    else
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
}