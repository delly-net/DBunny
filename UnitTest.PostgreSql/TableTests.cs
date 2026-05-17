using Delly.DBunny;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.PostgreSql;
using Delly.DBunny.Sql.Extension;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace UnitTest.PostgreSql;

public class TableTests : IDisposable
{
    private readonly PostgreSqlTestFixture _fixture;

    public TableTests()
    {
        _fixture = new PostgreSqlTestFixture();
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
        Assert.Contains(indexes, i => i.IndexName.ToLower().Contains("email") && i.UniqueFlag);
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
        Assert.Contains("integer", idColumn.ColumnType.ToLower());
        Assert.True(idColumn.PrimaryKeyFlag);
        Assert.False(idColumn.NullableFlag);

        var nameColumn = columns.First(c => c.ColumnName == "Name");
        Assert.Contains("character varying", nameColumn.ColumnType.ToLower());
        Assert.False(nameColumn.PrimaryKeyFlag);
        Assert.False(nameColumn.NullableFlag);

        var quantityColumn = columns.First(c => c.ColumnName == "Quantity");
        Assert.Contains("integer", quantityColumn.ColumnType.ToLower());
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
        var createSchemaSql = _fixture.Provider.SqlProvider.CreateSchema(schemaName, null);
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
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "SERIAL", PrimaryKeyFlag = true, NullableFlag = false }
        };
        var createTableSql = _fixture.Provider.SqlProvider.CreateTable("public", tableName, columnDesciptors);
        await ExecuteNonQueryAsync(_fixture.Connection, createTableSql);
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

    [Fact]
    public async Task DropTable_WhenTableExists_ShouldDropTableSuccessfully()
    {
        // Arrange
        var tableName = "TestDropTable";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "SERIAL", PrimaryKeyFlag = true, NullableFlag = false }
        };
        var createTableSql = _fixture.Provider.SqlProvider.CreateTable("public", tableName, columnDesciptors);
        await ExecuteNonQueryAsync(_fixture.Connection, createTableSql);

        // Act
        var dropTableSql = _fixture.Provider.SqlProvider.DropTable("public", tableName);
        await ExecuteNonQueryAsync(_fixture.Connection, dropTableSql);
        var tables = await _fixture.Provider.GetTables(_fixture.Connection, "public");

        // Assert
        Assert.DoesNotContain(tables, t => t.TableName == tableName);
    }

    [Fact]
    public async Task DropIndex_WhenIndexExists_ShouldDropIndexSuccessfully()
    {
        // Arrange
        var tableName = "TestDropIndex";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "SERIAL", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Value", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = false }
        };
        var createTableSql = _fixture.Provider.SqlProvider.CreateTable("public", tableName, columnDesciptors);
        await ExecuteNonQueryAsync(_fixture.Connection, createTableSql);

        var indexDesciptor = new DbIndexDesciptor { SchemaName = "public", TableName = tableName, IndexName = "Value", UniqueFlag = false, ColumnName = "Value" };
        var createIndexSql = _fixture.Provider.SqlProvider.CreateIndex(indexDesciptor);
        await ExecuteNonQueryAsync(_fixture.Connection, createIndexSql);

        // Act
        var dropIndexSql = _fixture.Provider.SqlProvider.DropIndex("public", tableName, "Value");
        await ExecuteNonQueryAsync(_fixture.Connection, dropIndexSql);
        var indexes = await _fixture.Provider.GetIndexes(_fixture.Connection, "public", tableName);

        // Assert
        Assert.DoesNotContain(indexes, i => i.IndexName.ToLower() == $"{tableName}_Value_IDX".ToLower());
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_DbColumnType_ShouldReturnCorrectTypes()
    {
        // Act
        var tinyType = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.TINY);
        var intType = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.INTEGER);
        var longType = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.LONG);
        var decimalType = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.DECIMAL);
        var varcharType = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR);
        var textType = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.TEXT);
        var timeType = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.TIME);

        // Assert
        Assert.Equal("SMALLINT", tinyType);
        Assert.Equal("INTEGER", intType);
        Assert.Equal("BIGINT", longType);
        Assert.Equal("NUMERIC(18,4)", decimalType);
        Assert.Equal("VARCHAR(255)", varcharType);
        Assert.Equal("TEXT", textType);
        Assert.Equal("TIMESTAMP", timeType);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_VarcharWithLength_ShouldIncludeLength()
    {
        // Act
        var varchar50 = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR, 50);
        var varchar100 = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR, 100);

        // Assert
        Assert.Equal("VARCHAR(50)", varchar50);
        Assert.Equal("VARCHAR(100)", varchar100);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_DecimalWithPrecision_ShouldIncludePrecision()
    {
        // Act
        var decimal182 = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.DECIMAL, 18, 2);
        var decimal104 = _fixture.Provider.SqlProvider.GetSpecialTypeName(DbColumnType.DECIMAL, 10, 4);

        // Assert
        Assert.Equal("NUMERIC(18,2)", decimal182);
        Assert.Equal("NUMERIC(10,4)", decimal104);
    }

    [Fact]
    public void SqlProvider_CreateTableColumnDefine_ShouldReturnCorrectDefinition()
    {
        // Act
        var primaryKeyColumn = _fixture.Provider.SqlProvider.CreateTableColumnDefine("Id", "SERIAL", true, false);
        var nullableColumn = _fixture.Provider.SqlProvider.CreateTableColumnDefine("Name", "VARCHAR(100)", false, false);
        var nullableTrueColumn = _fixture.Provider.SqlProvider.CreateTableColumnDefine("Age", "INTEGER", false, true);

        // Assert
        Assert.Equal("\"Id\" SERIAL NOT NULL PRIMARY KEY", primaryKeyColumn.Sql);
        Assert.Equal("\"Name\" VARCHAR(100) NOT NULL", nullableColumn.Sql);
        Assert.Equal("\"Age\" INTEGER NULL", nullableTrueColumn.Sql);
    }

    [Fact]
    public void SqlProvider_CreateDatabase_WithOptions_ShouldIncludeOptions()
    {
        // Act
        var options = new Dictionary<string, object>
        {
            { "owner", "testuser" },
            { "encoding", "UTF8" },
            { "template", "template0" }
        };
        var result = _fixture.Provider.SqlProvider.CreateDatabase("testdb", options);

        // Assert
        Assert.Contains("CREATE DATABASE \"testdb\"", result.Sql);
        Assert.Contains("OWNER \"testuser\"", result.Sql);
        Assert.Contains("ENCODING 'UTF8'", result.Sql);
        Assert.Contains("TEMPLATE \"template0\"", result.Sql);
    }

    [Fact]
    public void SqlProvider_DropDatabase_ShouldGenerateCorrectSql()
    {
        // Act
        var result = _fixture.Provider.SqlProvider.DropDatabase("testdb");

        // Assert
        Assert.Equal("DROP DATABASE IF EXISTS \"testdb\";", result.Sql);
    }

    [Fact]
    public void SqlProvider_CreateSchema_WithOptions_ShouldIncludeOptions()
    {
        // Act
        var options = new Dictionary<string, object>
        {
            { "authorization", "testuser" }
        };
        var result = _fixture.Provider.SqlProvider.CreateSchema("testschema", options);

        // Assert
        Assert.Contains("CREATE SCHEMA \"testschema\"", result.Sql);
        Assert.Contains("AUTHORIZATION \"testuser\"", result.Sql);
    }

    [Fact]
    public void SqlProvider_DropSchema_ShouldGenerateCorrectSql()
    {
        // Act
        var result = _fixture.Provider.SqlProvider.DropSchema("testschema");

        // Assert
        Assert.Equal("DROP SCHEMA IF EXISTS \"testschema\" CASCADE;", result.Sql);
    }

    [Fact]
    public async Task DropSchema_WhenSchemaExists_ShouldDropSchemaSuccessfully()
    {
        // Arrange
        var schemaName = $"test_drop_schema_{Guid.NewGuid():N}";

        try
        {
            // Create schema first
            var createSchemaSql = _fixture.Provider.SqlProvider.CreateSchema(schemaName, null);
            await ExecuteNonQueryAsync(_fixture.Connection, createSchemaSql);

            // Act
            var dropSchemaSql = _fixture.Provider.SqlProvider.DropSchema(schemaName);
            await ExecuteNonQueryAsync(_fixture.Connection, dropSchemaSql);

            // Verify schema is dropped by trying to create it again
            var createAgainSql = _fixture.Provider.SqlProvider.CreateSchema(schemaName, null);
            await ExecuteNonQueryAsync(_fixture.Connection, createAgainSql);

            // Cleanup
            await ExecuteNonQueryAsync(_fixture.Connection, dropSchemaSql);

            // Assert - If we reached here, the schema was successfully dropped and recreated
            Assert.True(true);
        }
        catch
        {
            Assert.True(false, "Schema operations failed");
        }
    }

    public void Dispose()
    {
        _fixture?.Dispose();
    }
}