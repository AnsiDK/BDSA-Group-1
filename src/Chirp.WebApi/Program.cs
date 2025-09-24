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


// GET /cheeps (all) or /cheeps?limit=10
app.MapGet("/cheeps", (IDatabaseRepository<Cheep> db, int? limit) =>
{
    IEnumerable<Cheep> all = db.ReadAll();
    if (limit is > 0)
        all = db.Read(limit.Value);
    
    if (all.Length < 1)
        return Results.NoContent();
    return Results.Ok(all);
});

// POST /cheep  { "author": "", "message": "" }
app.MapPost("/cheep", (IDatabaseRepository<Cheep> db, CreateCheepDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Author) || string.IsNullOrWhiteSpace(dto.Message))
        return Results.BadRequest("Author and Message are required.");

    long ts = (dto.Timestamp is > 0) ? dto.Timestamp.Value : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    var cheep = new Cheep
    {
        Author = dto.Author.Trim(),
        Message = dto.Message.Trim(),
        Timestamp = ts
    };
    db.Store(cheep);
    return Results.Created("/cheeps", cheep);
});

app.Run();

// local DTO for creating a new Cheep
public record CreateCheepDto(string Author, string Message, long? Timestamp);