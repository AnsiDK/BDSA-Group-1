using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using SimpleDB;
namespace SimpleDB;

public class CSVDatabase : IDatabaseRepository<Cheep>
{
    private readonly string _filePath;
    private static CSVDatabase? _instance;
    private static readonly object _lock = new();

    private CSVDatabase(string filePath)
    {
        _filePath = filePath;
    }

    // Initialize (or return existing) singleton with a chosen file path
    public static CSVDatabase Create(string filePath)
    {
        lock (_lock)
        {
            _instance ??= new CSVDatabase(filePath);
            return _instance;
        }
    }

    // Existing callers that relied on getInstance must now call Create first in startup
    public static CSVDatabase getInstance() =>
        _instance ?? throw new InvalidOperationException("CSVDatabase not initialized. Call Create(path) first.");

    public IEnumerable<Cheep> Read(int limit)
    {
        //string path = "data/chirp_cli_db.csv";
        EnsureFile();
        if (!File.Exists(_filePath))
            return Enumerable.Empty<Cheep>();
        try
        {
            using var reader = new StreamReader(_filePath);
            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false, // Writen without a header, so we tell CsvHelper not to expect one
                MissingFieldFound = null,
                BadDataFound = null
            };
            using var csv = new CsvReader(reader, cfg);
            return csv.GetRecords<Cheep>().Take(limit).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Occurred: {ex.Message}");
            return Enumerable.Empty<Cheep>();
        }
    }

    public void Store(Cheep cheep)
    {
        //string path = "data/chirp_cli_db.csv";
        EnsureFile();
        try
        {
            using var stream = new StreamWriter(_filePath, append: true);
            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false // Writen without a header, so we tell CsvHelper not to expect one
            };
            using var csv = new CsvWriter(stream, cfg);
            csv.WriteRecord(cheep);
            csv.NextRecord();
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while adding the cheep: {e.Message}");
        }
    }
    private void EnsureFile()
    {
        var dir = Path.GetDirectoryName(_filePath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        if (!File.Exists(_filePath))
            File.Create(_filePath).Dispose();
    }
}