namespace Chirp.Razor.Models;

// DTO used by Razor views. Only primitive/string fields.
public record CheepDTO(string Author, string Message, string Timestamp);