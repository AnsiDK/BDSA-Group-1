public class Cheep
{
    public required string Author { get; set; }
    public required string Message { get; set; }
    public required long Timestamp { get; set; }

    public void Display()
    {
        Console.WriteLine($"{Author} says:");
        Console.Write($"(@{DateTimeOffset.FromUnixTimeSeconds(Timestamp).DateTime})");
        Console.WriteLine($"{Message}");
    }

    //public static implicit operator Cheep(Cheep v)
    //{
    //    throw new NotImplementedException();
    //}
    
}