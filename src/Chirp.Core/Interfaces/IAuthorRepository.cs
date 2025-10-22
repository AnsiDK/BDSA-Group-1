using Chirp.Core.Entities;

namespace IAuthorRepository;

public interface IAuthorRepository
{
    Author? GetAuthorByName(string name);

    Author? GetAuthorByEmail(string email);

    void AddAuthor(Author author);
}