# DBunny

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
using Delly.DBunny.Sqlite;

// Create a SQLite provider
var provider = new SqliteProvider("Data Source=mydb.db");

// Execute a query
var sql = new Sqled("SELECT * FROM Users WHERE Name = @name");
sql.Set("name", "John");

var result = provider.Execute(sql);

// Read data
foreach (DataRow row in result.Tables[0].Rows)
{
    Console.WriteLine($"Id: {row["Id"]}, Name: {row["Name"]}");
}
```

## Core Concepts

### Sqled

A wrapper class for SQL commands that combines SQL text with parameters:

```csharp
var sql = new Sqled("SELECT * FROM Users WHERE Age > @minAge");
sql.Set("minAge", 18);

// Fluent API
sql.Append(" AND Status = @status")
    .Set("status", "Active");
```

### Key Interfaces

#### IDbProvider

The main database provider interface for:

- Creating database connections and commands
- Setting parameters
- Executing queries (returns DataSet)
- Reading data synchronously or asynchronously
- Getting database metadata (schemas, tables, columns, indexes)

#### ISqlProvider

Generates database-specific SQL for:

- Schema operations (create, get)
- Table operations (create, get)
- Column operations (create, rename, copy, drop)
- Index operations (create, get)
- Type conversions between .NET types and database types

## Supported Databases

| Database | Provider | Status |
|----------|----------|--------|
| SQLite | `Delly.DBunny.Sqlite` | ✓ Stable |
| MySQL | Coming soon | Planned |
| PostgreSQL | Coming soon | Planned |
| SQL Server | Coming soon | Planned |

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

© 2025 [delly.net](https://delly.net)