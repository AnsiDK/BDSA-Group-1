using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using System.Globalization;
using Chirp.Core.Entities;

namespace Chirp.Infrastructure.Services.AuthorService;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _repo;

    public AuthorService(IAuthorRepository repo)
    {
        _repo = repo;
    }

    public AuthorDTO? GetAuthorByName(string name)
    {
        return AuthorDTO(_repo.GetAuthorByName(name));
    }

    private AuthorDTO? AuthorDTO(Author? author)
    {
        throw new NotImplementedException();
    }

    public AuthorDTO? GetAuthorByEmail(string email)
    {
        return AuthorDTO(_repo.GetAuthorByEmail(email));
    }

    public void AddAuthor(Author author)
    {
        _repo.AddAuthor(author);
    }
}