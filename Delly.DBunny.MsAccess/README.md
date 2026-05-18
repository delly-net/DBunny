# Delly.DBunny.MsAccess

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](../../LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
[![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)](https://www.microsoft.com/windows)

Microsoft Access provider implementation for DBunny. Supports both modern .accdb (ACE 12.0+) and legacy .mdb (Jet 4.0) formats.

> **Windows Only:** This provider requires Windows as it depends on Microsoft Access Database Engine (OLE DB), which is not available on other platforms.

## Installation

```bash
dotnet add package Delly.DBunny.MsAccess
```

## Prerequisites

For Access database connectivity, you need to have one of the following installed on your system:

- **For .accdb (Access 2007+)**: Microsoft Access Database Engine 2016 Redistributable
  - Download from [Microsoft](https://www.microsoft.com/en-us/download/details.aspx?id=54920)
  - Provider: `Microsoft.ACE.OLEDB.12.0`

- **For .mdb (Access 2003)**: Jet 4.0 (included with Windows) or ACE driver
  - Provider: `Microsoft.Jet.OLEDB.4.0` (32-bit only, requires 32-bit application)

**Note**: The application's bitness must match the installed Access driver. Use 32-bit build for Jet 4.0 compatibility.

## Quick Start

```csharp
using Delly.DBunny;
using Delly.DBunny.MsAccess;
using Delly.DBunny.Sql.Extension;
using System.Data.Common;

// Create provider (default uses ACE for .accdb)
var provider = new MsAccessProvider();
var connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=mydb.accdb;";

using var connection = provider.GetDbConnection(connectionString);
connection.Open();

// Create a table
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "COUNTER", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true }
};
var createTableSql = provider.SqlProvider.CreateTable(string.Empty, "Users", columnDescriptors);

using var createCommand = provider.GetDbCommand(connection);
createCommand.CommandText = createTableSql.Sql;
await createCommand.ExecuteNonQueryAsync();

// Insert data
var insertSql = new Sqled("INSERT INTO [Users] (Name, Age) VALUES (?, ?)")
    .Set("name", "John Doe")
    .Set("age", 30);

using var insertCommand = provider.GetDbCommand(connection);
insertCommand.CommandText = insertSql.Sql;
provider.SetParameters(insertCommand, insertSql.Parameters);
await insertCommand.ExecuteNonQueryAsync();

// Query data
var selectSql = new Sqled("SELECT * FROM [Users] WHERE Age > ?")
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
using Delly.DBunny.MsAccess;
using Delly.DBunny.Connecting.Extension;

// For modern .accdb files (ACE 12.0)
var connectionDefine = new MsAccessConnectionDefine()
    .WithDataSource("mydb.accdb")
    .WithProviderAce()
    .WithPooling(false)
    .WithReadWrite()
    .WithConnectionTimeout(15)
    .WithDefaultCommandTimeout(30);

// For legacy .mdb files (Jet 4.0 - 32-bit only)
var legacyConnectionDefine = new MsAccessConnectionDefine()
    .WithDataSource("mydb.mdb")
    .WithProviderJet()
    .WithPooling(false);

var descriptor = connectionDefine.GetDbConnectionDescriptor(
    MsAccessConnectionDefine.DATABASE_TYPE, "Default");

var connectionString = descriptor.ConnectionString;
```

## Connection Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| Provider | Microsoft.ACE.OLEDB.12.0 | OLE DB provider (ACE or Jet) |
| Data Source | - | Database file path (.accdb or .mdb) |
| Jet OLEDB:Database Password | - | Database password |
| Persist Security Info | False | Persist security info |
| Mode | Share Deny None | Access mode (Share Deny None, Share Exclusive, Read, Read Write) |
| Connect Timeout | 15 | Connection timeout in seconds |
| Default Command Timeout | 30 | Command timeout in seconds |
| OLE DB Services | -1 | Enable OLE DB services (-1 = all services enabled, 0 = disabled) |
| Min Pool Size | 0 | Minimum pool size |
| Max Pool Size | 100 | Maximum pool size |

## Access-Specific Considerations

### File Formats

- **.accdb**: Modern Access format (2007+), requires ACE 12.0+ provider
- **.mdb**: Legacy format (2003 and earlier), requires Jet 4.0 or ACE provider

### Access Mode Options

- **Share Deny None**: Default, allows shared read/write access
- **Share Deny Read**: Others can write but not read
- **Share Deny Write**: Others can read but not write
- **Share Exclusive**: Exclusive access for this user
- **Read**: Read-only access
- **Read Write**: Read/write access

### Limitations

1. **No Database Operations**: Access databases are files, not servers
2. **No Schema Support**: Access doesn't support schemas
3. **Positional Parameters**: OleDb uses `?` parameters (position-based, not named)
4. **Name Quoting**: Uses square brackets `[name]`
5. **AUTOINCREMENT/COUNTER**: For auto-incrementing primary keys
6. **No Column Rename**: Access doesn't support renaming columns directly
7. **No Column Modify**: Access doesn't support modifying column types directly

### Type Mapping

| .NET Type | Access Type | Notes |
|-----------|-------------|-------|
| Boolean | BIT | - |
| Byte, SByte, Int16, UInt16, Int32, UInt32 | INTEGER | - |
| Int64, UInt64 | LONG | - |
| Single, Double | DOUBLE | - |
| Decimal | CURRENCY | Currency precision |
| DateTime | DATETIME | - |
| Char | CHAR(1) | Fixed-length 1 character |
| String (<=255 chars) | VARCHAR(n) | Variable-length |
| String (>255 chars) | LONGTEXT | Memo/long text |

### Supported Operations

- ✅ Create/Drop tables
- ✅ Add/Drop columns
- ✅ Create/Drop indexes
- ✅ Query/Insert/Update/Delete data
- ✅ Transactions
- ❌ Rename columns (not supported by Access)
- ❌ Modify column types (not supported by Access)
- ❌ Multiple databases (file-based only)

### Workarounds

**Rename Column**: Create a new column, copy data, drop the old column

```csharp
// 1. Add new column
await ExecuteNonQueryAsync(connection, "ALTER TABLE [Users] ADD COLUMN [NewName] VARCHAR(100);");

// 2. Copy data
await ExecuteNonQueryAsync(connection, "UPDATE [Users] SET [NewName] = [OldName];");

// 3. Drop old column
await ExecuteNonQueryAsync(connection, "ALTER TABLE [Users] DROP COLUMN [OldName];");
```

**Modify Column Type**: Create a new table with the new schema, copy data, drop old table

## Known Issues

- Column metadata from `MSysObjects` and `INFORMATION_SCHEMA` may have limited information
- Primary key detection requires additional queries beyond the basic `GetColumns` implementation
- Index uniqueness information is not fully exposed through system tables

## Dependencies

This provider uses .NET's built-in `System.Data.OleDb` - no additional NuGet packages required.

## License

[MIT License](../../LICENSE)