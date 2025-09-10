
class UserInterface
{
    public void DisplayMessage(string[] message)
    {
        foreach (var line in message)
        {
            Console.WriteLine(line);
        }
    }
}