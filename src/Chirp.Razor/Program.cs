using Chirp.Razor.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Resolve DB path from env or fallback to temp
var overridePath = Environment.GetEnvironmentVariable("CHIRPDBPATH");
var dbPath = string.IsNullOrWhiteSpace(overridePath)
    ? Path.Combine(Path.GetTempPath(), "chirp.db")
    : Path.GetFullPath(overridePath);

// Make sure directory exists
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

// Services
builder.Services.AddRazorPages();

// EF Core with SQLite
builder.Services.AddDbContext<ChirpDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
});

// App services
builder.Services.AddScoped<ICheepService, CheepService>();

var app = builder.Build();

// Apply pending migrations automatically (or switch to EnsureCreated for a quick start)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
    db.Database.Migrate();
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();