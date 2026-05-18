# Delly.DBunny.SqlServer

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](../../LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

SQL Server provider implementation for DBunny.

## Installation

```bash
dotnet add package Delly.DBunny.SqlServer
```

## Quick Start

```csharp
using Delly.DBunny;
using Delly.DBunny.SqlServer;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

// Create connection using builder
var connectionDefine = new SqlServerConnectionDefine()
    .WithServer("localhost")
    .WithDatabase("mydb")
    .WithUserId("sa")
    .WithPassword("your_password")
    .WithTrustServerCertificate(true);

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    SqlServerConnectionDefine.DATABASE_TYPE, "Default");

var provider = new SqlServerProvider();
using var connection = provider.GetDbConnection(descriptor.ConnectionString);
connection.Open();

// Create a table
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

// Insert data
var insertSql = new Sqled("INSERT INTO [dbo].[Users] (Name, Age) VALUES (@name, @age)")
    .Set("name", "John Doe")
    .Set("age", 30);

using var insertCommand = provider.GetDbCommand(connection);
insertCommand.CommandText = insertSql.Sql;
provider.SetParameters(insertCommand, insertSql.Parameters);
await insertCommand.ExecuteNonQueryAsync();

// Query data
var selectSql = new Sqled("SELECT * FROM [dbo].[Users] WHERE Age > @minAge")
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
using Delly.DBunny.SqlServer;
using Delly.DBunny.Connecting.Extension;

var connectionDefine = new SqlServerConnectionDefine()
    .WithServer("localhost")
    .WithDatabase("mydb")
    .WithUserId("sa")
    .WithPassword("your_password")
    .WithTrustServerCertificate(true)
    .WithEncrypt(false)
    .WithConnectionTimeout(30)
    .WithCommandTimeout(60)
    .WithPooling(true)
    .WithMinPoolSize(0)
    .WithMaxPoolSize(100)
    .WithMultipleActiveResultSets(true);

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    SqlServerConnectionDefine.DATABASE_TYPE, "Default");
```

## Connection Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| Server / Data Source | localhost | SQL Server server host |
| Database / Initial Catalog | - | Database name |
| User Id / User | - | Username |
| Password / Pwd | - | Password |
| Trust Server Certificate | True | Trust server certificate |
| Encrypt | True | Encrypt connection |
| Connection Timeout | 30 | Connection timeout in seconds |
| Command Timeout | 600 | Command timeout in seconds |
| Pooling | True | Enable connection pooling |
| Min Pool Size | 0 | Minimum pool size |
| Max Pool Size | 100 | Maximum pool size |
| Multiple Active Result Sets | True | Enable MARS |
| Packet Size | 8000 | Network packet size in bytes |
| Application Name | - | Application name |

## SQL Server Features

- **Database Layer**: SQL Server uses databases with full support
- **Schema Layer**: SQL Server uses schemas (default: dbo)
- **Name Quoting**: Uses square brackets `[name]`
- **Parameter Prefix**: `@`
- **Identity Columns**: Uses `IDENTITY(1,1)` for auto-increment
- **Type Mapping**:
  - Boolean → BIT
  - TinyInt → TINYINT
  - Int32 → INT
  - Int64 → BIGINT
  - Single → REAL
  - Double → FLOAT
  - Decimal → DECIMAL
  - DateTime → DATETIME
  - String → NVARCHAR
  - Large String → NVARCHAR(MAX)

## Schema Operations

SQL Server supports schema creation and management:

```csharp
// Create a schema
var createSchemaSql = provider.SqlProvider.CreateSchema("myschema", null!);
await ExecuteNonQueryAsync(connection, createSchemaSql);

// Get all schemas
var getSchemasSql = provider.SqlProvider.GetSchemas();
await provider.ReadAsync(connection, getSchemasSql, async reader =>
{
    while (await reader.ReadAsync())
    {
        var schemaName = reader.GetString(0);
        Console.WriteLine($"Schema: {schemaName}");
    }
});

// Drop a schema
var dropSchemaSql = provider.SqlProvider.DropSchema("myschema");
await ExecuteNonQueryAsync(connection, dropSchemaSql);
```

## Dependencies

- [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 6.0.1

## License

[MIT License](../../LICENSE)