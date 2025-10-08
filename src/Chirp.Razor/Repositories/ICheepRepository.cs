using Chirp.Razor.Models;

namespace Chirp.Razor.Repositories;

public interface ICheepRepository
{
    List<Cheep> GetCheeps(int page = 1, int pageSize = 32);
    List<Cheep> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32);
}