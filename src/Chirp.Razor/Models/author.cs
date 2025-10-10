namespace Chirp.Razor.Models;

public class Author
{
    public int AuthorId { get; set; }

    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;

    public List<Cheep> Cheeps { get; set; } = new();
}