
using SimpleDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
namespace Chirp.CLI;

public static class WebApp
{
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/cheeps", getCheeps);
        app.MapPost("/cheep", (Cheep cheep) => { storeCheep(cheep); });

        app.Run();
        return app;
    }
    private static IEnumerable<Cheep> getCheeps()
    {
        IDatabaseRepository<Cheep>? db = CSVDatabase.getInstance();
        return db.Read(10);
    }

    private static void storeCheep(Cheep cheep)
    {
        IDatabaseRepository<Cheep> db = CSVDatabase.getInstance();
        db.Store(cheep);
    }
}




