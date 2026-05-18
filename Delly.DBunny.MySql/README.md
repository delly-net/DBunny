# Delly.DBunny.MySql

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](../../LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

MySQL provider implementation for DBunny. Also compatible with MariaDB.

## Installation

```bash
dotnet add package Delly.DBunny.MySql
```

## Quick Start

```csharp
using Delly.DBunny;
using Delly.DBunny.MySql;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

// Create connection using builder
var connectionDefine = new MySqlConnectionDefine()
    .WithServer("localhost")
    .WithPort(3306)
    .WithDatabase("mydb")
    .WithUserId("root")
    .WithPassword("password")
    .WithCharset("utf8mb4");

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    MySqlConnectionDefine.DATABASE_TYPE, "Default");

var provider = new MySqlProvider();
using var connection = provider.GetDbConnection(descriptor.ConnectionString);
connection.Open();

// Create a table
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true }
};
var createTableSql = provider.SqlProvider.CreateTable(string.Empty, "Users", columnDescriptors);

using var createCommand = provider.GetDbCommand(connection);
createCommand.CommandText = createTableSql.Sql;
await createCommand.ExecuteNonQueryAsync();

// Insert data
var insertSql = new Sqled("INSERT INTO `Users` (Name, Age) VALUES (@name, @age)")
    .Set("name", "John Doe")
    .Set("age", 30);

using var insertCommand = provider.GetDbCommand(connection);
insertCommand.CommandText = insertSql.Sql;
provider.SetParameters(insertCommand, insertSql.Parameters);
await insertCommand.ExecuteNonQueryAsync();

// Query data
var selectSql = new Sqled("SELECT * FROM `Users` WHERE Age > @minAge")
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
using Delly.DBunny.MySql;
using Delly.DBunny.Connecting.Extension;

var connectionDefine = new MySqlConnectionDefine()
    .WithServer("localhost")
    .WithPort(3306)
    .WithDatabase("mydb")
    .WithUserId("root")
    .WithPassword("password")
    .WithCharset("utf8mb4")
    .WithSslMode("None")
    .WithAllowPublicKeyRetrieval(true)
    .WithConnectionTimeout(30)
    .WithDefaultCommandTimeout(600)
    .WithPooling(true)
    .WithMinPoolSize(0)
    .WithMaxPoolSize(100);

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    MySqlConnectionDefine.DATABASE_TYPE, "Default");
```

## Connection Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| Server | localhost | MySQL server host |
| Port | 3306 | MySQL server port |
| Database | - | Database name |
| User Id | - | Username |
| Password | - | Password |
| Charset | utf8mb4 | Character set |
| SSL Mode | Required | SSL mode (None, Preferred, Required, DisableCAVerification, VerifyCA, VerifyFull) |
| Allow Public Key Retrieval | True | Allow public key retrieval (for authentication) |
| Connection Timeout | 30 | Connection timeout in seconds |
| Default Command Timeout | 600 | Command timeout in seconds |
| Pooling | True | Enable connection pooling |
| Minimum Pool Size | 0 | Minimum pool size |
| Maximum Pool Size | 100 | Maximum pool size |
| Persist Security Info | False | Persist security info in connection string |
| Allow Zero DateTime | False | Allow zero datetime values (0000-00-00) |
| Convert Zero DateTime | True | Convert zero datetime to DateTime.MinValue |

## MySQL Features

- **Database Layer**: MySQL uses databases (no separate schema layer)
- **No Schema Layer**: MySQL doesn't use separate schemas
- **Name Quoting**: Uses backticks `` `name` ``
- **Parameter Prefix**: `@`
- **Auto Increment**: Uses `AUTO_INCREMENT` for auto-incrementing primary keys
- **Type Mapping**:
  - Boolean → TINYINT(1)
  - Byte, SByte → TINYINT(3)
  - Int16, UInt16 → SMALLINT
  - Int32, UInt32 → INT
  - Int64, UInt64 → BIGINT
  - Single → FLOAT
  - Double → DOUBLE
  - Decimal → DECIMAL
  - DateTime → DATETIME
  - String (<=65535 chars) → VARCHAR
  - String (>65535 chars) → TEXT

## MariaDB Compatibility

This provider also works with MariaDB using the same connection parameters and MySqlConnector driver.

## SSL Mode Options

- **None**: No SSL (not recommended for production)
- **Preferred**: Try SSL first, fall back to non-SSL
- **Required**: SSL required (but certificate not verified)
- **VerifyCA**: SSL required and certificate authority verified
- **VerifyFull**: SSL required with full certificate verification

## Zero DateTime Handling

MySQL supports "zero" datetime values (0000-00-00). To handle these:

```csharp
// Allow reading zero datetime values
connectionDefine.WithAllowZeroDateTime(true);

// Convert zero datetime to DateTime.MinValue (default enabled)
connectionDefine.WithConvertZeroDateTime(true);
```

## Dependencies

- [MySqlConnector](https://www.nuget.org/packages/MySqlConnector/) 2.4.0

## License

[MIT License](../../LICENSE)