using Chirp.Razor.Data;
using Chirp.Razor.Models;
using Microsoft.EntityFrameworkCore;

public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    List<CheepViewModel> GetCheeps(int page = 1, int pageSize = 32);
    List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32);
}

public class CheepService : ICheepService
{
    private readonly ChirpDbContext _db;

    public CheepService(ChirpDbContext db)
    {
        _db = db;
    }

    public List<CheepViewModel> GetCheeps(int page = 1, int pageSize = 32)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;

        return _db.Cheeps
            .AsNoTracking()
            .Include(c => c.Author)
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CheepViewModel(
                c.Author.Name,
                c.Text,
                FormatTs(c.Timestamp)))
            .ToList();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32)
    {
        if (string.IsNullOrWhiteSpace(author)) return new();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;

        return _db.Cheeps
            .AsNoTracking()
            .Include(c => c.Author)
            .Where(c => c.Author.Name == author)
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CheepViewModel(
                c.Author.Name,
                c.Text,
                FormatTs(c.Timestamp)))
            .ToList();
    }

    private static string FormatTs(DateTime dtUtc)
        => dtUtc.ToUniversalTime().ToString("MM/dd/yy H:mm:ss");
}