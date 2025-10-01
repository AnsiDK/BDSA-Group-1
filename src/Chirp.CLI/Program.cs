using System;
using System.Collections.Generic;
using DocoptNet;
//using Microsoft.AspNetCore.Builder;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
//using Microsoft.Data.Sqlite;
//using System.IO;
using SimpleDB;
using Chirp.Models;

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

    static async Task<int> Main(string[] args) {
        var arguments = new Docopt().Apply(usage, args, exit: true);

        var SQLite = SQLiteDatabase<Cheep>.Create("chirp.db", new CheepMapper());
        IDatabaseRepository<Cheep> localDb = SQLiteDatabase<Cheep>.getInstance();

        var apiBaseUrl = arguments["--api"]?.ToString() ?? "https://bdsagroup1chirpremotedb1-axhbcyh6b2h9c5fe.norwayeast-01.azurewebsites.net/";
        using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) }; // Set base address for HttpClient
        bool useApi = true; // C# uses "bool" instead of "boolean"

        if (arguments["cheep"].IsTrue) {
            var message = arguments["<message>"]?.ToString() ?? "";
            var authorRaw = arguments["--author"]?.ToString();
            var author = string.IsNullOrWhiteSpace(authorRaw) || authorRaw == "<system user>"
                ? Environment.UserName
                : authorRaw;
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    

            if (useApi) {
                try
                {
                    var resp = await http.PostAsJsonAsync("/cheep", new
                    {
                        Author = author,
                        Message = message,
                        Timestamp = ts
                    });
                    if (!resp.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"API Error: {resp.StatusCode} {resp.ReasonPhrase}. Falling back to SQLite.");
                        useApi = false;
                    }
                }
                catch (HttpRequestException ex)
                {
                    {
                        Console.WriteLine($"API unreachable ({ex.Message}). Falling back to SQLite.");
                        useApi = false;
                    }

                }
            }

                if (!useApi) {
                    localDb.Store(new Cheep(author, message, ts));
                    Console.WriteLine("Cheep saved locally.");
                }
            }
            else {
                List<Cheep>? cheeps = null;

                try {
                    var resp = await http.GetAsync("/cheeps");
                    if (resp.IsSuccessStatusCode) {
                        cheeps = await resp.Content.ReadFromJsonAsync<List<Cheep>>();
                        UserInterface.DisplayMessage(cheeps);
                    }
                    else {
                        Console.WriteLine($"API Error: {resp.StatusCode} {resp.ReasonPhrase}.");
                    }
                }
                catch (HttpRequestException ex) {
                    Console.WriteLine($"API unreachable ({ex.Message}). Falling back to SQLite.");
                }
            }
            return 0;
        }
        
    }
