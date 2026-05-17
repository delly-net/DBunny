using Delly.DBunny;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.Providing.Extension;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.MySql;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace UnitTest.MySql;

public class TableTests : IAsyncLifetime
{
    private readonly IDbProvider _provider;
    private readonly DbConnection _connection;
    private readonly DbConnectionDescriptor _connectionDescriptor;
    private readonly string _testDatabaseName;
    private readonly string _testSchema;

    public TableTests()
    {
        _testDatabaseName = $"testdb_{Guid.NewGuid():N}";
        _testSchema = "test_schema";

        var server = Environment.GetEnvironmentVariable("MYSQL_TEST_SERVER") ?? "localhost";
        var port = int.Parse(Environment.GetEnvironmentVariable("MYSQL_TEST_PORT") ?? "3306");
        var userId = Environment.GetEnvironmentVariable("MYSQL_TEST_USER_ID") ?? "root";
        var password = Environment.GetEnvironmentVariable("MYSQL_TEST_PASSWORD") ?? "root";

        // 使用 MySqlConnectionDefine 定义连接
        var connectionDefine = new MySqlConnectionDefine()
            .WithServer(server)
            .WithPort(port)
            .WithDatabase(_testDatabaseName)
            .WithUserId(userId)
            .WithPassword(password)
            .WithCharset("utf8mb4")
            .WithSslMode("None");

        // 创建连接描述器
        _connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(MySqlConnectionDefine.DATABASE_TYPE, "Default");
        // 创建连接工厂
        var connectionFactory = new DefaultDbConnectionFactory(_connectionDescriptor);
        // 创建提供程序工厂
        var providerFactory = new DefaultDbProviderFactory(new MySqlProvider());

        // 通过 Provider 获取连接
        _provider = providerFactory.GetProvider(connectionFactory.GetDefaultConnection().DatabaseType)!;
        _connection = _provider.GetDbConnection(_connectionDescriptor.ConnectionString);
    }

    public async Task InitializeAsync()
    {
        _connection.Open();
        // 创建测试数据库
        var createDatabaseSql = new Sqled($"CREATE DATABASE IF NOT EXISTS `{_testDatabaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci");
        await ExecuteNonQueryAsync(_connection, createDatabaseSql);

        // 创建测试 Schema
        var createSchemaSql = new Sqled($"CREATE SCHEMA IF NOT EXISTS `{_testSchema}`");
        await ExecuteNonQueryAsync(_connection, createSchemaSql);

        // 切换到测试数据库
        var useDatabaseSql = new Sqled($"USE `{_testDatabaseName}`");
        await ExecuteNonQueryAsync(_connection, useDatabaseSql);
    }

    public async Task DisposeAsync()
    {
        try
        {
            // 切换到系统数据库
            var useMysqlSql = new Sqled("USE `mysql`");
            await ExecuteNonQueryAsync(_connection, useMysqlSql);

            // 删除测试数据库
            var dropDatabaseSql = new Sqled($"DROP DATABASE IF EXISTS `{_testDatabaseName}`");
            await ExecuteNonQueryAsync(_connection, dropDatabaseSql);
        }
        catch { }
        finally
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }

    [Fact]
    public async Task CreateSchema_ShouldCreateSchemaSuccessfully()
    {
        // Arrange
        var schemaName = "new_test_schema";

        // Act
        var createSchemaSql = _provider.SqlProvider.CreateSchema(schemaName);
        await ExecuteNonQueryAsync(_connection, createSchemaSql);

        var schemas = await _provider.GetSchemas(_connection);

        // Assert
        Assert.Contains(schemas, s => s == schemaName);
    }

    [Fact]
    public async Task CreateTable_WithMultipleColumns_ShouldCreateTableSuccessfully()
    {
        // Arrange
        var tableName = "TestUsers";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Age", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Email", ColumnType = "VARCHAR(255)", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "CreatedAt", ColumnType = "DATETIME", PrimaryKeyFlag = false, NullableFlag = false }
        };

        var createTableSql = _provider.SqlProvider.CreateTable(_testSchema, tableName, columnDesciptors);

        // Act
        await ExecuteNonQueryAsync(_connection, createTableSql);
        var tables = await _provider.GetTables(_connection, _testSchema);

        // Assert
        Assert.Contains(tables, t => t.TableName == tableName && t.SchemaName == _testSchema);
    }

    [Fact]
    public async Task CreateColumn_WhenTableExists_ShouldAddColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestProducts";
        await CreateSimpleTableAsync(_testSchema, tableName, "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Name VARCHAR(100) NOT NULL");

        // Act
        var columnDesciptor = new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Price", ColumnType = "DECIMAL(18,2)", PrimaryKeyFlag = false, NullableFlag = true };
        var addColumnSql = _provider.SqlProvider.CreateColumn(columnDesciptor);
        await ExecuteNonQueryAsync(_connection, addColumnSql);
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.Contains(columns, c => c.ColumnName == "Price");
    }

    [Fact]
    public async Task DropColumn_WhenColumnExists_ShouldDropColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestOrders";
        await CreateSimpleTableAsync(_testSchema, tableName, "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, OrderDate DATETIME NOT NULL, Status VARCHAR(50) NOT NULL");

        // Act
        var dropColumnSql = _provider.SqlProvider.DropColumn(_testSchema, tableName, "Status");
        await ExecuteNonQueryAsync(_connection, dropColumnSql);
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName == "Status");
    }

    [Fact]
    public async Task RenameColumn_WhenColumnExists_ShouldRenameColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestCustomers";
        await CreateSimpleTableAsync(_testSchema, tableName, "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, OldName VARCHAR(100) NOT NULL");

        // Act
        var renameColumnSql = _provider.SqlProvider.RenameColumn(_testSchema, tableName, "OldName", "NewName");
        await ExecuteNonQueryAsync(_connection, renameColumnSql);
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName == "OldName");
        Assert.Contains(columns, c => c.ColumnName == "NewName");
    }

    [Fact]
    public async Task CreateIndex_ShouldCreateIndexSuccessfully()
    {
        // Arrange
        var tableName = "TestEmployees";
        await CreateSimpleTableAsync(_testSchema, tableName, "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Email VARCHAR(255) NOT NULL");

        // Act
        var indexDesciptor = new DbIndexDesciptor { SchemaName = _testSchema, TableName = tableName, IndexName = "Email", UniqueFlag = true, ColumnName = "Email" };
        var createIndexSql = _provider.SqlProvider.CreateIndex(indexDesciptor);
        await ExecuteNonQueryAsync(_connection, createIndexSql);
        var indexes = await _provider.GetIndexes(_connection, _testSchema, tableName);

        // Assert
        Assert.Contains(indexes, i => i.IndexName == $"{tableName}_Email_IDX" && i.UniqueFlag == true);
    }

    [Fact]
    public async Task GetColumns_ShouldReturnAllColumnsWithMetadata()
    {
        // Arrange
        var tableName = "TestItems";
        await CreateSimpleTableAsync(_testSchema, tableName, "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Name VARCHAR(100) NOT NULL, Quantity INT NULL");

        // Act
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.Equal(3, columns.Count);

        var idColumn = columns.First(c => c.ColumnName == "Id");
        Assert.Contains("INT", idColumn.ColumnType);
        Assert.True(idColumn.PrimaryKeyFlag);
        Assert.False(idColumn.NullableFlag);

        var nameColumn = columns.First(c => c.ColumnName == "Name");
        Assert.Contains("VARCHAR", nameColumn.ColumnType);
        Assert.False(nameColumn.PrimaryKeyFlag);
        Assert.False(nameColumn.NullableFlag);

        var quantityColumn = columns.First(c => c.ColumnName == "Quantity");
        Assert.Contains("INT", quantityColumn.ColumnType);
        Assert.False(quantityColumn.PrimaryKeyFlag);
        Assert.True(quantityColumn.NullableFlag);
    }

    [Fact]
    public async Task GetTables_ShouldReturnAllTables()
    {
        // Arrange
        await CreateSimpleTableAsync(_testSchema, "Table1", "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY");
        await CreateSimpleTableAsync(_testSchema, "Table2", "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY");
        await CreateSimpleTableAsync(_testSchema, "Table3", "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY");

        // Act
        var tables = await _provider.GetTables(_connection, _testSchema);

        // Assert
        Assert.Contains(tables, t => t.TableName == "Table1");
        Assert.Contains(tables, t => t.TableName == "Table2");
        Assert.Contains(tables, t => t.TableName == "Table3");
    }

    [Fact]
    public async Task Sqled_WithParameters_ShouldExecuteCorrectly()
    {
        // Arrange
        await CreateSimpleTableAsync(_testSchema, "TestParams", "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Name VARCHAR(100) NOT NULL, Value INT NOT NULL");

        // Act
        var insertSql = new Sqled("INSERT INTO `TestParams` (Name, Value) VALUES (@name, @value)")
            .Set("name", "TestRecord")
            .Set("value", 42);
        await ExecuteNonQueryAsync(_connection, insertSql);

        var selectSql = new Sqled("SELECT Value FROM `TestParams` WHERE Name = @name").Set("name", "TestRecord");
        var result = await ExecuteScalarAsync<int>(_connection, selectSql);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task GetSchemas_ShouldReturnAllSchemas()
    {
        // Act
        var schemas = await _provider.GetSchemas(_connection);

        // Assert
        Assert.Contains(schemas, s => s == "information_schema");
        Assert.Contains(schemas, s => s == _testDatabaseName);
    }

    [Fact]
    public async Task CopyColumn_ShouldCopyColumnData()
    {
        // Arrange
        var tableName = "TestCopy";
        await CreateSimpleTableAsync(_testSchema, tableName, "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, OriginalValue INT NOT NULL");

        // 插入测试数据
        var insertSql = new Sqled("INSERT INTO `TestCopy` (OriginalValue) VALUES (@value)")
            .Set("value", 100);
        await ExecuteNonQueryAsync(_connection, insertSql);

        // Act
        var copyColumnSql = _provider.SqlProvider.CopyColumn(_testSchema, tableName, "OriginalValue", "CopiedValue", "INT");
        await ExecuteNonQueryAsync(_connection, copyColumnSql);

        var selectSql = new Sqled("SELECT OriginalValue, CopiedValue FROM `TestCopy` WHERE Id = @id")
            .Set("id", 1);
        var result = await ReadSingleAsync(_connection, selectSql);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result["OriginalValue"]);
        Assert.Equal(100, result["CopiedValue"]);
    }

    [Fact]
    public void ConnectionDescriptor_ShouldHaveCorrectProperties()
    {
        // Assert
        Assert.Equal("Default", _connectionDescriptor.Name);
        Assert.Equal("MYSQL", _connectionDescriptor.DatabaseType);
        Assert.Contains("Server=", _connectionDescriptor.ConnectionString);
        Assert.Contains("Database=", _connectionDescriptor.ConnectionString);
    }

    [Fact]
    public void ConnectionDescriptor_ShouldContainTestDatabaseName()
    {
        // Assert
        Assert.Contains(_testDatabaseName, _connectionDescriptor.ConnectionString);
    }

    private async Task CreateSimpleTableAsync(string schema, string tableName, string columns)
    {
        var sql = $"CREATE TABLE `{schema}`.`{tableName}`({columns});";
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

    private async Task<Dictionary<string, object?>?> ReadSingleAsync(DbConnection connection, Sqled sql)
    {
        Dictionary<string, object?>? result = null;
        await _provider.ReadAsync(connection, sql, async reader =>
        {
            if (await reader.ReadAsync())
            {
                result = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader[i];
                    result[reader.GetName(i)] = value == DBNull.Value ? null : value;
                }
            }
        });
        return result;
    }
}