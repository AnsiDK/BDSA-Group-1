using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string path = "chirp_cli_db.csv"; // Path to your CSV file

        if (args.Length == 0)
        {
            try
            {
                foreach (var line in File.ReadLines(path))
                {
                    var values = line.Split(',');
                    // Process values here
                    Console.WriteLine(string.Join(" | ", values));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        else if (args.Length >= 2 && args[0] == "--cheep")
        {
            string message = string.Join(" ", args, 1, args.Length - 1).Trim('"');
            string author = "CLI User"; // Default author for CLI entries
            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string cheepLine = $"{author},{message},{unixTimestamp}";

            try
            {
                File.AppendAllText(path, cheepLine + Environment.NewLine);
                Console.WriteLine("Cheep added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding the cheep: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Invalid arguments. Use no arguments to read or '--add <entry>' to add a new entry.");
        }
                   
    }
}