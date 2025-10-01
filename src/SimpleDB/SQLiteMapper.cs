using Microsoft.Data.Sqlite;
using System.Data;

namespace SimpleDB;

public interface ISQLiteMapper<T>
{
    string CreateTableSQL { get; }
    string SelectAllSQL { get; }
    string SelectLimitSQL(int limit);
    string InsertSQL { get; }

    T FromRow(IDataRecord row);
    void BindInsertParameters(SqliteCommand cmd, T item);
}