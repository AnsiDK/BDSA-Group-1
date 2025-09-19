using System;
using System.Collections.Generic;
using DocoptNet;
using SimpleDB;

class Program
{
    private const string usage = @"
        Chirp CLI.

        Usage:
        chirp cheep <message>
        chirp

        Options:
        -h --help     Show this screen.
    ";

    static int Main(string[] args)
    {
        var arguments = new Docopt().Apply(usage, args, exit: true);

        if (arguments["cheep"].IsTrue)
        {
            string message = arguments["<message>"].ToString();
            var cheep = new Cheep
            {
                Author = Environment.UserName,
                Message = message,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            IDatabaseRepository<Cheep> db = CSVDatabase.getInstance();
            db.Store(cheep);
        }
        else
        {
            IDatabaseRepository<Cheep> db = CSVDatabase.getInstance();
            db.Read(10);
        }

        return 0;
    }
}