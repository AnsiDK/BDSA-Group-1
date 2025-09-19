using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using SimpleDB;
namespace SimpleDB;

public sealed class CSVDatabase : IDatabaseRepository<Cheep>
{
    private static CSVDatabase instance;

    private CSVDatabase() { }

    public static CSVDatabase getInstance()
    {
        if (instance == null)
        {
            instance = new CSVDatabase();
        }
        return instance;
    }
    public IEnumerable<Cheep> Read(int limit)
    {   
        string path = "src/chirp_cli_db.csv";
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        try
        {
            var records = csv.GetRecords<Cheep>().Take(limit).ToList();
            //UserInterface.DisplayMessage(records);
            return records;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return Enumerable.Empty<Cheep>();
        }
    }

    public void Store(Cheep cheep)
    {
        string path = "src/chirp_cli_db.csv";
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
}