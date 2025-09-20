using Microsoft.AspNetCore.OpenApi;
using SimpleDB;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

// Minimal API Essentials
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Build absolute path to solution-level data folder
var dataDir = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data"));
var csvPath = Path.Combine(dataDir, "chirp_cli_db.csv");

// Register singleton CSVDatabase instance with the chosen file path
builder.Services.AddSingleton<IDatabaseRepository<Cheep>>(_ => CSVDatabase.Create(csvPath));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// GET /cheeps?limit=10
app.MapGet("/cheeps", (IDatabaseRepository<Cheep> db, int limit = 10) =>
{
    if (limit <= 0 || limit > 100) limit = 10;
    var cheeps = db.Read(limit);
    return Results.Ok(cheeps);
});

// POST /cheep  { "author": "", "message": "" }
app.MapPost("/cheep", (IDatabaseRepository<Cheep> db, CreateCheepDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Author) || string.IsNullOrWhiteSpace(dto.Message))
        return Results.BadRequest("Author and Message are required.");

    var cheep = new Cheep
    {
        Author = dto.Author.Trim(),
        Message = dto.Message.Trim(),
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    };
    db.Store(cheep);
    return Results.Created("/cheeps", cheep);
});

app.Run();

// local DTO for creating a new Cheep
public record CreateCheepDto(string Author, string Message);