using Delly.DBunny;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.PostgreSql;
using Delly.DBunny.Sql.Extension;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace UnitTest.PostgreSql;

public class TableTests : IClassFixture<PostgreSqlTestFixture>
{
    private readonly PostgreSqlTestFixture _fixture;

    public TableTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateTable_WithMultipleColumns_ShouldCreateTableSuccessfully()
    {
        // Arrange
        var tableName = "TestUsers";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "SERIAL", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { ColumnName = "Email", ColumnType = "VARCHAR(255)", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { ColumnName = "CreatedAt", ColumnType = "TIMESTAMP", PrimaryKeyFlag = false, NullableFlag = false }
        };

        var createTableSql = _fixture.Provider.SqlProvider.CreateTable("public", tableName, columnDesciptors);

        // Act
        await ExecuteNonQueryAsync(_fixture.Connection, createTableSql);
        var tables = await _fixture.Provider.GetTables(_fixture.Connection, "public");

        // Assert
        Assert.Contains(tables, t => t.TableName == tableName);
    }

    [Fact]
    public async Task CreateColumn_WhenTableExists_ShouldAddColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestProducts";
        await CreateSimpleTableAsync(tableName, "\"Id\" SERIAL NOT NULL PRIMARY KEY, \"Name\" VARCHAR(100) NOT NULL");

        // Act
        var columnDesciptor = new DbColumnDesciptor { SchemaName = "public", TableName = tableName, ColumnName = "Price", ColumnType = "NUMERIC(10,2)", PrimaryKeyFlag = false, NullableFlag = true };
        var addColumnSql = _fixture.Provider.SqlProvider.CreateColumn(columnDesciptor);
        await ExecuteNonQueryAsync(_fixture.Connection, addColumnSql);
        var columns = await _fixture.Provider.GetColumns(_fixture.Connection, "public", tableName);

        // Assert
        Assert.Contains(columns, c => c.ColumnName == "Price");
    }

    [Fact]
    public async Task DropColumn_WhenColumnExists_ShouldDropColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestOrders";
        await CreateSimpleTableAsync(tableName, "\"Id\" SERIAL NOT NULL PRIMARY KEY, \"OrderDate\" TIMESTAMP NOT NULL, \"Status\" VARCHAR(50) NOT NULL");

        // Act
        var dropColumnSql = _fixture.Provider.SqlProvider.DropColumn("public", tableName, "Status");
        await ExecuteNonQueryAsync(_fixture.Connection, dropColumnSql);
        var columns = await _fixture.Provider.GetColumns(_fixture.Connection, "public", tableName);

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName == "Status");
    }

    [Fact]
    public async Task RenameColumn_WhenColumnExists_ShouldRenameColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestCustomers";
        await CreateSimpleTableAsync(tableName, "\"Id\" SERIAL NOT NULL PRIMARY KEY, \"OldName\" VARCHAR(100) NOT NULL");

        // Act
        var renameColumnSql = _fixture.Provider.SqlProvider.RenameColumn("public", tableName, "OldName", "NewName");
        await ExecuteNonQueryAsync(_fixture.Connection, renameColumnSql);
        var columns = await _fixture.Provider.GetColumns(_fixture.Connection, "public", tableName);

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName == "OldName");
        Assert.Contains(columns, c => c.ColumnName == "NewName");
    }

    [Fact]
    public async Task CreateIndex_ShouldCreateIndexSuccessfully()
    {
        // Arrange
        var tableName = "TestEmployees";
        await CreateSimpleTableAsync(tableName, "\"Id\" SERIAL NOT NULL PRIMARY KEY, \"Email\" VARCHAR(255) NOT NULL");

        // Act
        var indexDesciptor = new DbIndexDesciptor { SchemaName = "public", TableName = tableName, IndexName = "Email", UniqueFlag = true, ColumnName = "Email" };
        var createIndexSql = _fixture.Provider.SqlProvider.CreateIndex(indexDesciptor);
        await ExecuteNonQueryAsync(_fixture.Connection, createIndexSql);
        var indexes = await _fixture.Provider.GetIndexes(_fixture.Connection, "public", tableName);

        // Assert
        Assert.Contains(indexes, i => i.IndexName == $"{tableName}_Email_IDX");
    }

    [Fact]
    public async Task GetColumns_ShouldReturnAllColumnsWithMetadata()
    {
        // Arrange
        var tableName = "TestItems";
        await CreateSimpleTableAsync(tableName, "\"Id\" SERIAL NOT NULL PRIMARY KEY, \"Name\" VARCHAR(100) NOT NULL, \"Quantity\" INTEGER NULL");

        // Act
        var columns = await _fixture.Provider.GetColumns(_fixture.Connection, "public", tableName);

        // Assert
        Assert.Equal(3, columns.Count);

        var idColumn = columns.First(c => c.ColumnName == "Id");
        Assert.Contains("SERIAL", idColumn.ColumnType);
        Assert.True(idColumn.PrimaryKeyFlag);
        Assert.False(idColumn.NullableFlag);

        var nameColumn = columns.First(c => c.ColumnName == "Name");
        Assert.Contains("VARCHAR", nameColumn.ColumnType);
        Assert.False(nameColumn.PrimaryKeyFlag);
        Assert.False(nameColumn.NullableFlag);

        var quantityColumn = columns.First(c => c.ColumnName == "Quantity");
        Assert.Contains("INTEGER", quantityColumn.ColumnType);
        Assert.False(quantityColumn.PrimaryKeyFlag);
        Assert.True(quantityColumn.NullableFlag);
    }

    [Fact]
    public async Task GetTables_ShouldReturnAllTables()
    {
        // Arrange
        await CreateSimpleTableAsync("Table1", "\"Id\" SERIAL NOT NULL PRIMARY KEY");
        await CreateSimpleTableAsync("Table2", "\"Id\" SERIAL NOT NULL PRIMARY KEY");
        await CreateSimpleTableAsync("Table3", "\"Id\" SERIAL NOT NULL PRIMARY KEY");

        // Act
        var tables = await _fixture.Provider.GetTables(_fixture.Connection, "public");

        // Assert
        Assert.Contains(tables, t => t.TableName == "Table1");
        Assert.Contains(tables, t => t.TableName == "Table2");
        Assert.Contains(tables, t => t.TableName == "Table3");
    }

    [Fact]
    public async Task GetSchemas_ShouldReturnSchemas()
    {
        // Act
        var schemas = await _fixture.Provider.GetSchemas(_fixture.Connection);

        // Assert
        Assert.Contains("public", schemas);
    }

    [Fact]
    public async Task CreateSchema_ShouldCreateNewSchemaSuccessfully()
    {
        // Arrange
        var schemaName = "test_schema_" + Guid.NewGuid().ToString("N");

        // Act
        var createSchemaSql = _fixture.Provider.SqlProvider.CreateSchema(schemaName);
        await ExecuteNonQueryAsync(_fixture.Connection, createSchemaSql);
        var schemas = await _fixture.Provider.GetSchemas(_fixture.Connection);

        // Assert
        Assert.Contains(schemaName, schemas);
    }

    [Fact]
    public async Task Sqled_WithParameters_ShouldExecuteCorrectly()
    {
        // Arrange
        await CreateSimpleTableAsync("TestParams", "\"Id\" SERIAL NOT NULL PRIMARY KEY, \"Name\" VARCHAR(100) NOT NULL, \"Value\" INTEGER NOT NULL");

        // Act
        var insertSql = new Sqled("INSERT INTO \"TestParams\" (\"Name\", \"Value\") VALUES (@name, @value)")
            .Set("name", "TestRecord")
            .Set("value", 42);
        await ExecuteNonQueryAsync(_fixture.Connection, insertSql);

        var selectSql = new Sqled("SELECT \"Value\" FROM \"TestParams\" WHERE \"Name\" = @name").Set("name", "TestRecord");
        var result = await ExecuteScalarAsync<int>(_fixture.Connection, selectSql);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void ConnectionDescriptor_ShouldHaveCorrectProperties()
    {
        // Assert
        Assert.Equal("Default", _fixture.ConnectionDescriptor.Name);
        Assert.Equal("POSTGRESQL", _fixture.ConnectionDescriptor.DatabaseType);
        Assert.Contains("Host=", _fixture.ConnectionDescriptor.ConnectionString);
        Assert.Contains("Database=", _fixture.ConnectionDescriptor.ConnectionString);
    }

    [Fact]
    public void Provider_ShouldHaveCorrectDatabaseType()
    {
        // Assert
        Assert.Equal("POSTGRESQL", _fixture.Provider.DatabaseType);
        Assert.NotNull(_fixture.Provider.SqlProvider);
        Assert.True(_fixture.Provider.SqlProvider.HasSchema);
    }

    [Fact]
    public void SqlProvider_GetSpecialName_ShouldQuoteWithDoubleQuotes()
    {
        // Arrange
        var testName = "MyTable";

        // Act
        var result = _fixture.Provider.SqlProvider.GetSpecialName(testName);

        // Assert
        Assert.Equal("\"MyTable\"", result);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_ShouldReturnCorrectTypes()
    {
        // Act
        var boolType = _fixture.Provider.SqlProvider.GetSpecialTypeName(TypeCode.Boolean);
        var intType = _fixture.Provider.SqlProvider.GetSpecialTypeName(TypeCode.Int32);
        var longType = _fixture.Provider.SqlProvider.GetSpecialTypeName(TypeCode.Int64);
        var doubleType = _fixture.Provider.SqlProvider.GetSpecialTypeName(TypeCode.Double);
        var decimalType = _fixture.Provider.SqlProvider.GetSpecialTypeName(TypeCode.Decimal);
        var stringType = _fixture.Provider.SqlProvider.GetSpecialTypeName(TypeCode.String);
        var dateTimeType = _fixture.Provider.SqlProvider.GetSpecialTypeName(TypeCode.DateTime);

        // Assert
        Assert.Equal("BOOLEAN", boolType);
        Assert.Equal("INTEGER", intType);
        Assert.Equal("BIGINT", longType);
        Assert.Equal("DOUBLE PRECISION", doubleType);
        Assert.Equal("NUMERIC(18,4)", decimalType);
        Assert.Equal("VARCHAR(255)", stringType);
        Assert.Equal("TIMESTAMP", dateTimeType);
    }

    private async Task CreateSimpleTableAsync(string tableName, string columns)
    {
        var sql = new Sqled($"CREATE TABLE \"public\".\"{tableName}\"({columns});");
        await ExecuteNonQueryAsync(_fixture.Connection, sql);
    }

    private async Task ExecuteNonQueryAsync(DbConnection connection, Sqled sql)
    {
        using var command = _fixture.Provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
        _fixture.Provider.SetParameters(command, sql.Parameters);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ExecuteScalarAsync<T>(DbConnection connection, Sqled sql)
    {
        using var command = _fixture.Provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
        _fixture.Provider.SetParameters(command, sql.Parameters);
        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value ? (T)Convert.ChangeType(result, typeof(T))! : default!;
    }
}