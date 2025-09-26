var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

static Cheep getCheeps()
{
    IDatabaseRepository<Cheep> db = CSVDatabase.getInstance();
    db.read;
}

static void storeCheep(Cheep cheep)
{ 
    IDatabaseRepository<Cheep> db = CSVDatabase.getInstance();
    db.Store(cheep);
}

app.MapGet("/cheeps", getCheeps);
app.MapPost("/cheep", (Cheep cheep) => { storeCheep(cheep); });
app.run();




