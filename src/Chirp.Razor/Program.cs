using Chirp.Models;
using SimpleDB;
using SimpleDB.Mappers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Build absolute path to solution-level data folder
/*
var dataDir = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data"));
var dbPath = Path.Combine(dataDir, "chirp_cli_db.db");
*/

var overridePath = Environment.GetEnvironmentVariable("CHIRPDBPATH");
var dbPath = string.IsNullOrWhiteSpace(overridePath)
    ? Path.Combine(Path.GetTempPath(), "chirp.db")
    : Path.GetFullPath(overridePath);

// Make sure directory exists
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

// Register the database repository (this calls Create, so getInstance is NOT needed)
builder.Services.AddSingleton<IDatabaseRepository<Cheep>>(_ =>
{
    Console.WriteLine($"[Startup] Using SQLite DB at: {dbPath}");
    return SQLiteDatabase<Cheep>.Create(dbPath, new CheepMapper());
});

// CheepService depends on the repository
builder.Services.AddScoped<ICheepService, CheepService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();

public partial class Program { } //For testing