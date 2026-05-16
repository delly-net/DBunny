using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using Delly.DBunny;
using Delly.DBunny.Providing.Extension;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Sqlite;
using Xunit;

namespace UnitTest.Sqlite;

public class TableTests : IDisposable
{
    private readonly SqliteProvider _provider;
    private readonly DbConnection _connection;
    private readonly string _testDbPath;

    public TableTests()
    {
        _provider = new SqliteProvider();
        _testDbPath = Path.Combine(Path.GetTempPath(), $"testdb_{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={_testDbPath}";
        _connection = _provider.GetDbConnection(connectionString);
        _connection.Open();
    }

    [Fact]
    public async Task CreateTable_WithMultipleColumns_ShouldCreateTableSuccessfully()
    {
        // Arrange
        var tableName = "TestUsers";
        var columnDefines = new List<Sqled>
        {
            _provider.SqlProvider.ColumnDefine("Id", "INTEGER", true, false),
            _provider.SqlProvider.ColumnDefine("Name", "TEXT(100)", false, false),
            _provider.SqlProvider.ColumnDefine("Age", "INTEGER", false, true),
            _provider.SqlProvider.ColumnDefine("Email", "TEXT(255)", false, true),
            _provider.SqlProvider.ColumnDefine("CreatedAt", "TEXT(32)", false, false)
        };

        var createTableSql = _provider.SqlProvider.CreateTable(string.Empty, tableName, columnDefines);

        // Act
        await ExecuteNonQueryAsync(_connection, createTableSql);
        var tables = await _provider.GetTables(_connection, string.Empty);

        // Assert
        Assert.Contains(tables, t => t.TableName == tableName);
    }

    [Fact]
    public async Task CreateColumn_WhenTableExists_ShouldAddColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestProducts";
        await CreateSimpleTableAsync(tableName, "Id INTEGER NOT NULL PRIMARY KEY, Name TEXT(100) NOT NULL");

        // Act
        var addColumnSql = _provider.SqlProvider.CreateColumn(string.Empty, tableName, "Price", "REAL", false, true);
        await ExecuteNonQueryAsync(_connection, addColumnSql);
        var columns = await _provider.GetColumns(_connection, string.Empty, tableName);

        // Assert
        Assert.Contains(columns, c => c.ColumnName == "Price");
    }

    [Fact]
    public async Task DropColumn_WhenColumnExists_ShouldDropColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestOrders";
        await CreateSimpleTableAsync(tableName, "Id INTEGER NOT NULL PRIMARY KEY, OrderDate TEXT(32) NOT NULL, Status TEXT(50) NOT NULL");

        // Act
        var dropColumnSql = _provider.SqlProvider.DropColumn(string.Empty, tableName, "Status");
        await ExecuteNonQueryAsync(_connection, dropColumnSql);
        var columns = await _provider.GetColumns(_connection, string.Empty, tableName);

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName == "Status");
    }

    [Fact]
    public async Task RenameColumn_WhenColumnExists_ShouldRenameColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestCustomers";
        await CreateSimpleTableAsync(tableName, "Id INTEGER NOT NULL PRIMARY KEY, OldName TEXT(100) NOT NULL");

        // Act
        var renameColumnSql = _provider.SqlProvider.RenameColumn(string.Empty, tableName, "OldName", "NewName");
        await ExecuteNonQueryAsync(_connection, renameColumnSql);
        var columns = await _provider.GetColumns(_connection, string.Empty, tableName);

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName == "OldName");
        Assert.Contains(columns, c => c.ColumnName == "NewName");
    }

    [Fact]
    public async Task CreateIndex_ShouldCreateIndexSuccessfully()
    {
        // Arrange
        var tableName = "TestEmployees";
        await CreateSimpleTableAsync(tableName, "Id INTEGER NOT NULL PRIMARY KEY, Email TEXT(255) NOT NULL");

        // Act
        var createIndexSql = _provider.SqlProvider.CreateIndex(string.Empty, tableName, "Email", true);
        await ExecuteNonQueryAsync(_connection, createIndexSql);
        var indexes = await _provider.GetIndexes(_connection, string.Empty, tableName);

        // Assert
        Assert.Contains(indexes, i => i.IndexName == $"{tableName}_Email_IDX");
    }

    [Fact]
    public async Task GetColumns_ShouldReturnAllColumnsWithMetadata()
    {
        // Arrange
        var tableName = "TestItems";
        await CreateSimpleTableAsync(tableName, "Id INTEGER NOT NULL PRIMARY KEY, Name TEXT(100) NOT NULL, Quantity INTEGER NULL");

        // Act
        var columns = await _provider.GetColumns(_connection, string.Empty, tableName);

        // Assert
        Assert.Equal(3, columns.Count);

        var idColumn = columns.First(c => c.ColumnName == "Id");
        Assert.Equal("INTEGER", idColumn.ColumnType);
        Assert.True(idColumn.PrimaryKeyFlag);
        Assert.False(idColumn.NullableFlag);

        var nameColumn = columns.First(c => c.ColumnName == "Name");
        Assert.Equal("TEXT(100)", nameColumn.ColumnType);
        Assert.False(nameColumn.PrimaryKeyFlag);
        Assert.False(nameColumn.NullableFlag);

        var quantityColumn = columns.First(c => c.ColumnName == "Quantity");
        Assert.Equal("INTEGER", quantityColumn.ColumnType);
        Assert.False(quantityColumn.PrimaryKeyFlag);
        Assert.True(quantityColumn.NullableFlag);
    }

    [Fact]
    public async Task GetTables_ShouldReturnAllTables()
    {
        // Arrange
        await CreateSimpleTableAsync("Table1", "Id INTEGER NOT NULL PRIMARY KEY");
        await CreateSimpleTableAsync("Table2", "Id INTEGER NOT NULL PRIMARY KEY");
        await CreateSimpleTableAsync("Table3", "Id INTEGER NOT NULL PRIMARY KEY");

        // Act
        var tables = await _provider.GetTables(_connection, string.Empty);

        // Assert
        Assert.Contains(tables, t => t.TableName == "Table1");
        Assert.Contains(tables, t => t.TableName == "Table2");
        Assert.Contains(tables, t => t.TableName == "Table3");
    }

    [Fact]
    public async Task Sqled_WithParameters_ShouldExecuteCorrectly()
    {
        // Arrange
        await CreateSimpleTableAsync("TestParams", "Id INTEGER NOT NULL PRIMARY KEY, Name TEXT(100) NOT NULL, Value INTEGER NOT NULL");

        // Act
        var insertSql = new Sqled("INSERT INTO [TestParams] (Name, Value) VALUES (@name, @value)")
            .Set("name", "TestRecord")
            .Set("value", 42);
        await ExecuteNonQueryAsync(_connection, insertSql);

        var selectSql = new Sqled("SELECT Value FROM [TestParams] WHERE Name = @name").Set("name", "TestRecord");
        var result = await ExecuteScalarAsync<int>(_connection, selectSql);

        // Assert
        Assert.Equal(42, result);
    }

    private async Task CreateSimpleTableAsync(string tableName, string columns)
    {
        var sql = $"CREATE TABLE [{tableName}]({columns});";
        await ExecuteNonQueryAsync(_connection, sql);
    }

    private async Task ExecuteNonQueryAsync(DbConnection connection, Sqled sql)
    {
        using var command = _provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
        _provider.SetParameters(command, sql.Parameters);
        await command.ExecuteNonQueryAsync();
    }

    private async Task ExecuteNonQueryAsync(DbConnection connection, string sql)
    {
        await ExecuteNonQueryAsync(connection, new Sqled(sql));
    }

    private async Task<T> ExecuteScalarAsync<T>(DbConnection connection, Sqled sql)
    {
        using var command = _provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
        _provider.SetParameters(command, sql.Parameters);
        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value ? (T)result : default!;
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();

        if (File.Exists(_testDbPath))
        {
            try { File.Delete(_testDbPath); }
            catch { }
        }
    }
}