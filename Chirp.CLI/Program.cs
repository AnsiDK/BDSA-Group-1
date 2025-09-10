using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using SimpleDB;

class Program

{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            IDatabaseRepository<string> db = new CSVDatabase<string>();
            db.Read(10);
        }
        else if (args.Length >= 2 && args[0] == "--cheep")
        {
            string message = string.Join(" ", args, 1, args.Length - 1).Trim('"');
            IDatabaseRepository<string> db = new CSVDatabase<string>();
            db.Store(message);
        }
        else
        {
            Console.WriteLine("Invalid arguments. Use no arguments to read or '--add <entry>' to add a new entry.");
        }

    }
}