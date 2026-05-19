using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.MySql;
using System.Data.Common;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Delly.DBunny.Core.Providing.Extension;
using Delly.DBunny.Core.Sql.Extension;
using Delly.DBunny.Core;

namespace UnitTest.MySql;

[Collection("MySqlTests")]
public class TableTests : IAsyncLifetime
{
    private readonly IDbProvider _provider;
    private readonly DbConnection _connection;
    private readonly DbConnectionDescriptor _connectionDescriptor;
    private readonly string _testDatabaseName;
    private readonly string _testSchema;

    public TableTests()
    {
        _testDatabaseName = $"testdb";
        _testSchema = "test_schema";

        var server = Environment.GetEnvironmentVariable("MYSQL_TEST_SERVER") ?? "192.168.56.103";
        var port = int.Parse(Environment.GetEnvironmentVariable("MYSQL_TEST_PORT") ?? "3306");
        var userId = Environment.GetEnvironmentVariable("MYSQL_TEST_USER_ID") ?? "root";
        var password = Environment.GetEnvironmentVariable("MYSQL_TEST_PASSWORD") ?? "123456";

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
        // 创建测试数据库 (MySQL中schema就是database)
        var createDatabaseSql = _provider.SqlProvider.CreateDatabase(_testSchema, null!);
        await ExecuteNonQueryAsync(_connection, createDatabaseSql);

        // 切换到测试数据库
        var useSchemaSql = new Sqled($"USE `{_testSchema}`");
        await ExecuteNonQueryAsync(_connection, useSchemaSql);
    }

    public async Task DisposeAsync()
    {
        try
        {
            // 切换到系统数据库
            var useMysqlSql = new Sqled("USE `mysql`");
            await ExecuteNonQueryAsync(_connection, useMysqlSql);

            // 删除测试数据库
            var dropDatabaseSql = _provider.SqlProvider.DropDatabase(_testSchema);
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
    public async Task CreateSchema_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.CreateSchema("testschema", null!));
    }

    [Fact]
    public async Task CreateTable_WithMultipleColumns_ShouldCreateTableSuccessfully()
    {
        // Arrange
        var tableName = $"TestUsers_{Guid.NewGuid():N}";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Age", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Email", ColumnType = "VARCHAR(255)", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "CreatedAt", ColumnType = "DATETIME", PrimaryKeyFlag = false, NullableFlag = false }
        };

        var createTableSql = _provider.SqlProvider.CreateTable(new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName }, columnDesciptors);

        try
        {
            // Act
            await ExecuteNonQueryAsync(_connection, createTableSql);
            var tables = await _provider.GetTablesAsync(_connection, _testSchema);

            // Assert
            Assert.Contains(tables, t => t.TableName == tableName && t.SchemaName == _testSchema);
        }
        finally
        {
            // Cleanup
            var dropTableSql = _provider.SqlProvider.DropTable(new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName });
            await ExecuteNonQueryAsync(_connection, dropTableSql);
        }
    }

    [Fact]
    public async Task CreateColumn_WhenTableExists_ShouldAddColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestProducts";
        await CreateSimpleTableAsync(_testSchema, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act
        var columnDesciptor = new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Price", ColumnType = "DECIMAL(18,2)", PrimaryKeyFlag = false, NullableFlag = true };
        var addColumnSql = _provider.SqlProvider.CreateColumn(columnDesciptor);
        await ExecuteNonQueryAsync(_connection, addColumnSql);
        var columns = await _provider.GetColumnsAsync(_connection, new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName });

        // Assert
        Assert.Contains(columns, c => c.ColumnName == "Price");
    }

    [Fact]
    public async Task DropColumn_WhenColumnExists_ShouldDropColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestOrders";
        await CreateSimpleTableAsync(_testSchema, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OrderDate", ColumnType = "DATETIME", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Status", ColumnType = "VARCHAR(50)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act
        var dropColumnSql = _provider.SqlProvider.DropColumn(new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName }, "Status");
        await ExecuteNonQueryAsync(_connection, dropColumnSql);
        var columns = await _provider.GetColumnsAsync(_connection, new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName });

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName == "Status");
    }

    [Fact]
    public async Task RenameColumn_WhenColumnExists_ShouldRenameColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestCustomers";
        await CreateSimpleTableAsync(_testSchema, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OldName", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act
        var renameColumnSql = _provider.SqlProvider.RenameColumn(new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName }, "OldName", "NewName");
        await ExecuteNonQueryAsync(_connection, renameColumnSql);
        var columns = await _provider.GetColumnsAsync(_connection, new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName });

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName == "OldName");
        Assert.Contains(columns, c => c.ColumnName == "NewName");
    }

    [Fact]
    public async Task CreateIndex_ShouldCreateIndexSuccessfully()
    {
        // Arrange
        var tableName = "TestEmployees";
        await CreateSimpleTableAsync(_testSchema, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Email", ColumnType = "VARCHAR(255)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act
        var indexDesciptor = new DbIndexDesciptor { SchemaName = _testSchema, TableName = tableName, IndexName = "Email", UniqueFlag = true, ColumnName = "Email" };
        var createIndexSql = _provider.SqlProvider.CreateIndex(indexDesciptor);
        await ExecuteNonQueryAsync(_connection, createIndexSql);
        var indexes = await _provider.GetIndexesAsync(_connection, new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName });

        // Assert
        Assert.Contains(indexes, i => i.IndexName == $"{tableName}_Email_IDX" && i.UniqueFlag == true);
    }

    [Fact]
    public async Task GetColumns_ShouldReturnAllColumnsWithMetadata()
    {
        // Arrange
        var tableName = "TestItems";

        // 判断表是否存在
        var tables = await _provider.GetTablesAsync(_connection, _testSchema);
        if (!tables.Where(d => d.TableName == tableName).Any())
        {
            await CreateSimpleTableAsync(_testSchema, tableName,
                new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
                new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
                new DbColumnDesciptor { ColumnName = "Quantity", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true });
        }

        // Act
        var columns = await _provider.GetColumnsAsync(_connection, new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName });

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
        await CreateSimpleTableAsync(_testSchema, "Table1",
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false });
        await CreateSimpleTableAsync(_testSchema, "Table2",
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false });
        await CreateSimpleTableAsync(_testSchema, "Table3",
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false });

        // Act
        var tables = await _provider.GetTablesAsync(_connection, _testSchema);

        // Assert
        Assert.Contains(tables, t => t.TableName == "Table1");
        Assert.Contains(tables, t => t.TableName == "Table2");
        Assert.Contains(tables, t => t.TableName == "Table3");
    }

    [Fact]
    public async Task Sqled_WithParameters_ShouldExecuteCorrectly()
    {
        // Arrange
        // 判断表是否存在
        var tableName = "TestParams";
        var tables = await _provider.GetTablesAsync(_connection, _testSchema);
        if (!tables.Where(d => d.TableName == tableName).Any())
        {
            await CreateSimpleTableAsync(_testSchema, "TestParams",
                new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
                new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
                new DbColumnDesciptor { ColumnName = "Value", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = false });
        }

        // Act
        var insertSql = new Sqled($"INSERT INTO `TestParams` (Name, Value) VALUES (@name, @value)")
            .Set("name", "TestRecord")
            .Set("value", 42);
        await ExecuteNonQueryAsync(_connection, insertSql);

        var selectSql = new Sqled($"SELECT Value FROM `TestParams` WHERE Name = @name").Set("name", "TestRecord");
        var result = await ExecuteScalarAsync<int>(_connection, selectSql);

        // Assert
        Assert.Equal(42, result);
    }


    [Fact]
    public async Task GetSchemas_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _provider.GetSchemasAsync(_connection));
    }

    [Fact]
    public async Task CopyColumn_ShouldCopyColumnData()
    {
        // Arrange
        var tableName = "TestCopy";
        await CreateSimpleTableAsync(_testSchema, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OriginalValue", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = false });

        // 插入测试数据
        var insertSql = new Sqled($"INSERT INTO `{tableName}` (OriginalValue) VALUES (@value)")
            .Set("value", 100);
        await ExecuteNonQueryAsync(_connection, insertSql);

        // Act - 先添加列
        var columnDesciptor = new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "CopiedValue", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true };
        var addColumnSql = _provider.SqlProvider.CreateColumn(columnDesciptor);
        await ExecuteNonQueryAsync(_connection, addColumnSql);

        // 然后复制数据
        var copyColumnSql = _provider.SqlProvider.CopyColumn(new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName }, "OriginalValue", "CopiedValue", "SIGNED");
        await ExecuteNonQueryAsync(_connection, copyColumnSql);

        var selectSql = new Sqled($"SELECT OriginalValue, CopiedValue FROM `{tableName}` WHERE Id = @id")
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

    private async Task CreateSimpleTableAsync(string schema, string tableName, params DbColumnDesciptor[] columns)
    {
        var dropTableSql = _provider.SqlProvider.DropTable(new DbTableDesciptor { SchemaName = schema, TableName = tableName });
        await ExecuteNonQueryAsync(_connection, dropTableSql);
        var columnDesciptors = columns.ToList();
        var createTableSql = _provider.SqlProvider.CreateTable(new DbTableDesciptor { SchemaName = schema, TableName = tableName }, columnDesciptors);
        await ExecuteNonQueryAsync(_connection, createTableSql);
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

    [Fact]
    public void Provider_ShouldHaveCorrectDatabaseType()
    {
        // Assert
        Assert.Equal("MYSQL", _provider.DatabaseType);
        Assert.NotNull(_provider.SqlProvider);
    }

    [Fact]
    public void SqlProvider_HasDatabase_ShouldBeTrue()
    {
        // Assert
        Assert.True(_provider.SqlProvider.HasDatabase);
    }

    [Fact]
    public void SqlProvider_HasSchema_ShouldBeFalse()
    {
        // Assert
        Assert.False(_provider.SqlProvider.HasSchema);
    }

    [Fact]
    public void SqlProvider_GetSpecialName_ShouldQuoteWithBackticks()
    {
        // Arrange
        var testName = "MyTable";

        // Act
        var result = _provider.SqlProvider.GetSpecialName(testName);

        // Assert
        Assert.Equal("`MyTable`", result);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_TypeCode_ShouldReturnCorrectTypes()
    {
        // Act
        var boolType = _provider.SqlProvider.GetSpecialTypeName(TypeCode.Boolean);
        var intType = _provider.SqlProvider.GetSpecialTypeName(TypeCode.Int32);
        var longType = _provider.SqlProvider.GetSpecialTypeName(TypeCode.Int64);
        var doubleType = _provider.SqlProvider.GetSpecialTypeName(TypeCode.Double);
        var decimalType = _provider.SqlProvider.GetSpecialTypeName(TypeCode.Decimal);
        var stringType = _provider.SqlProvider.GetSpecialTypeName(TypeCode.String);
        var dateTimeType = _provider.SqlProvider.GetSpecialTypeName(TypeCode.DateTime);

        // Assert
        Assert.Equal("TINYINT(1)", boolType);
        Assert.Equal("INT", intType);
        Assert.Equal("BIGINT", longType);
        Assert.Equal("DOUBLE", doubleType);
        Assert.Equal("DECIMAL(18,4)", decimalType);
        Assert.Equal("VARCHAR(255)", stringType);
        Assert.Equal("DATETIME", dateTimeType);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_DbColumnType_ShouldReturnCorrectTypes()
    {
        // Act
        var tinyType = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.TINY);
        var intType = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.INTEGER);
        var longType = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.LONG);
        var decimalType = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.DECIMAL);
        var varcharType = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR);
        var textType = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.TEXT);
        var timeType = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.TIME);

        // Assert
        Assert.Equal("TINYINT", tinyType);
        Assert.Equal("INT", intType);
        Assert.Equal("BIGINT", longType);
        Assert.Equal("DECIMAL(18,4)", decimalType);
        Assert.Equal("VARCHAR(255)", varcharType);
        Assert.Equal("TEXT", textType);
        Assert.Equal("DATETIME", timeType);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_VarcharWithLength_ShouldIncludeLength()
    {
        // Act
        var varchar50 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR, 50);
        var varchar100 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR, 100);

        // Assert
        Assert.Equal("VARCHAR(50)", varchar50);
        Assert.Equal("VARCHAR(100)", varchar100);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_DecimalWithPrecision_ShouldIncludePrecision()
    {
        // Act
        var decimal182 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.DECIMAL, 18, 2);
        var decimal104 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.DECIMAL, 10, 4);

        // Assert
        Assert.Equal("DECIMAL(18,2)", decimal182);
        Assert.Equal("DECIMAL(10,4)", decimal104);
    }

    [Fact]
    public void SqlProvider_CreateTableColumnDefine_ShouldReturnCorrectDefinition()
    {
        // Act
        var primaryKeyColumn = _provider.SqlProvider.CreateTableColumnDefine(new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false });
        var nullableColumn = _provider.SqlProvider.CreateTableColumnDefine(new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false });
        var nullableTrueColumn = _provider.SqlProvider.CreateTableColumnDefine(new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true });

        // Assert
        Assert.Equal("`Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY", primaryKeyColumn.Sql);
        Assert.Equal("`Name` VARCHAR(100) NOT NULL", nullableColumn.Sql);
        Assert.Equal("`Age` INT NULL", nullableTrueColumn.Sql);
    }

    [Fact]
    public void SqlProvider_CreateDatabase_WithOptions_ShouldIncludeOptions()
    {
        // Act
        var options = new Dictionary<string, object>
        {
            { "character_set", "latin1" },
            { "collation", "latin1_swedish_ci" }
        };
        var result = _provider.SqlProvider.CreateDatabase("testdb", options);

        // Assert
        Assert.Contains("CREATE DATABASE IF NOT EXISTS `testdb`", result.Sql);
        Assert.Contains("CHARACTER SET latin1", result.Sql);
        Assert.Contains("COLLATE latin1_swedish_ci", result.Sql);
    }

    [Fact]
    public void SqlProvider_DropDatabase_ShouldGenerateCorrectSql()
    {
        // Act
        var result = _provider.SqlProvider.DropDatabase("testdb");

        // Assert
        Assert.Equal("DROP DATABASE IF EXISTS `testdb`;", result.Sql);
    }

    [Fact]
    public async Task DropTable_WhenTableExists_ShouldDropTableSuccessfully()
    {
        // Arrange
        var tableName = "TestDropTable";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false }
        };
        var createTableSql = _provider.SqlProvider.CreateTable(new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName }, columnDesciptors);
        await ExecuteNonQueryAsync(_connection, createTableSql);

        // Act
        var dropTableSql = _provider.SqlProvider.DropTable(new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName });
        await ExecuteNonQueryAsync(_connection, dropTableSql);
        var tables = await _provider.GetTablesAsync(_connection, _testSchema);

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
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Value", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = false }
        };
        var createTableSql = _provider.SqlProvider.CreateTable(new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName }, columnDesciptors);
        await ExecuteNonQueryAsync(_connection, createTableSql);

        var indexDesciptor = new DbIndexDesciptor { SchemaName = _testSchema, TableName = tableName, IndexName = "Value", UniqueFlag = false, ColumnName = "Value" };
        var createIndexSql = _provider.SqlProvider.CreateIndex(indexDesciptor);
        await ExecuteNonQueryAsync(_connection, createIndexSql);

        // Act
        var dropIndexSql = _provider.SqlProvider.DropIndex(new DbIndexDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Value", IndexName = "" });
        await ExecuteNonQueryAsync(_connection, dropIndexSql);
        var indexes = await _provider.GetIndexesAsync(_connection, new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName });

        // Assert
        Assert.DoesNotContain(indexes, i => i.IndexName == $"{tableName}_Value_IDX");
    }

    [Fact]
    public void SqlProvider_GetSchemas_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.GetSchemas());
    }

    [Fact]
    public void SqlProvider_CreateSchema_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.CreateSchema("testschema", null!));
    }

    [Fact]
    public void SqlProvider_DropSchema_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.DropSchema("testschema"));
    }

    [Fact]
    public async Task ModifyColumn_WhenTableExists_ShouldModifyColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestModifyColumn";
        await CreateSimpleTableAsync(_testSchema, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Score", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = false });

        var column = new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Score" };
        var columnTarget = new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Score", ColumnType = "DECIMAL(10,2)", NullableFlag = true };

        // Act
        var modifyColumnSql = _provider.SqlProvider.ModifyColumn(column, columnTarget);
        await ExecuteNonQueryAsync(_connection, modifyColumnSql);
        var columns = await _provider.GetColumnsAsync(_connection, new DbTableDesciptor { SchemaName = _testSchema, TableName = tableName });

        // Assert
        var scoreColumn = columns.First(c => c.ColumnName == "Score");
        Assert.Contains("DECIMAL", scoreColumn.ColumnType);
        Assert.True(scoreColumn.NullableFlag);
    }

    [Fact]
    public void SqlProvider_ModifyColumn_ShouldGenerateCorrectSql()
    {
        // Arrange
        var column = new DbColumnDesciptor { SchemaName = "testschema", TableName = "testtable", ColumnName = "testcol" };
        var columnTarget = new DbColumnDesciptor { ColumnName = "testcol", ColumnType = "VARCHAR(100)", NullableFlag = true };

        // Act
        var result = _provider.SqlProvider.ModifyColumn(column, columnTarget);

        // Assert
        Assert.Equal("ALTER TABLE `testschema`.`testtable` MODIFY COLUMN `testcol` VARCHAR(100) NULL;", result.Sql);
    }
}
