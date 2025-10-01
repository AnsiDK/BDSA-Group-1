using System;
using System.Collections.Generic;
using DocoptNet;
using Microsoft.AspNetCore.Builder;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using System.IO;


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

        var apiBaseUrl = arguments["--api"]?.ToString() ?? "https://bdsagroup1chirpremotedb1-axhbcyh6b2h9c5fe.norwayeast-01.azurewebsites.net/";
        using var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) }; // Set base address for HttpClient
        bool useApi = true; // C# uses "bool" instead of "boolean"

        // SQLite setup
        SQLitePCL.Batteries_V2.Init(); // Initialize SQLite

        var dbPath = Path.Combine(AppContext.BaseDirectory, "chirp.db"); // Database file path
        Console.WriteLine($"[SQLite] Using DB at: {dbPath}"); // Log the database path (temporary)

        var connectionString = $"Data Source={dbPath}"; // Connection string

        using var connection = new SqliteConnection(connectionString); // Create connection
        connection.Open(); // Open connection

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Cheeps (
                    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    Author    TEXT    NOT NULL,
                    Message   TEXT    NOT NULL,
                    Timestamp INTEGER NOT NULL
                );";
            command.ExecuteNonQuery();
        }

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
                        {
                            Console.WriteLine($"API unreachable ({ex.Message}). Falling back to SQLite.");
                            useApi = false;
                        }

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

                    using (var insertCommand = connection.CreateCommand()) 
                    {
                        insertCommand.CommandText = @"
                        INSERT INTO Cheeps (Author, Message, Timestamp)
                        VALUES ($author, $message, $timestamp);";
                        insertCommand.Parameters.AddWithValue("$author", cheep.Author);
                        insertCommand.Parameters.AddWithValue("$message", cheep.Message);
                        insertCommand.Parameters.AddWithValue("$timestamp", cheep.Timestamp);
                        insertCommand.ExecuteNonQuery();
                    }
                    Console.WriteLine("Cheep saved locally.");
                }
            }
            else
            {
                List<Cheep>? cheeps = null;

                try
                {
                    var resp = await http.GetAsync("/cheeps");
                    if (resp.IsSuccessStatusCode)
                    {
                        cheeps = await resp.Content.ReadFromJsonAsync<List<Cheep>>();
                        UserInterface.DisplayMessage(cheeps);
                    }
                    else
                    {
                        Console.WriteLine($"API Error: {resp.StatusCode} {resp.ReasonPhrase}.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"API unreachable ({ex.Message}).");
                }
            }
            return 0;
        }
    }
