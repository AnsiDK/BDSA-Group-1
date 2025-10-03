using Chirp.Models;
using SimpleDB;
public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps(int page = 1, int pageSize = 32);
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32);
}

public class CheepService : ICheepService
{   
    private readonly IDatabaseRepository<Cheep> _db;

    public CheepService(IDatabaseRepository<Cheep> db)
    {
        _db = db;
    }
    // These would normally be loaded from a database for example
    private static readonly List<CheepViewModel> _cheeps = new()
        {
            new CheepViewModel("Helge", "Hello, BDSA students!", UnixTimeStampToDateTimeString(1690892208)),
            new CheepViewModel("Adrian", "Hej, velkommen til kurset.", UnixTimeStampToDateTimeString(1690895308)),
        };

    public List<CheepViewModel> GetCheeps(int page = 1, int pageSize = 32)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;

        // Fetch all (simple approach)
        var all = _db.ReadAll(); // IEnumerable<Cheep>
        
        var pageItems = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(Map)
            .ToList();

        return pageItems;
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32)
    {
        if (string.IsNullOrWhiteSpace(author))
            return new List<CheepViewModel>();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;

        var filtered = _db.ReadAll()
            .Where(c => string.Equals(c.Author, author, StringComparison.OrdinalIgnoreCase));

        var pageItems = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(Map)
            .ToList();

        return pageItems;
    }

    private static CheepViewModel Map(Cheep c) =>
        new(c.Author, c.Message, UnixTimeStampToDateTimeString(c.Timestamp));

    private static string UnixTimeStampToDateTimeString(long ts)
    {
        if (ts <= 0) return "-";
        var dt = DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
        return dt.ToString("MM/dd/yy H:mm:ss");
    }
}
