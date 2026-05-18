# Delly.DBunny.Oracle

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](../../LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

Oracle provider implementation for DBunny. Supports Oracle Database 11g and later.

## Installation

```bash
dotnet add package Delly.DBunny.Oracle
```

## Quick Start

```csharp
using Delly.DBunny;
using Delly.DBunny.Oracle;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

// Create connection using builder
var connectionDefine = new OracleConnectionDefine()
    .WithDataSource("localhost:1521/ORCL")
    .WithUserId("system")
    .WithPassword("password");

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    OracleConnectionDefine.DATABASE_TYPE, "Default");

var provider = new OracleProvider();
using var connection = provider.GetDbConnection(descriptor.ConnectionString);
connection.Open();

// Create a table
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR2(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "NUMBER(10)", PrimaryKeyFlag = false, NullableFlag = true }
};
var createTableSql = provider.SqlProvider.CreateTable(string.Empty, "Users", columnDescriptors);

using var createCommand = provider.GetDbCommand(connection);
createCommand.CommandText = createTableSql.Sql;
await createCommand.ExecuteNonQueryAsync();

// Insert data
var insertSql = new Sqled("INSERT INTO \"Users\" (Name, Age) VALUES (:name, :age)")
    .Set("name", "John Doe")
    .Set("age", 30);

using var insertCommand = provider.GetDbCommand(connection);
insertCommand.CommandText = insertSql.Sql;
provider.SetParameters(insertCommand, insertSql.Parameters);
await insertCommand.ExecuteNonQueryAsync();

// Query data
var selectSql = new Sqled("SELECT * FROM \"Users\" WHERE Age > :minAge")
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
using Delly.DBunny.Oracle;
using Delly.DBunny.Connecting.Extension;

var connectionDefine = new OracleConnectionDefine()
    .WithDataSource("localhost:1521/ORCL")
    .WithUserId("system")
    .WithPassword("password")
    .WithConnectionTimeout(30)
    .WithPooling(true)
    .WithMinPoolSize(0)
    .WithMaxPoolSize(100);

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    OracleConnectionDefine.DATABASE_TYPE, "Default");
```

## Connection Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| Data Source | - | Oracle data source (host:port/service or TNS name) |
| User Id | - | Username |
| Password | - | Password |
| Connection Timeout | 30 | Connection timeout in seconds |
| Pooling | True | Enable connection pooling |
| Min Pool Size | 0 | Minimum pool size |
| Max Pool Size | 100 | Maximum pool size |

## Oracle Features

- **No Database Layer**: Oracle doesn't support database-level operations at SQL level
- **Schema Layer**: Full schema support (schema = user)
- **Name Quoting**: Uses double quotes `"name"`
- **Parameter Prefix**: `:`
- **Sequences**: Uses sequences for auto-incrementing columns (SEQUENCE + TRIGGER)
- **Type Mapping**:
  - Boolean → NUMBER(1)
  - Byte, SByte → NUMBER(3)
  - Int16, UInt16 → NUMBER(5)
  - Int32, UInt32 → NUMBER(10)
  - Int64, UInt64 → NUMBER(19)
  - Single → BINARY_FLOAT
  - Double → BINARY_DOUBLE
  - Decimal → NUMBER
  - DateTime → TIMESTAMP
  - String (<=4000 chars) → VARCHAR2
  - String (>4000 chars) → CLOB

## Data Source Format

The Data Source parameter can be specified in several ways:

### Easy Connect Format

```csharp
// host:port/service
.WithDataSource("localhost:1521/ORCL")

// Using SID (legacy)
.WithDataSource("localhost:1521:XE")
```

### TNS Names

If you have a tnsnames.ora file configured, use the TNS alias:

```csharp
.WithDataSource("ORCL")
```

### Full Connection Descriptor

```csharp
.WithDataSource("(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)))")
```

## Schema in Oracle

In Oracle, each user has their own schema. When you connect with a user, you're working in that user's schema by default.

```csharp
// Get tables in current schema (user)
var tables = await provider.GetTables(connection, string.Empty);

// Get tables in specific schema
var tables = await provider.GetTables(connection, "OTHER_USER");
```

## Auto-Increment Columns

Oracle doesn't have native AUTO_INCREMENT like MySQL. Instead, you use sequences:

```csharp
// Create a sequence first
await ExecuteNonQueryAsync(connection, "CREATE SEQUENCE seq_users_id START WITH 1 INCREMENT BY 1");

// Then create table with default value
await ExecuteNonQueryAsync(connection, "INSERT INTO \"Users\" (Id, Name) VALUES (seq_users_id.NEXTVAL, 'John')");
```

## Dependencies

- [Oracle.ManagedDataAccess.Core](https://www.nuget.org/packages/Oracle.ManagedDataAccess.Core/) 2.19.180

## License

[MIT License](../../LICENSE)