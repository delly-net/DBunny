# Delly.DBunny.Sqlite

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](../../LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

SQLite provider implementation for DBunny.

## Installation

```bash
dotnet add package Delly.DBunny.Sqlite
```

## Quick Start

```csharp
using Delly.DBunny;
using Delly.DBunny.Sqlite;
using Delly.DBunny.Sql.Extension;
using System.Data.Common;

// Create provider
var provider = new SqliteProvider();
var connectionString = "Data Source=mydb.db;Pooling=False";

using var connection = provider.GetDbConnection(connectionString);
connection.Open();

// Create a table
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "TEXT(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true }
};
var createTableSql = provider.SqlProvider.CreateTable(string.Empty, "Users", columnDescriptors);

using var createCommand = provider.GetDbCommand(connection);
createCommand.CommandText = createTableSql.Sql;
await createCommand.ExecuteNonQueryAsync();

// Insert data
var insertSql = new Sqled("INSERT INTO [Users] (Name, Age) VALUES (@name, @age)")
    .Set("name", "John Doe")
    .Set("age", 30);

using var insertCommand = provider.GetDbCommand(connection);
insertCommand.CommandText = insertSql.Sql;
provider.SetParameters(insertCommand, insertSql.Parameters);
await insertCommand.ExecuteNonQueryAsync();

// Query data
var selectSql = new Sqled("SELECT * FROM [Users] WHERE Age > @minAge")
    .Set("minAge", 18);

await provider.ReadAsync(connection, selectSql, async reader =>
{
    while (await reader.ReadAsync())
    {
        var id = reader["Id"];
        var name = reader["Name"];
        var age = reader["Age"];
        Console.WriteLine($"Id: {id}, Name: {name}, Age: {age}");
    }
});
```

## Connection Builder

Use the fluent builder for connection configuration:

```csharp
using Delly.DBunny.Sqlite;
using Delly.DBunny.Connecting.Extension;

var connectionDefine = new SqliteConnectionDefine()
    .WithDataSource("mydb.db")
    .WithPooling(false)
    .WithForeignKeys(true)
    .WithCacheSize(2000)
    .WithDefaultTimeout(30)
    .WithJournalMode("WAL");

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    SqliteConnectionDefine.DATABASE_TYPE, "Default");

var connectionString = descriptor.ConnectionString;
```

## Connection Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| Data Source | - | Database file path |
| Version | 3 | SQLite version |
| Password | - | Database encryption password |
| Page Size | 4096 | Page size in bytes |
| Cache Size | -2000 | Cache size in KB |
| Mode | ReadWriteCreate | File open mode |
| Default Timeout | 30 | Command timeout in seconds |
| Journal Mode | Delete | Journal mode (Delete, Truncate, Persist, Memory, WAL, Off) |
| Pooling | True | Enable connection pooling |
| Foreign Keys | False | Enable foreign key constraints |
| Fail If Missing | False | Fail if database file doesn't exist |
| Read Only | False | Open in read-only mode |
| Legacy Format | False | Use legacy file format |
| DateTime Format | ISO8601 | DateTime format |
| DateTime Kind | Unspecified | DateTime kind (Unspecified, Utc, Local) |

## SQLite Features

- **No Database Layer**: SQLite uses a file-based storage, no database operations
- **No Schema Layer**: SQLite doesn't use schemas
- **Name Quoting**: Uses square brackets `[name]`
- **Parameter Prefix**: `@`
- **Type Mapping**:
  - Boolean, TinyInt, Int32, Int64 → INTEGER
  - Single, Double, Decimal → REAL
  - String, DateTime → TEXT

## Dependencies

- [System.Data.SQLite.Core](https://www.nuget.org/packages/System.Data.SQLite.Core/) 1.0.119

## License

[MIT License](../../LICENSE)