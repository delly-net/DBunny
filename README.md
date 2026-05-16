# DBunny

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

A lightweight .NET database abstraction layer that provides a unified API for working with multiple database types. DBunny uses a provider pattern to enable database-specific implementations while maintaining a common interface.

## Features

- **Multi-Database Support**: Write database-agnostic code, switch providers as needed
- **Lightweight**: Minimal overhead with a clean, simple API
- **Provider Pattern**: Easy to extend with new database providers
- **Async Support**: Full async/await support for database operations
- **Multi-Target**: Supports .NET Standard 2.0, .NET 5.0, and .NET 8.0
- **AOT Compatible**: Ready for Native AOT compilation on .NET 8.0
- **Null Reference Types**: Enabled for .NET 5.0 and later

## Installation

### Core Package

```bash
dotnet add package Delly.DBunny
```

### SQLite Provider

```bash
dotnet add package Delly.DBunny.Sqlite
```

## Quick Start

```csharp
using Delly.DBunny;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.Providing.Extension;
using Delly.DBunny.Sqlite;
using Delly.DBunny.Sql.Extension;
using System.Data.Common;

// Define connection settings
var connectionDefine = new SqliteConnectionDefine()
    .WithDataSource("Data Source=mydb.db")
    .WithPooling(false)
    .WithForeignKeys(true);

// Create connection descriptor
var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    SqliteConnectionDefine.DATABASE_TYPE, "Default");

// Create connection factory
var connectionFactory = new DefaultDbConnectionFactory(connectionDescriptor);

// Create provider factory with SQLite provider
var providerFactory = new DefaultDbProviderFactory(new SqliteProvider());

// Get provider and connection
var provider = providerFactory.GetProvider(
    connectionFactory.GetDefaultConnection().DatabaseType)!;

using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// Execute a query
var sql = new Sqled("SELECT * FROM Users WHERE Name = @name")
    .Set("name", "John");

using var command = provider.GetDbCommand(connection);
command.CommandText = sql.Sql;
provider.SetParameters(command, sql.Parameters);

// Read data
await provider.ReadAsync(connection, sql, async reader =>
{
    while (await reader.ReadAsync())
    {
        var id = reader["Id"];
        var name = reader["Name"];
        Console.WriteLine($"Id: {id}, Name: {name}");
    }
});
```

## Core Concepts

### Sqled

A wrapper class for SQL commands that combines SQL text with parameters:

```csharp
var sql = new Sqled("SELECT * FROM Users WHERE Age > @minAge")
    .Set("minAge", 18);

// Fluent API for building queries
sql.Append(" AND Status = @status")
    .Set("status", "Active");

// For complex SQL, use Builder directly
var createTableSql = new Sqled();
createTableSql.Builder.AppendLine("CREATE TABLE [Users](");
createTableSql.Builder.Append("    [Id] INTEGER NOT NULL PRIMARY KEY,");
createTableSql.Builder.Append("    [Name] TEXT(100) NOT NULL,");
createTableSql.Builder.AppendLine("    [Age] INTEGER NULL");
createTableSql.Builder.AppendLine(");");
```

### Key Interfaces

#### IDbProvider

The main database provider interface for:

- Creating database connections and commands
- Setting parameters
- Executing queries (returns DataSet)
- Reading data synchronously or asynchronously
- Getting database metadata (schemas, tables, columns, indexes)

```csharp
// Execute non-query (INSERT, UPDATE, DELETE)
var insertSql = new Sqled("INSERT INTO [Users] (Name, Age) VALUES (@name, @age)")
    .Set("name", "John Doe")
    .Set("age", 30);

using var command = provider.GetDbCommand(connection);
command.CommandText = insertSql.Sql;
provider.SetParameters(command, insertSql.Parameters);
await command.ExecuteNonQueryAsync();
```

#### ISqlProvider

Generates database-specific SQL for:

- Schema operations (create, get)
- Table operations (create, get)
- Column operations (create, rename, copy, drop)
- Index operations (create, get)
- Type conversions between .NET types and database types

```csharp
// Create a table using SQL provider
var columnDefines = new List<Sqled>
{
    provider.SqlProvider.ColumnDefine("Id", "INTEGER", true, false),
    provider.SqlProvider.ColumnDefine("Name", "TEXT(100)", false, false),
    provider.SqlProvider.ColumnDefine("Age", "INTEGER", false, true)
};

var createTableSql = provider.SqlProvider.CreateTable(
    string.Empty, "Users", columnDefines);

await ExecuteNonQueryAsync(connection, createTableSql);
```

## Supported Databases

| Database | Provider | Status |
|----------|----------|--------|
| [SQLite](https://www.sqlite.org/) | `Delly.DBunny.Sqlite` | ✅ Stable |
| [MySQL](https://www.mysql.com/) | Coming soon | Planned |
| [PostgreSQL](https://www.postgresql.org/) | Coming soon | Planned |
| [SQL Server](https://www.microsoft.com/sql-server/) | Coming soon | Planned |

## Links

- [Documentation](#) *[待更新]*
- [Issues](https://github.com/delly-net/DBunny/issues)
- [Releases](https://github.com/delly-net/DBunny/releases)

## License

[MIT License](LICENSE)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

© 2025 [delly.net](https://delly.net)