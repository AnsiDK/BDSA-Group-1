using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

class Program

{
    static void Main(string[] args)
    {
        string path = "chirp_cli_db.csv"; // Path to your CSV file
        using var reader = new StreamReader(path);
        
        if (args.Length == 0)
        {
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            try
            {
                var records = csv.GetRecords<Cheep>();
                foreach (var Cheep in records)
                {
                    Cheep.Display();
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
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

            var cheep = new Cheep
            {
                Author = author,
                Message = message,
                Timestamp = timestamp
            };

            using var stream = new StreamWriter(path, append: true);
            using var csv = new CsvWriter(stream, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            });

            try
            {
                csv.WriteRecord(cheep);
                csv.NextRecord();
                Console.WriteLine("Record successfully added!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while adding the cheep: {e.Message}");
            }
        }
        else
        {
            Console.WriteLine("Invalid arguments. Use no arguments to read or '--add <entry>' to add a new entry.");
        }

    }
}