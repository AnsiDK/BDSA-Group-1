using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;

public class SQLiteDatabase<T> : IDatabaseRepository<T> {
    private readonly string _dbPath;
    private static SQLiteDatabase<T>? _mapper;
    private static readonly object _lock = new();

    private SQLiteDatabase(string dbPath, ISQLiteMapper<T> mapper)
    {
        SQLitePCL.Batteries_V2.Init(); // Initialize SQLite

        var fullPath = Path.IsPathRooted(dbPath) ? dbPath : Path.Combine(AppContext.BaseDirectory, dbPath);
        _dbPath = $"Data Source={fullPath}";
        _mapper = mapper;

        EnsureSchema();
    }

    public static SQLiteDatabase Create(string dbPath, ISQLiteMapper<T> mapper)
    {
        lock (_lock)
        {
            _instance ??= new SQLiteDatabase<T>(dbPath, mapper);
            return _instance;
        }
    }

    public static SQLiteDatabase getInstance() =>
        _instance ?? throw new InvalidOperationException("SQLiteDatabase not initialized. Call Create(path) first.");

    private void EnsureSchema() {
        using var connection = new SqliteConnection(_dbPath);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = _mapper.CreateTableSQL; 
        cmd.ExecuteNonQuery();
    }

    public IEnumerable<T> Read(int limit)
    {
        var list = new List<T>();
        using var connection = new SqliteConnection(_dbPath);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = _mapper.SelectLimitSQL(limit);
        cmd.Parameters.AddWithValue("$limit", limit);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(_mapper.FromRow(reader));
        }
        return list;
    }

    public IEnumerable<T> ReadAll()
    {
        var list = new List<T>();
        using var connection = new SqliteConnection(_dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = _mapper.SelectAllSQL;
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            list.Add(_mapper.FromRow(reader));
        }
        return list;
    }

    public void Store(T item) {
        using var connection = new SqliteConnection(_dbPath);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = _mapper.InsertSQL;
        _mapper.BindInsertParameters(cmd, item);

        cmd.ExecuteNonQuery();
    }
}
