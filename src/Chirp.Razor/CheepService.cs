using Chirp.Razor.Data;
using Chirp.Razor.Models;
using Microsoft.EntityFrameworkCore;

//public record CheepViewModel(string Author, string Message, string Timestamp);

public class CheepDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
}

public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32);
}

public class CheepService : ICheepService
{
    // private readonly ChirpDbContext _db;
    private readonly Chirp.Razor.Repositories.ICheepRepository _repo;

    public CheepService(Chirp.Razor.Repositories.ICheepRepository repo)
    {
        // _db = db;
        _repo = repo;
    }

    public List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32)
    {
        // if (page < 1) page = 1;
        // if (pageSize < 1) pageSize = 32;

        var items = _repo.GetCheeps(page, pageSize);
        return items.Select(c => new CheepDTO(
                c.Author.Name,
                c.Text,
                FormatTs(c.Timestamp)))
            .ToList();
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32)
    {
        // if (string.IsNullOrWhiteSpace(author)) return new();

        // if (page < 1) page = 1;
        // if (pageSize < 1) pageSize = 32;

        var items = _repo.GetCheepsFromAuthor(author, page, pageSize);
        return items.Select(c => new CheepViewModel(
                c.Author.Name,
                c.Text,
                FormatTs(c.Timestamp)))
            .ToList();
    }

    private static string FormatTs(DateTime dtUtc)
        => dtUtc.ToUniversalTime().ToString("dd/mm/yy H:mm:ss");
}