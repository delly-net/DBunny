# Delly.DBunny.Core

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](../../LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

The core library for DBunny, providing interfaces and base types for database abstraction.

## Installation

```bash
dotnet add package Delly.DBunny.Core
```

## Core Types

### IDbProvider

The main database provider interface for managing database connections and commands.

```csharp
public interface IDbProvider
{
    string DatabaseType { get; }
    ISqlProvider SqlProvider { get; }

    DbConnection GetDbConnection(string connectionString);
    DbCommand GetDbCommand(DbConnection connection);
    void SetParameters(DbCommand command, IEnumerable<KeyValuePair<string, object>> parameters);
    DataSet GetDataSet(DbCommand command);

    Task<IReadOnlyList<string>> GetSchemas(DbConnection connection);
    Task<IReadOnlyList<DbTableDesciptor>> GetTables(DbConnection connection, string schema);
    Task<IReadOnlyList<DbColumnDesciptor>> GetColumns(DbConnection connection, string schema, string table);
    Task<IReadOnlyList<DbIndexDesciptor>> GetIndexes(DbConnection connection, string schema, string table);
}
```

### ISqlProvider

Interface for generating database-specific SQL statements.

```csharp
public interface ISqlProvider
{
    string GetSpecialName(string name);
    string GetSpecialTypeName(TypeCode typeCode, int length = 0, int precision = 0);
    string GetSpecialTypeName(ColumnType columnType, int length = 0, int precision = 0);

    bool HasDatabase { get; }
    bool HasSchema { get; }

    Sqled GetDatabases();
    Sqled CreateDatabase(string database, IDictionary<string, object> options);
    Sqled DropDatabase(string database);

    Sqled GetSchemas();
    Sqled CreateSchema(string schema, IDictionary<string, object> options);
    Sqled DropSchema(string schema);

    Sqled GetTables(string schema);
    Sqled CreateTable(string schema, string table, IList<DbColumnDesciptor> columnDesciptors);
    Sqled DropTable(string schema, string table);

    Sqled GetColumns(string schema, string table);
    Sqled CreateColumn(DbColumnDesciptor columnDesciptor);
    Sqled RenameColumn(string schema, string table, string column, string columnTarget);
    Sqled ModifyColumn(DbColumnDesciptor column, DbColumnDesciptor columnTarget);
    Sqled CopyColumn(string schema, string table, string column, string columnTarget, string columnType);
    Sqled DropColumn(string schema, string table, string column);

    Sqled GetIndexes(string schema, string table);
    Sqled CreateIndex(DbIndexDesciptor indexDesciptor);
    Sqled DropIndex(string schema, string table, string column);
}
```

### Sqled

A wrapper class for SQL commands that combines SQL text with parameters.

```csharp
var sql = new Sqled("SELECT * FROM Users WHERE Name = @name")
    .Set("name", "John");

// Fluent API
sql.Append(" AND Age > @minAge")
    .Set("minAge", 18);

// Access properties
string query = sql.Sql;
Dictionary<string, object> params = sql.Parameters;

// For complex SQL, use Builder directly
var createTableSql = new Sqled();
createTableSql.Builder.AppendLine("CREATE TABLE [Users](");
createTableSql.Builder.Append("    [Id] INTEGER NOT NULL PRIMARY KEY,");
createTableSql.Builder.Append("    [Name] TEXT(100) NOT NULL,");
createTableSql.Builder.AppendLine("    [Age] INTEGER NULL");
createTableSql.Builder.AppendLine(");");
```

### Descriptors

#### DbConnectionDescriptor

Describes a database connection.

```csharp
public class DbConnectionDescriptor
{
    public string Name { get; set; }
    public string DatabaseType { get; set; }
    public string ConnectionString { get; set; }
}
```

#### DbTableDesciptor

Describes a database table.

```csharp
public class DbTableDesciptor
{
    public string SchemaName { get; set; }
    public string TableName { get; set; }
}
```

#### DbColumnDesciptor

Describes a database table column.

```csharp
public class DbColumnDesciptor : DbTableDesciptor
{
    public string ColumnName { get; set; }
    public string ColumnType { get; set; }
    public bool PrimaryKeyFlag { get; set; }
    public bool NullableFlag { get; set; }
}
```

#### DbIndexDesciptor

Describes a database index.

```csharp
public class DbIndexDesciptor : DbTableDesciptor
{
    public string IndexName { get; set; }
    public bool UniqueFlag { get; set; }
    public string ColumnName { get; set; }
}
```

### ColumnType

Enum for database column types.

```csharp
public enum ColumnType
{
    UNKNOW,
    DECIMAL,
    TINY,
    INTEGER,
    LONG,
    TIME,
    VARCHAR,
    TEXT
}
```

### Base Classes

#### BaseConnectionDefine

Abstract base class for connection definitions:

```csharp
public abstract class BaseConnectionDefine : IDbConnectionDefine
{
    protected void Set(string key, object value);
    protected T Get<T>(string key, T defaultValue);
    protected bool ContainsKey(string key);
    protected IDictionary<string, object> GetProperties();
    public string GetConnectionString();
}
```

### Extensions

#### SqledExtension

Fluent methods for building SQL:

```csharp
public static class SqledExtension
{
    public static Sqled Append(this Sqled sqled, string sql);
    public static Sqled Set(this Sqled sqled, string key, object value);
}
```

#### DbProviderExtension

Helper methods for executing queries:

```csharp
public static class DbProviderExtension
{
    public static async Task ReadAsync(this IDbProvider provider, DbConnection connection, Sqled sqled, Func<DbDataReader, Task> readAction);
    public static void Read(this IDbProvider provider, DbConnection connection, Sqled sqled, Action<DbDataReader> readAction);
}
```

#### DbConnectionDefineExtension

Extension methods for connection define:

```csharp
public static class DbConnectionDefineExtension
{
    public static DbConnectionDescriptor GetDbConnectionDescriptor(this IDbConnectionDefine connectionDefine, string databaseType, string name);
}
```

## License

[MIT License](../../LICENSE)