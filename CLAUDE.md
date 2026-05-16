# DBunny

DBunny is a lightweight .NET database abstraction layer that provides a unified API for working with multiple database types. It uses a provider pattern to enable database-specific implementations while maintaining a common interface.

## Project Structure

- **Delly.DBunny** - Core library with interfaces and base types
- **Delly.DBunny.Sqlite** - SQLite provider implementation

## Target Frameworks

The project targets multiple .NET frameworks:
- .NET Standard 2.0
- .NET 5.0
- .NET 8.0 (AOT compatible)

Nullable reference types are enabled for .NET 5.0 and .NET 8.0.

## Architecture

### Core Concepts

**Sqled**: A wrapper class for SQL commands that combines SQL text with parameters. It supports fluent building with `Append()` and parameter setting with `Set()`.

```csharp
var sql = new Sqled("SELECT * FROM Users WHERE Name = @name");
sql.Set("name", "John");
```

### Key Interfaces

**IDbConnectionDefine**: Interface for database connection configuration with ConnectionString property.

**IDbProvider**: The main database provider interface that handles:
- Creating database connections and commands
- Setting parameters
- Executing queries (returns DataSet)
- Reading data synchronously or asynchronously
- Getting database metadata (schemas, tables, columns, indexes)

**ISqlProvider**: Generates database-specific SQL for:
- Schema operations (create, get)
- Table operations (create, get)
- Column operations (create, rename, copy, drop)
- Index operations (create, get)
- Type conversions between .NET types and database types

**IDbConnectionFactory**: Factory for obtaining database connection descriptors by name.

**IDbProviderFactory**: Factory for obtaining database providers by type.

### Base Classes

**BaseConnectionDefine**: Abstract base class for connection definitions:
- Stores key-value pairs for connection parameters
- Generates connection strings from parameters
- Provides protected methods for Get/Set operations

### Default Implementations

**DefaultDbConnectionFactory**: Default implementation of IDbConnectionFactory:
- Stores DbConnectionDescriptor instances in a dictionary
- Provides GetConnection(name) and GetDefaultConnection() methods

**DefaultDbProviderFactory**: Default implementation of IDbProviderFactory:
- Manages IDbProvider instances by database type
- Provides GetProvider(databaseType) and GetDatabaseTypes() methods
- Supports Append(provider) and Clear() for dynamic provider management

### Descriptors

- **DbConnectionDescriptor**: Describes a database connection (Name, DatabaseType, ConnectionString)
- **DbTableDesciptor**: Describes a database table (SchemaName, TableName)
- **DbColumnDesciptor**: Extends DbTableDesciptor with column info (ColumnName, ColumnType, PrimaryKeyFlag, NullableFlag)
- **DbIndexDesciptor**: Describes a database index (TableName, IndexName, UniqueFlag)

### Type System

**DbColumnType**: Enum for database column types:
- `UNKNOW`, `DECIMAL`, `TINY`, `INTEGER`, `LONG`, `TIME`, `VARCHAR`, `TEXT`

## SQLite Implementation

The SQLite provider ([SqliteProvider](Delly.DBunny.Sqlite/SqliteProvider.cs)) implements IDbProvider with:
- Uses `System.Data.SQLite` package
- Parameters prefixed with `@`
- No schema support (SQLite doesn't use schemas)
- Uses `PRAGMA` commands for metadata queries

The SQL provider ([SqliteSqlProvider](Delly.DBunny.Sqlite/SqliteSqlProvider.cs)) implements ISqlProvider:
- Names wrapped in square brackets `[name]`
- Type mappings: INTEGER for integral types, REAL for floating-point, TEXT for strings/dates

The connection define ([SqliteConnectionDefine](Delly.DBunny.Sqlite/SqliteConnectionDefine.cs)) provides SQLite-specific connection parameters:
- Database type constant: `DATABASE_TYPE = "SQLITE"`
- Connection parameter constants (UPPER_CASE with `_KEY` suffix):
  - `DATA_SOURCE_KEY`, `VERSION_KEY`, `PASSWORD_KEY`
  - `PAGE_SIZE_KEY`, `CACHE_SIZE_KEY`, `MODE_KEY`
  - `DEFAULT_TIMEOUT_KEY`, `JOURNAL_MODE_KEY`, `POOLING_KEY`
  - `FOREIGN_KEYS_KEY`, `FAIL_IF_MISSING_KEY`, `READ_ONLY_KEY`
  - `LEGACY_FORMAT_KEY`, `DATE_TIME_FORMAT_KEY`, `DATE_TIME_KIND_KEY`

## Extensions

- **SqledExtension**: Fluent methods for building SQL (`Append()`, `Set()`)
- **DbProviderExtension**: Helper methods for executing queries (`Read()`, `ReadAsync()`)
- **SqliteConnectionDefineExtension**: Fluent builder methods for SQLite connection:
  - `WithDataSource()`, `WithPassword()`, `WithPageSize()`, `WithCacheSize()`
  - `WithDefaultTimeout()`, `WithPooling()`, `WithForeignKeys()`, `WithReadOnly()`

## Adding a New Database Provider

To add support for a new database:

1. Create a new project (e.g., `Delly.DBunny.Postgres`)
2. Create a connection define class extending `BaseConnectionDefine`:
   - Define connection parameter constants with `_KEY` suffix
   - Define database type constant (e.g., `DATABASE_TYPE`)
   - Add properties for connection parameters
   - Optionally add extension methods in `*ConnectionDefineExtension.cs`
3. Implement `IDbProvider` with database-specific connection/command handling
4. Implement `ISqlProvider` with database-specific SQL generation
5. Add any required NuGet packages for the database driver
6. Target the same frameworks: netstandard2.0;net5.0;net8.0

## Conventions

- Use async methods for I/O operations (returns Task)
- Nullable reference types where applicable (.NET 5.0+)
- Database-specific names are quoted via `GetSpecialName()` in ISqlProvider
- Parameters are passed as `IEnumerable<KeyValuePair<string, object>>`
- **Constants** use UPPER_CASE with underscores (e.g., `DATA_SOURCE_KEY`, `DEFAULT_TIMEOUT_KEY`)

## License

MIT License