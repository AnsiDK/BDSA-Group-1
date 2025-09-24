
class UserInterface
{
    public static void DisplayMessage(List<Cheep> message)
    {
        Console.WriteLine("Cheeps:" + message.Count);
        foreach (var line in message)
        {
            Console.WriteLine(line.Message);
        }
    }
}