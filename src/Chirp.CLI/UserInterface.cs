using System;
using System.Collections.Generic;
using Chirp.Models;

static class UserInterface
{
    public static void DisplayMessage(IEnumerable<Cheep> messages)
    {
        foreach (var c in messages)
        {
            Console.WriteLine(c.Message);
        }
    }

    public static void DisplayCheep(Cheep cheep)
    {
        Console.WriteLine($"[{cheep.Timestamp}] {cheep.Author}:");
        Console.WriteLine(cheep.Message);
    }
}