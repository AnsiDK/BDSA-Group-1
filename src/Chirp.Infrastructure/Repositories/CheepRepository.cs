using Chirp.Core.Entities;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepRepository : ICheepRepository
{
    private readonly ChirpDbContext _db;

    public CheepRepository(ChirpDbContext db)
    {
        _db = db;
    }

    public List<Cheep> GetCheeps(int page = 1, int pageSize = 32)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;

        return _db.Cheeps
            .AsNoTracking()
            .Include(c => c.Author)
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public List<Cheep> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32)
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
            .ToList();
    }
}
