using System.Data;
using Microsoft.Data.Sqlite;
using Chirp.Models;

namespace SimpleDB.Mappers;

public class CheepMapper : ISQLiteMapper<Cheep> {
    public string CreateTableSQL => @"
        CREATE TABLE IF NOT EXISTS Cheeps (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            Author    TEXT    NOT NULL,
            Message   TEXT    NOT NULL,
            Timestamp INTEGER NOT NULL
        );";

    public string SelectAllSQL => @"
        SELECT Id, Author, Message, Timestamp 
        FROM Cheeps 
        ORDER BY Id DESC;";

    public string SelectLimitSQL(int limit) => $@"
        SELECT Id, Author, Message, Timestamp 
        FROM Cheeps 
        ORDER BY Id DESC 
        LIMIT $limit;";

    public string InsertSQL => @"
        INSERT INTO Cheeps (Author, Message, Timestamp) 
        VALUES ($author, $message, $timestamp);";

    public Cheep FromRow(IDataRecord row)
    {
        return new Cheep
        {
            Author = row.GetString(0),
            Message = row.GetString(1),
            Timestamp = row.GetInt64(2)
        };
    }

    public void BindInsertParameters(SqliteCommand cmd, Cheep item)
    {
        cmd.Parameters.AddWithValue("$author", item.Author);
        cmd.Parameters.AddWithValue("$message", item.Message);
        cmd.Parameters.AddWithValue("$timestamp", item.Timestamp);
    }
}