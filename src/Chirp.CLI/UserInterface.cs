
class UserInterface
{
    public static void DisplayMessage(List<Cheep> message)
    {
        foreach (var line in message)
        {
            Console.WriteLine($"[{line.Timestamp}] {line.Author}:");
            Console.WriteLine(line.Message);
        }
    }

    public static void DisplayCheep(Cheep cheep)
    {
        Console.WriteLine($"[{cheep.Timestamp}] {cheep.Author}:");
        Console.WriteLine(cheep.Message);
    }
}