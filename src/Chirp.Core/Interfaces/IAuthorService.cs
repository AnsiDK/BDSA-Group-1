using Chirp.Core.Entities;

namespace IAuthorService;

public interface IAuthorService
{
    AuthorDTO? GetAuthorByName(string name);

    AuthorDTO? GetAuthorByEmail(string email);

    void AddAuthor(Author author);
}