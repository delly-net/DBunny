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

## Packages

| Package | Description | Platform | NuGet |
|---------|-------------|----------|-------|
| [Delly.DBunny.Core](Delly.DBunny.Core/) | Core library with interfaces and base types | Cross-platform | - |
| [Delly.DBunny.Sqlite](Delly.DBunny.Sqlite/) | SQLite provider implementation | Cross-platform | - |
| [Delly.DBunny.MySql](Delly.DBunny.MySql/) | MySQL provider implementation | Cross-platform | - |
| [Delly.DBunny.PostgreSql](Delly.DBunny.PostgreSql/) | PostgreSQL provider implementation | Cross-platform | - |
| [Delly.DBunny.Oracle](Delly.DBunny.Oracle/) | Oracle provider implementation | Cross-platform | - |
| [Delly.DBunny.SqlServer](Delly.DBunny.SqlServer/) | SQL Server provider implementation | Cross-platform | - |
| [Delly.DBunny.MsAccess](Delly.DBunny.MsAccess/) | Microsoft Access provider (.accdb & .mdb) | Windows only | - |

## Installation

### Core Package

```bash
dotnet add package Delly.DBunny.Core
```

### SQLite Provider

```bash
dotnet add package Delly.DBunny.Sqlite
```

### MySQL Provider

```bash
dotnet add package Delly.DBunny.MySql
```

### PostgreSQL Provider

```bash
dotnet add package Delly.DBunny.PostgreSql
```

### Oracle Provider

```bash
dotnet add package Delly.DBunny.Oracle
```

### SQL Server Provider

```bash
dotnet add package Delly.DBunny.SqlServer
```

### Microsoft Access Provider

> **Windows Only:** The Access provider is only supported on Windows.

```bash
dotnet add package Delly.DBunny.MsAccess
```

## Quick Start

### SQLite Example

```csharp
using Delly.DBunny;
using Delly.DBunny.Sqlite;
using Delly.DBunny.Sql.Extension;
using System.Data.Common;

// Create SQLite provider directly
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

// Execute a query
var sql = new Sqled("SELECT * FROM Users WHERE Age > @minAge")
    .Set("minAge", 18);

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

### SQL Server Example

```csharp
using Delly.DBunny;
using Delly.DBunny.SqlServer;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

// Define connection using builder
var connectionDefine = new SqlServerConnectionDefine()
    .WithServer("localhost")
    .WithDatabase("mydb")
    .WithUserId("sa")
    .WithPassword("your_password")
    .WithTrustServerCertificate(true);

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    SqlServerConnectionDefine.DATABASE_TYPE, "Default");

var provider = new SqlServerProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// Create a table in dbo schema
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "NVARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true }
};
var createTableSql = provider.SqlProvider.CreateTable("dbo", "Users", columnDescriptors);

using var createCommand = provider.GetDbCommand(connection);
createCommand.CommandText = createTableSql.Sql;
await createCommand.ExecuteNonQueryAsync();

// ... same query operations as SQLite
```

### MySQL Example

```csharp
using Delly.DBunny;
using Delly.DBunny.MySql;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

var connectionDefine = new MySqlConnectionDefine()
    .WithServer("localhost")
    .WithPort(3306)
    .WithDatabase("mydb")
    .WithUserId("root")
    .WithPassword("password")
    .WithCharset("utf8mb4");

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    MySqlConnectionDefine.DATABASE_TYPE, "Default");

var provider = new MySqlProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// ... same query operations as SQLite (uses backticks for name quoting)
```

### PostgreSQL Example

```csharp
using Delly.DBunny;
using Delly.DBunny.PostgreSql;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

var connectionDefine = new PostgreSqlConnectionDefine()
    .WithHost("localhost")
    .WithPort(5432)
    .WithDatabase("mydb")
    .WithUsername("postgres")
    .WithPassword("password");

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    PostgreSqlConnectionDefine.DATABASE_TYPE, "Default");

var provider = new PostgreSqlProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// ... same query operations as SQLite (uses double quotes for name quoting)
```

### Oracle Example

```csharp
using Delly.DBunny;
using Delly.DBunny.Oracle;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

var connectionDefine = new OracleConnectionDefine()
    .WithDataSource("localhost:1521/ORCL")
    .WithUserId("system")
    .WithPassword("password");

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    OracleConnectionDefine.DATABASE_TYPE, "Default");

var provider = new OracleProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// ... same query operations as SQLite (note: Oracle uses : parameter prefix)
```

### Microsoft Access Example

> **Note:** The Access provider is Windows-only as it depends on Microsoft Access Database Engine (OLE DB).

```csharp
using Delly.DBunny;
using Delly.DBunny.MsAccess;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

// For modern .accdb files
var connectionDefine = new MsAccessConnectionDefine()
    .WithDataSource("mydb.accdb")
    .WithProviderAce()
    .WithPooling(false)
    .WithReadWrite();

// For legacy .mdb files (32-bit only)
var legacyConnectionDefine = new MsAccessConnectionDefine()
    .WithDataSource("mydb.mdb")
    .WithProviderJet()
    .WithPooling(false);

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    MsAccessConnectionDefine.DATABASE_TYPE, "Default");

var provider = new MsAccessProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// Note: Access uses position-based parameters (?)
var insertSql = new Sqled("INSERT INTO [Users] (Name, Age) VALUES (?, ?)")
    .Set("name", "John Doe")
    .Set("age", 30);

// ... same query operations as SQLite
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

- Database operations (create, get, drop)
- Schema operations (create, get, drop)
- Table operations (create, get, drop)
- Column operations (create, rename, modify, copy, drop)
- Index operations (create, get, drop)
- Type conversions between .NET types and database types

```csharp
// Create a table using SQL provider
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "TEXT(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true }
};

var createTableSql = provider.SqlProvider.CreateTable(string.Empty, "Users", columnDescriptors);

using var command = provider.GetDbCommand(connection);
command.CommandText = createTableSql.Sql;
await command.ExecuteNonQueryAsync();
```

## Database Comparison

| Feature | SQLite | MySQL | PostgreSQL | Oracle | SQL Server | MsAccess |
|---------|--------|-------|------------|--------|------------|----------|
| Platform | Cross-platform | Cross-platform | Cross-platform | Cross-platform | Cross-platform | Windows only |
| HasDatabase | No | Yes | Yes | No | Yes | No |
| HasSchema | No | No | Yes | Yes | Yes | No |
| Name Quoting | `[name]` | `` `name` `` | `"name"` | `"name"` | `[name]` | `[name]` |
| Parameter Prefix | `@` | `@` | `@` | `:` | `@` | `?` (positional) |
| Primary Key | NOT NULL PRIMARY KEY | AUTO_INCREMENT PRIMARY KEY | NOT NULL PRIMARY KEY | NOT NULL PRIMARY KEY | IDENTITY(1,1) PRIMARY KEY | AUTOINCREMENT |
| Decimal Type | REAL | DECIMAL | NUMERIC | NUMBER | DECIMAL | CURRENCY |
| DateTime Type | TEXT | DATETIME | TIMESTAMP | TIMESTAMP | DATETIME | DATETIME |
| Large Text | TEXT | TEXT | TEXT | CLOB | NVARCHAR(MAX) | LONGTEXT |

## Links

- [Documentation](#)
- [Issues](https://github.com/delly-net/DBunny/issues)
- [Releases](https://github.com/delly-net/DBunny/releases)

## License

[MIT License](LICENSE)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

© 2025 [delly.net](https://delly.net)