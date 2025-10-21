using Chirp.Core.DTOs;

namespace Chirp.Core.Interfaces;

public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32);
}
