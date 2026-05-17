using Delly.DBunny;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.Providing.Extension;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Sqlite;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace UnitTest.Sqlite;

public class TableTests : IDisposable
{
    private readonly IDbProvider _provider;
    private readonly DbConnection _connection;
    private readonly string _testDbPath;
    private readonly DbConnectionDescriptor _connectionDescriptor;

    public TableTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"testdb_{Guid.NewGuid():N}.db");

        // 使用 SqliteConnectionDefine 定义连接
        var connectionDefine = new SqliteConnectionDefine()
            .WithDataSource(_testDbPath)
            .WithPooling(false)
            .WithForeignKeys(true);

        // 创建连接描述器
        _connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(SqliteConnectionDefine.DATABASE_TYPE, "Default");
        // 创建连接工厂
        var connectionFactory = new DefaultDbConnectionFactory(_connectionDescriptor);
        // 创建提供程序工厂
        var providerFactory = new DefaultDbProviderFactory(new SqliteProvider());

        // 通过 Provider 获取连接
        _provider = providerFactory.GetProvider(connectionFactory.GetDefaultConnection().DatabaseType)!;
        _connection = _provider.GetDbConnection(_connectionDescriptor.ConnectionString);
        _connection.Open();
    }

    [Fact]
    public async Task CreateTable_WithMultipleColumns_ShouldCreateTableSuccessfully()
    {
        // Arrange
        var tableName = "TestUsers";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "TEXT(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { ColumnName = "Email", ColumnType = "TEXT(255)", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { ColumnName = "CreatedAt", ColumnType = "TEXT(32)", PrimaryKeyFlag = false, NullableFlag = false }
        };

        var createTableSql = _provider.SqlProvider.CreateTable(string.Empty, tableName, columnDesciptors);

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
        var columnDesciptor = new DbColumnDesciptor { SchemaName = string.Empty, TableName = tableName, ColumnName = "Price", ColumnType = "REAL", PrimaryKeyFlag = false, NullableFlag = true };
        var addColumnSql = _provider.SqlProvider.CreateColumn(columnDesciptor);
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
        var indexDesciptor = new DbIndexDesciptor { SchemaName = string.Empty, TableName = tableName, IndexName = "Email", UniqueFlag = true, ColumnName = "Email" };
        var createIndexSql = _provider.SqlProvider.CreateIndex(indexDesciptor);
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

    [Fact]
    public void ConnectionDescriptor_ShouldHaveCorrectProperties()
    {
        // Assert
        Assert.Equal("Default", _connectionDescriptor.Name);
        Assert.Equal("SQLITE", _connectionDescriptor.DatabaseType);
        Assert.Contains("Data Source=", _connectionDescriptor.ConnectionString);
    }

    [Fact]
    public void ConnectionDescriptor_ShouldContainTestDbPath()
    {
        // Assert
        Assert.Contains(_testDbPath, _connectionDescriptor.ConnectionString);
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
        return result != null && result != DBNull.Value ? (T)Convert.ChangeType(result, typeof(T))! : default!;
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