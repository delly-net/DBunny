# Delly.DBunny.PostgreSql

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](../../LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

PostgreSQL provider implementation for DBunny.

## Installation

```bash
dotnet add package Delly.DBunny.PostgreSql
```

## Quick Start

```csharp
using Delly.DBunny;
using Delly.DBunny.PostgreSql;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

// Create connection using builder
var connectionDefine = new PostgreSqlConnectionDefine()
    .WithHost("localhost")
    .WithPort(5432)
    .WithDatabase("mydb")
    .WithUsername("postgres")
    .WithPassword("password");

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    PostgreSqlConnectionDefine.DATABASE_TYPE, "Default");

var provider = new PostgreSqlProvider();
using var connection = provider.GetDbConnection(descriptor.ConnectionString);
connection.Open();

// Create a table
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true }
};
var createTableSql = provider.SqlProvider.CreateTable(string.Empty, "Users", columnDescriptors);

using var createCommand = provider.GetDbCommand(connection);
createCommand.CommandText = createTableSql.Sql;
await createCommand.ExecuteNonQueryAsync();

// Insert data
var insertSql = new Sqled("INSERT INTO \"Users\" (Name, Age) VALUES (@name, @age)")
    .Set("name", "John Doe")
    .Set("age", 30);

using var insertCommand = provider.GetDbCommand(connection);
insertCommand.CommandText = insertSql.Sql;
provider.SetParameters(insertCommand, insertSql.Parameters);
await insertCommand.ExecuteNonQueryAsync();

// Query data
var selectSql = new Sqled("SELECT * FROM \"Users\" WHERE Age > @minAge")
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
using Delly.DBunny.PostgreSql;
using Delly.DBunny.Connecting.Extension;

var connectionDefine = new PostgreSqlConnectionDefine()
    .WithHost("localhost")
    .WithPort(5432)
    .WithDatabase("mydb")
    .WithUsername("postgres")
    .WithPassword("password")
    .WithSearchPath("public")
    .WithSslMode("Disable")
    .WithTrustServerCertificate(false)
    .WithTimeout(30)
    .WithCommandTimeout(60)
    .WithPooling(true)
    .WithMinPoolSize(0)
    .WithMaxPoolSize(100);

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    PostgreSqlConnectionDefine.DATABASE_TYPE, "Default");
```

## Connection Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| Host | localhost | PostgreSQL server host |
| Port | 5432 | PostgreSQL server port |
| Database | - | Database name |
| Username | - | Username |
| Password | - | Password |
| Search Path | - | Schema search path |
| SSL Mode | Require | SSL mode (Disable, Allow, Prefer, Require) |
| Trust Server Certificate | False | Trust server certificate |
| Timeout | 30 | Connection timeout in seconds |
| Command Timeout | 30 | Command timeout in seconds |
| Pooling | True | Enable connection pooling |
| Min Pool Size | 0 | Minimum pool size |
| Max Pool Size | 100 | Maximum pool size |
| Keep Alive | 0 | TCP keep-alive time in seconds |
| Keep Alive Idle | 0 | TCP keep-alive idle time |
| Timezone | UTC | Timezone |
| Encoding | UTF8 | Character encoding |

## PostgreSQL Features

- **Database Layer**: Full database support
- **Schema Layer**: Full schema support (default: public)
- **Name Quoting**: Uses double quotes `"name"`
- **Parameter Prefix**: `@`
- **Type Mapping**:
  - Boolean → BOOLEAN
  - TinyInt → SMALLINT
  - Int32 → INTEGER
  - Int64 → BIGINT
  - Single → REAL
  - Double → DOUBLE PRECISION
  - Decimal → NUMERIC
  - DateTime → TIMESTAMP
  - String → VARCHAR
  - Large String → TEXT

## Dependencies

- [Npgsql](https://www.nuget.org/packages/Npgsql/) 8.0.3

## License

[MIT License](../../LICENSE)