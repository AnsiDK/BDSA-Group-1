namespace Chirp.Razor.Models;

public class Cheep
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public DateTime Timestamp { get; set; } // store UTC
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}