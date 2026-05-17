using Delly.DBunny;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.Providing.Extension;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Oracle;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace UnitTest.Oracle;

public class TableTests : IAsyncLifetime
{
    private readonly IDbProvider _provider;
    private readonly DbConnection _connection;
    private readonly DbConnectionDescriptor _connectionDescriptor;
    private readonly string _testSchema;

    public TableTests()
    {
        _testSchema = $"test_{Guid.NewGuid():N}";

        var dataSource = Environment.GetEnvironmentVariable("ORACLE_TEST_DATA_SOURCE") ?? "localhost:1521/xe";
        var userId = Environment.GetEnvironmentVariable("ORACLE_TEST_USER_ID") ?? "system";
        var password = Environment.GetEnvironmentVariable("ORACLE_TEST_PASSWORD") ?? "oracle";

        // 使用 OracleConnectionDefine 定义连接
        var connectionDefine = new OracleConnectionDefine()
            .WithDataSource(dataSource)
            .WithUserId(userId)
            .WithPassword(password)
            .WithPooling(true)
            .WithMinPoolSize(0)
            .WithMaxPoolSize(100)
            .WithCommandTimeout(600);

        // 创建连接描述器
        _connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(OracleConnectionDefine.DATABASE_TYPE, "Default");
        // 创建连接工厂
        var connectionFactory = new DefaultDbConnectionFactory(_connectionDescriptor);
        // 创建提供程序工厂
        var providerFactory = new DefaultDbProviderFactory(new OracleProvider());

        // 通过 Provider 获取连接
        _provider = providerFactory.GetProvider(connectionFactory.GetDefaultConnection().DatabaseType)!;
        _connection = _provider.GetDbConnection(_connectionDescriptor.ConnectionString);
    }

    public async Task InitializeAsync()
    {
        _connection.Open();
        // 创建测试 Schema
        try
        {
            var dropUserSql = new Sqled($"DROP USER \"{_testSchema}\" CASCADE");
            await ExecuteNonQueryAsync(_connection, dropUserSql);
        }
        catch { }

        var createSchemaSql = new Sqled($"CREATE USER \"{_testSchema}\" IDENTIFIED BY \"password123\" DEFAULT TABLESPACE USERS TEMPORARY TABLESPACE TEMP");
        await ExecuteNonQueryAsync(_connection, createSchemaSql);

        var grantPrivilegesSql = new Sqled($"GRANT CONNECT, RESOURCE, CREATE VIEW, CREATE SEQUENCE, CREATE TRIGGER TO \"{_testSchema}\"");
        await ExecuteNonQueryAsync(_connection, grantPrivilegesSql);
    }

    public async Task DisposeAsync()
    {
        try
        {
            // 删除测试 Schema
            var dropSchemaSql = new Sqled($"DROP USER \"{_testSchema}\" CASCADE");
            await ExecuteNonQueryAsync(_connection, dropSchemaSql);
        }
        catch { }
        finally
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }

    [Fact]
    public async Task CreateTable_WithMultipleColumns_ShouldCreateTableSuccessfully()
    {
        // Arrange
        var tableName = "TestUsers";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR2(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Age", ColumnType = "NUMBER(10)", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { ColumnName = "Email", ColumnType = "VARCHAR2(255)", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { ColumnName = "CreatedAt", ColumnType = "TIMESTAMP", PrimaryKeyFlag = false, NullableFlag = false }
        };

        var createTableSql = _provider.SqlProvider.CreateTable(_testSchema, tableName, columnDesciptors);

        // Act
        await ExecuteNonQueryAsync(_connection, createTableSql);
        var tables = await _provider.GetTables(_connection, _testSchema);

        // Assert
        Assert.Contains(tables, t => t.TableName.ToUpper() == tableName.ToUpper());
    }

    [Fact]
    public async Task CreateColumn_WhenTableExists_ShouldAddColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestProducts";
        await CreateSimpleTableAsync(tableName, "Id NUMBER(10) NOT NULL PRIMARY KEY, Name VARCHAR2(100) NOT NULL");

        // Act
        var columnDesciptor = new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Price", ColumnType = "NUMBER(18,2)", PrimaryKeyFlag = false, NullableFlag = true };
        var addColumnSql = _provider.SqlProvider.CreateColumn(columnDesciptor);
        await ExecuteNonQueryAsync(_connection, addColumnSql);
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.Contains(columns, c => c.ColumnName.ToUpper() == "PRICE");
    }

    [Fact]
    public async Task DropColumn_WhenColumnExists_ShouldDropColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestOrders";
        await CreateSimpleTableAsync(tableName, "Id NUMBER(10) NOT NULL PRIMARY KEY, OrderDate TIMESTAMP NOT NULL, Status VARCHAR2(50) NOT NULL");

        // Act
        var dropColumnSql = _provider.SqlProvider.DropColumn(_testSchema, tableName, "Status");
        await ExecuteNonQueryAsync(_connection, dropColumnSql);
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName.ToUpper() == "STATUS");
    }

    [Fact]
    public async Task RenameColumn_WhenColumnExists_ShouldRenameColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestCustomers";
        await CreateSimpleTableAsync(tableName, "Id NUMBER(10) NOT NULL PRIMARY KEY, OldName VARCHAR2(100) NOT NULL");

        // Act
        var renameColumnSql = _provider.SqlProvider.RenameColumn(_testSchema, tableName, "OldName", "NewName");
        await ExecuteNonQueryAsync(_connection, renameColumnSql);
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName.ToUpper() == "OLDNAME");
        Assert.Contains(columns, c => c.ColumnName.ToUpper() == "NEWNAME");
    }

    [Fact]
    public async Task CreateIndex_ShouldCreateIndexSuccessfully()
    {
        // Arrange
        var tableName = "TestEmployees";
        await CreateSimpleTableAsync(tableName, "Id NUMBER(10) NOT NULL PRIMARY KEY, Email VARCHAR2(255) NOT NULL");

        // Act
        var indexDesciptor = new DbIndexDesciptor { SchemaName = _testSchema, TableName = tableName, IndexName = "Email", UniqueFlag = true, ColumnName = "Email" };
        var createIndexSql = _provider.SqlProvider.CreateIndex(indexDesciptor);
        await ExecuteNonQueryAsync(_connection, createIndexSql);
        var indexes = await _provider.GetIndexes(_connection, _testSchema, tableName);

        // Assert
        Assert.Contains(indexes, i => i.IndexName.ToUpper() == $"{tableName}_Email_IDX".ToUpper());
    }

    [Fact]
    public async Task GetColumns_ShouldReturnAllColumnsWithMetadata()
    {
        // Arrange
        var tableName = "TestItems";
        await CreateSimpleTableAsync(tableName, "Id NUMBER(10) NOT NULL PRIMARY KEY, Name VARCHAR2(100) NOT NULL, Quantity NUMBER(10) NULL");

        // Act
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.Equal(3, columns.Count);

        var idColumn = columns.First(c => c.ColumnName.ToUpper() == "ID");
        Assert.Contains("NUMBER", idColumn.ColumnType);
        Assert.True(idColumn.PrimaryKeyFlag);
        Assert.False(idColumn.NullableFlag);

        var nameColumn = columns.First(c => c.ColumnName.ToUpper() == "NAME");
        Assert.Contains("VARCHAR2", nameColumn.ColumnType);
        Assert.False(nameColumn.PrimaryKeyFlag);
        Assert.False(nameColumn.NullableFlag);

        var quantityColumn = columns.First(c => c.ColumnName.ToUpper() == "QUANTITY");
        Assert.Contains("NUMBER", quantityColumn.ColumnType);
        Assert.False(quantityColumn.PrimaryKeyFlag);
        Assert.True(quantityColumn.NullableFlag);
    }

    [Fact]
    public async Task GetTables_ShouldReturnAllTables()
    {
        // Arrange
        await CreateSimpleTableAsync("Table1", "Id NUMBER(10) NOT NULL PRIMARY KEY");
        await CreateSimpleTableAsync("Table2", "Id NUMBER(10) NOT NULL PRIMARY KEY");
        await CreateSimpleTableAsync("Table3", "Id NUMBER(10) NOT NULL PRIMARY KEY");

        // Act
        var tables = await _provider.GetTables(_connection, _testSchema);

        // Assert
        Assert.Contains(tables, t => t.TableName.ToUpper() == "TABLE1");
        Assert.Contains(tables, t => t.TableName.ToUpper() == "TABLE2");
        Assert.Contains(tables, t => t.TableName.ToUpper() == "TABLE3");
    }

    [Fact]
    public async Task Sqled_WithParameters_ShouldExecuteCorrectly()
    {
        // Arrange
        await CreateSimpleTableAsync("TestParams", "Id NUMBER(10) NOT NULL PRIMARY KEY, Name VARCHAR2(100) NOT NULL, Value NUMBER(10) NOT NULL");

        // Act
        var insertSql = new Sqled($"INSERT INTO \"{_testSchema}\".\"TestParams\" (Id, Name, Value) VALUES (:id, :name, :value)")
            .Set("id", 1)
            .Set("name", "TestRecord")
            .Set("value", 42);
        await ExecuteNonQueryAsync(_connection, insertSql);

        var selectSql = new Sqled($"SELECT Value FROM \"{_testSchema}\".\"TestParams\" WHERE Name = :name").Set("name", "TestRecord");
        var result = await ExecuteScalarAsync<int>(_connection, selectSql);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void ConnectionDescriptor_ShouldHaveCorrectProperties()
    {
        // Assert
        Assert.Equal("Default", _connectionDescriptor.Name);
        Assert.Equal("ORACLE", _connectionDescriptor.DatabaseType);
        Assert.Contains("Data Source=", _connectionDescriptor.ConnectionString);
    }

    [Fact]
    public async Task GetSchemas_ShouldReturnUserSchemas()
    {
        // Act
        var schemas = await _provider.GetSchemas(_connection);

        // Assert
        Assert.NotNull(schemas);
        Assert.True(schemas.Count > 0);
    }

    [Fact]
    public async Task CopyColumn_WhenColumnExists_ShouldCopyColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestCopyColumn";
        await CreateSimpleTableAsync(tableName, "Id NUMBER(10) NOT NULL PRIMARY KEY, OriginalValue VARCHAR2(100) NOT NULL");

        // Act
        var insertSql = new Sqled($"INSERT INTO \"{_testSchema}\".\"{tableName}\" (Id, OriginalValue) VALUES (:id, :value)")
            .Set("id", 1)
            .Set("value", "Test Value");
        await ExecuteNonQueryAsync(_connection, insertSql);

        var copyColumnSql = _provider.SqlProvider.CopyColumn(_testSchema, tableName, "OriginalValue", "CopiedValue", "VARCHAR2(100)");
        var alterTableSql = new Sqled($"ALTER TABLE \"{_testSchema}\".\"{tableName}\" ADD (CopiedValue VARCHAR2(100))");
        await ExecuteNonQueryAsync(_connection, alterTableSql);
        await ExecuteNonQueryAsync(_connection, copyColumnSql);

        var selectSql = new Sqled($"SELECT CopiedValue FROM \"{_testSchema}\".\"{tableName}\" WHERE Id = :id").Set("id", 1);
        var result = await ExecuteScalarAsync<string>(_connection, selectSql);

        // Assert
        Assert.Equal("Test Value", result);
    }

    private async Task CreateSimpleTableAsync(string tableName, string columns)
    {
        var sql = new Sqled();
        sql.Builder.AppendLine($"CREATE TABLE \"{_testSchema}\".\"{tableName}\"(");
        sql.Builder.Append($"    {columns}");
        sql.Builder.AppendLine();
        sql.Builder.AppendLine(");");
        await ExecuteNonQueryAsync(_connection, sql);
    }

    private async Task ExecuteNonQueryAsync(DbConnection connection, Sqled sql)
    {
        using var command = _provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
        _provider.SetParameters(command, sql.Parameters);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ExecuteScalarAsync<T>(DbConnection connection, Sqled sql)
    {
        using var command = _provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
        _provider.SetParameters(command, sql.Parameters);
        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value ? (T)Convert.ChangeType(result, typeof(T))! : default!;
    }
}