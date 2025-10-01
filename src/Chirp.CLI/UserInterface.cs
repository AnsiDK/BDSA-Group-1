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
}