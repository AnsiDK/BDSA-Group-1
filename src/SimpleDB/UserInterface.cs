
class UserInterface
{
    public static void DisplayMessage(List<Cheep> message)
    {
        foreach (var line in message)
        {
            Console.WriteLine(line);
        }
    }
}