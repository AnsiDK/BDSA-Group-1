namespace Chirp.Razor.Models;

public class CheepDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
}

