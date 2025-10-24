using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using System.Globalization;

namespace Chirp.Infrastructure.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _repo;

    public AuthorService(IAuthorRepository repo)
    {
        _repo = repo;
    }

    public AuthorDTO? GetAuthorByName(string name)
    {
        return _repo.GetAuthorByName(name);
    }

    public AuthorDTO? GetAuthorByEmail(string email)
    {
        return _repo.GetAuthorByEmail(email);
    }

    public void AddAuthor(Author author)
    {
        _repo.AddAuthor(author);
    }
}