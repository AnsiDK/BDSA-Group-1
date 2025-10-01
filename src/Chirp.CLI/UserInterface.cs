using System;
using System.Collections.Generic;
using Chirp.Models;

static class UserInterface
{
    public static void DisplayMessage(IEnumerable<Cheep> messages)
    {
        foreach (var c in messages)
        {
<<<<<<< HEAD
            Console.WriteLine($"[{line.Timestamp}] {line.Author}:");
            Console.WriteLine(line.Message);
=======
            Console.WriteLine(c.Message);
>>>>>>> Refactoring-to-SQLite-DB
        }
    }

    public static void DisplayCheep(Cheep cheep)
    {
        Console.WriteLine($"[{cheep.Timestamp}] {cheep.Author}:");
        Console.WriteLine(cheep.Message);
    }
}