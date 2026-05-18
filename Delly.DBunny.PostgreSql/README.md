# Delly.DBunny.PostgreSql

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](../../LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

PostgreSQL provider implementation for DBunny. Also compatible with compatible databases like CockroachDB.

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

// Create a table in public schema
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true }
};
var createTableSql = provider.SqlProvider.CreateTable("public", "Users", columnDescriptors);

using var createCommand = provider.GetDbCommand(connection);
createCommand.CommandText = createTableSql.Sql;
await createCommand.ExecuteNonQueryAsync();

// Insert data
var insertSql = new Sqled("INSERT INTO \"public\".\"Users\" (Name, Age) VALUES (@name, @age)")
    .Set("name", "John Doe")
    .Set("age", 30);

using var insertCommand = provider.GetDbCommand(connection);
insertCommand.CommandText = insertSql.Sql;
provider.SetParameters(insertCommand, insertSql.Parameters);
await insertCommand.ExecuteNonQueryAsync();

// Query data
var selectSql = new Sqled("SELECT * FROM \"public\".\"Users\" WHERE Age > @minAge")
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
    .WithSslMode("Prefer")
    .WithTrustServerCertificate(false)
    .WithTimeout(30)
    .WithCommandTimeout(600)
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
| Search Path | public | Schema search path (comma-separated) |
| SSL Mode | Prefer | SSL mode (Disable, Allow, Prefer, Require, VerifyCA, VerifyFull) |
| Trust Server Certificate | False | Trust server certificate |
| Timeout | 30 | Connection timeout in seconds |
| Command Timeout | 600 | Command timeout in seconds |
| Pooling | True | Enable connection pooling |
| Minimum Pool Size | 0 | Minimum pool size |
| Maximum Pool Size | 100 | Maximum pool size |

## PostgreSQL Features

- **Database Layer**: Full database support
- **Schema Layer**: Full schema support (default: public)
- **Name Quoting**: Uses double quotes `"name"`
- **Parameter Prefix**: `@`
- **Serial Types**: Uses `SERIAL`/`BIGSERIAL` for auto-incrementing columns
- **Type Mapping**:
  - Boolean → BOOLEAN
  - Byte, SByte → SMALLINT
  - Int16, UInt16 → SMALLINT
  - Int32, UInt32 → INTEGER
  - Int64, UInt64 → BIGINT
  - Single → REAL
  - Double → DOUBLE PRECISION
  - Decimal → NUMERIC
  - DateTime → TIMESTAMP
  - String (<=65535 chars) → VARCHAR
  - String (>65535 chars) → TEXT

## Schema Operations

PostgreSQL supports schema creation and management:

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

## Search Path

The search path determines which schemas are searched for unqualified objects:

```csharp
// Set multiple search paths
connectionDefine.WithSearchPath("public,myschema,other_schema");
```

## SSL Mode Options

- **Disable**: No SSL
- **Allow**: Allow SSL but don't require it
- **Prefer**: Try SSL first, fall back to non-SSL (default)
- **Require**: SSL required (but certificate not verified)
- **VerifyCA**: SSL required and certificate authority verified
- **VerifyFull**: SSL required with full certificate verification

## CockroachDB Compatibility

This provider also works with CockroachDB using the same connection parameters, as CockroachDB is PostgreSQL wire protocol compatible.

## Dependencies

- [Npgsql](https://www.nuget.org/packages/Npgsql/) 8.0.3

## License

[MIT License](../../LICENSE)