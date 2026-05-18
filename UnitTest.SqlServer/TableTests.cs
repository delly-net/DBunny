using Delly.DBunny;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.Providing.Extension;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.SqlServer;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace UnitTest.SqlServer;

[Collection("SqlServerTests")]
public class TableTests : IAsyncLifetime
{
    private readonly IDbProvider _provider;
    private readonly DbConnection _connection;
    private readonly DbConnectionDescriptor _connectionDescriptor;
    private readonly string _testDatabaseName;
    private readonly string _testSchema;

    public TableTests()
    {
        _testDatabaseName = "testdb";
        _testSchema = "test_schema";

        var server = Environment.GetEnvironmentVariable("SQLSERVER_TEST_SERVER") ?? "192.168.56.103";
        var userId = Environment.GetEnvironmentVariable("SQLSERVER_TEST_USER_ID") ?? "sa";
        var password = Environment.GetEnvironmentVariable("SQLSERVER_TEST_PASSWORD") ?? "Admin@123456";

        // 使用 SqlServerConnectionDefine 定义连接（先连接到 master 数据库）
        var connectionDefine = new SqlServerConnectionDefine()
            .WithServer(server)
            .WithDatabase("master")
            .WithUserId(userId)
            .WithPassword(password)
            .WithTrustServerCertificate(true)
            .WithEncrypt(false);

        // 创建连接描述器
        _connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(SqlServerConnectionDefine.DATABASE_TYPE, "Default");
        // 创建连接工厂
        var connectionFactory = new DefaultDbConnectionFactory(_connectionDescriptor);
        // 创建提供程序工厂
        var providerFactory = new DefaultDbProviderFactory(new SqlServerProvider());

        // 通过 Provider 获取连接
        _provider = providerFactory.GetProvider(connectionFactory.GetDefaultConnection().DatabaseType)!;
        _connection = _provider.GetDbConnection(_connectionDescriptor.ConnectionString);
    }

    public async Task InitializeAsync()
    {
        _connection.Open();
        // 创建测试数据库
        var createDatabaseSql = _provider.SqlProvider.CreateDatabase(_testDatabaseName, null!);
        await ExecuteNonQueryAsync(_connection, createDatabaseSql);

        // 切换到测试数据库
        var useDbSql = new Sqled($"USE [{_testDatabaseName}]");
        await ExecuteNonQueryAsync(_connection, useDbSql);

        // 创建测试 Schema
        var createSchemaSql = new Sqled($"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{_testSchema}') EXEC('CREATE SCHEMA [{_testSchema}]')");
        await ExecuteNonQueryAsync(_connection, createSchemaSql);
    }

    public async Task DisposeAsync()
    {
        try
        {
            // 切换到 master 数据库
            var useMasterSql = new Sqled("USE [master]");
            await ExecuteNonQueryAsync(_connection, useMasterSql);

            // 删除测试数据库
            var dropDatabaseSql = _provider.SqlProvider.DropDatabase(_testDatabaseName);
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
        var testSchema = "new_test_schema";
        var dropSchemaSql = _provider.SqlProvider.DropSchema(testSchema);
        await ExecuteNonQueryAsync(_connection, dropSchemaSql);

        // Act
        var createSchemaSql = _provider.SqlProvider.CreateSchema(testSchema, null!);
        await ExecuteNonQueryAsync(_connection, createSchemaSql);
        var schemas = await _provider.GetSchemas(_connection);

        // Assert
        Assert.Contains(schemas, s => s == testSchema);

        // Cleanup
        await ExecuteNonQueryAsync(_connection, dropSchemaSql);
    }

    [Fact]
    public async Task DropSchema_ShouldDropSchemaSuccessfully()
    {
        // Arrange
        var testSchema = "drop_test_schema";
        var createSchemaSql = _provider.SqlProvider.CreateSchema(testSchema, null!);
        await ExecuteNonQueryAsync(_connection, createSchemaSql);

        // Act
        var dropSchemaSql = _provider.SqlProvider.DropSchema(testSchema);
        await ExecuteNonQueryAsync(_connection, dropSchemaSql);
        var schemas = await _provider.GetSchemas(_connection);

        // Assert
        Assert.DoesNotContain(schemas, s => s == testSchema);
    }

    [Fact]
    public async Task CreateTable_WithMultipleColumns_ShouldCreateTableSuccessfully()
    {
        // Arrange
        var tableName = $"TestUsers_{Guid.NewGuid():N}";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Name", ColumnType = "NVARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Age", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Email", ColumnType = "NVARCHAR(255)", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "CreatedAt", ColumnType = "DATETIME", PrimaryKeyFlag = false, NullableFlag = false }
        };

        var createTableSql = _provider.SqlProvider.CreateTable(_testSchema, tableName, columnDesciptors);

        try
        {
            // Act
            await ExecuteNonQueryAsync(_connection, createTableSql);
            var tables = await _provider.GetTables(_connection, _testSchema);

            // Assert
            Assert.Contains(tables, t => t.TableName == tableName && t.SchemaName == _testSchema);
        }
        finally
        {
            // Cleanup
            var dropTableSql = _provider.SqlProvider.DropTable(_testSchema, tableName);
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
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "NVARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false });

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
        await CreateSimpleTableAsync(_testSchema, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OrderDate", ColumnType = "DATETIME", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Status", ColumnType = "NVARCHAR(50)", PrimaryKeyFlag = false, NullableFlag = false });

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
        await CreateSimpleTableAsync(_testSchema, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OldName", ColumnType = "NVARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false });

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
        await CreateSimpleTableAsync(_testSchema, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Email", ColumnType = "NVARCHAR(255)", PrimaryKeyFlag = false, NullableFlag = false });

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

        // 判断表是否存在
        var tables = await _provider.GetTables(_connection, _testSchema);
        if (!tables.Where(d => d.TableName == tableName).Any())
        {
            await CreateSimpleTableAsync(_testSchema, tableName,
                new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
                new DbColumnDesciptor { ColumnName = "Name", ColumnType = "NVARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
                new DbColumnDesciptor { ColumnName = "Quantity", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true });
        }

        // Act
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.Equal(3, columns.Count);

        var idColumn = columns.First(c => c.ColumnName == "Id");
        Assert.Contains("int", idColumn.ColumnType, StringComparison.OrdinalIgnoreCase);
        Assert.True(idColumn.PrimaryKeyFlag);
        Assert.False(idColumn.NullableFlag);

        var nameColumn = columns.First(c => c.ColumnName == "Name");
        Assert.Contains("nvarchar", nameColumn.ColumnType, StringComparison.OrdinalIgnoreCase);
        Assert.False(nameColumn.PrimaryKeyFlag);
        Assert.False(nameColumn.NullableFlag);

        var quantityColumn = columns.First(c => c.ColumnName == "Quantity");
        Assert.Contains("int", quantityColumn.ColumnType, StringComparison.OrdinalIgnoreCase);
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
        // 判断表是否存在
        var tableName = "TestParams";
        var tables = await _provider.GetTables(_connection, _testSchema);
        if (!tables.Where(d => d.TableName == tableName).Any())
        {
            await CreateSimpleTableAsync(_testSchema, "TestParams",
                new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
                new DbColumnDesciptor { ColumnName = "Name", ColumnType = "NVARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
                new DbColumnDesciptor { ColumnName = "Value", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = false });
        }

        // Act
        var insertSql = new Sqled($"INSERT INTO [{_testSchema}].[TestParams] (Name, Value) VALUES (@name, @value)")
            .Set("name", "TestRecord")
            .Set("value", 42);
        await ExecuteNonQueryAsync(_connection, insertSql);

        var selectSql = new Sqled($"SELECT Value FROM [{_testSchema}].[TestParams] WHERE Name = @name").Set("name", "TestRecord");
        var result = await ExecuteScalarAsync<int>(_connection, selectSql);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task GetSchemas_ShouldReturnNonSystemSchemas()
    {
        // Arrange
        var testSchema = "schema_test_get";
        var createSchemaSql = _provider.SqlProvider.CreateSchema(testSchema, null!);
        await ExecuteNonQueryAsync(_connection, createSchemaSql);

        // Act
        var schemas = await _provider.GetSchemas(_connection);

        // Assert
        Assert.Contains(schemas, s => s == testSchema);
        Assert.DoesNotContain(schemas, s => s == "dbo");
        Assert.DoesNotContain(schemas, s => s == "sys");
        Assert.DoesNotContain(schemas, s => s == "INFORMATION_SCHEMA");

        // Cleanup
        var dropSchemaSql = _provider.SqlProvider.DropSchema(testSchema);
        await ExecuteNonQueryAsync(_connection, dropSchemaSql);
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
        var insertSql = new Sqled($"INSERT INTO [{_testSchema}].[{tableName}] (OriginalValue) VALUES (@value)")
            .Set("value", 100);
        await ExecuteNonQueryAsync(_connection, insertSql);

        // Act - 先添加列
        var columnDesciptor = new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "CopiedValue", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true };
        var addColumnSql = _provider.SqlProvider.CreateColumn(columnDesciptor);
        await ExecuteNonQueryAsync(_connection, addColumnSql);

        // 然后复制数据
        var copyColumnSql = _provider.SqlProvider.CopyColumn(_testSchema, tableName, "OriginalValue", "CopiedValue", "INT");
        await ExecuteNonQueryAsync(_connection, copyColumnSql);

        var selectSql = new Sqled($"SELECT OriginalValue, CopiedValue FROM [{_testSchema}].[{tableName}] WHERE Id = @id")
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
        Assert.Equal("SQLSERVER", _connectionDescriptor.DatabaseType);
        Assert.Contains("Data Source=", _connectionDescriptor.ConnectionString);
        Assert.Contains("Database=", _connectionDescriptor.ConnectionString);
    }

    [Fact]
    public void ConnectionDescriptor_ShouldContainMasterDatabaseName()
    {
        // Assert
        Assert.Contains("master", _connectionDescriptor.ConnectionString);
    }

    private async Task CreateSimpleTableAsync(string schema, string tableName, params DbColumnDesciptor[] columns)
    {
        var dropTableSql = _provider.SqlProvider.DropTable(schema, tableName);
        await ExecuteNonQueryAsync(_connection, dropTableSql);
        var columnDesciptors = columns.ToList();
        var createTableSql = _provider.SqlProvider.CreateTable(schema, tableName, columnDesciptors);
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
        Assert.Equal("SQLSERVER", _provider.DatabaseType);
        Assert.NotNull(_provider.SqlProvider);
    }

    [Fact]
    public void SqlProvider_HasDatabase_ShouldBeTrue()
    {
        // Assert
        Assert.True(_provider.SqlProvider.HasDatabase);
    }

    [Fact]
    public void SqlProvider_HasSchema_ShouldBeTrue()
    {
        // Assert
        Assert.True(_provider.SqlProvider.HasSchema);
    }

    [Fact]
    public void SqlProvider_GetSpecialName_ShouldQuoteWithBrackets()
    {
        // Arrange
        var testName = "MyTable";

        // Act
        var result = _provider.SqlProvider.GetSpecialName(testName);

        // Assert
        Assert.Equal("[MyTable]", result);
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
        Assert.Equal("BIT", boolType);
        Assert.Equal("INT", intType);
        Assert.Equal("BIGINT", longType);
        Assert.Equal("FLOAT", doubleType);
        Assert.Equal("DECIMAL(18,4)", decimalType);
        Assert.Equal("NVARCHAR(255)", stringType);
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
        Assert.Equal("NVARCHAR(255)", varcharType);
        Assert.Equal("NVARCHAR(MAX)", textType);
        Assert.Equal("DATETIME", timeType);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_VarcharWithLength_ShouldIncludeLength()
    {
        // Act
        var varchar50 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR, 50);
        var varchar100 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR, 100);

        // Assert
        Assert.Equal("NVARCHAR(50)", varchar50);
        Assert.Equal("NVARCHAR(100)", varchar100);
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
        var primaryKeyColumn = _provider.SqlProvider.CreateTableColumnDefine("Id", "INT", true, false);
        var nullableColumn = _provider.SqlProvider.CreateTableColumnDefine("Name", "NVARCHAR(100)", false, false);
        var nullableTrueColumn = _provider.SqlProvider.CreateTableColumnDefine("Age", "INT", false, true);

        // Assert
        Assert.Equal("[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY", primaryKeyColumn.Sql);
        Assert.Equal("[Name] NVARCHAR(100) NOT NULL", nullableColumn.Sql);
        Assert.Equal("[Age] INT NULL", nullableTrueColumn.Sql);
    }

    [Fact]
    public void SqlProvider_CreateDatabase_WithOptions_ShouldIncludeOptions()
    {
        // Act
        var options = new Dictionary<string, object>
        {
            { "collation", "SQL_Latin1_General_CP1_CI_AS" }
        };
        var result = _provider.SqlProvider.CreateDatabase("testdb", options);

        // Assert
        Assert.Contains("CREATE DATABASE [testdb]", result.Sql);
        Assert.Contains("COLLATE SQL_Latin1_General_CP1_CI_AS", result.Sql);
    }

    [Fact]
    public void SqlProvider_DropDatabase_ShouldGenerateCorrectSql()
    {
        // Act
        var result = _provider.SqlProvider.DropDatabase("testdb");

        // Assert
        Assert.Equal("DROP DATABASE IF EXISTS [testdb];", result.Sql);
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
        var createTableSql = _provider.SqlProvider.CreateTable(_testSchema, tableName, columnDesciptors);
        await ExecuteNonQueryAsync(_connection, createTableSql);

        // Act
        var dropTableSql = _provider.SqlProvider.DropTable(_testSchema, tableName);
        await ExecuteNonQueryAsync(_connection, dropTableSql);
        var tables = await _provider.GetTables(_connection, _testSchema);

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
        var createTableSql = _provider.SqlProvider.CreateTable(_testSchema, tableName, columnDesciptors);
        await ExecuteNonQueryAsync(_connection, createTableSql);

        var indexDesciptor = new DbIndexDesciptor { SchemaName = _testSchema, TableName = tableName, IndexName = "Value", UniqueFlag = false, ColumnName = "Value" };
        var createIndexSql = _provider.SqlProvider.CreateIndex(indexDesciptor);
        await ExecuteNonQueryAsync(_connection, createIndexSql);

        // Act
        var dropIndexSql = _provider.SqlProvider.DropIndex(_testSchema, tableName, "Value");
        await ExecuteNonQueryAsync(_connection, dropIndexSql);
        var indexes = await _provider.GetIndexes(_connection, _testSchema, tableName);

        // Assert
        Assert.DoesNotContain(indexes, i => i.IndexName == $"{tableName}_Value_IDX");
    }

    [Fact]
    public void SqlProvider_GetSchemas_ShouldReturnSchemasSql()
    {
        // Act
        var result = _provider.SqlProvider.GetSchemas();

        // Assert
        Assert.Contains("SELECT name FROM sys.schemas", result.Sql);
        Assert.Contains("NOT IN ('dbo', 'guest', 'sys', 'INFORMATION_SCHEMA')", result.Sql);
    }

    [Fact]
    public void SqlProvider_CreateSchema_ShouldCreateSchemaCorrectly()
    {
        // Act
        var result = _provider.SqlProvider.CreateSchema("testschema", null!);

        // Assert
        Assert.Equal("CREATE SCHEMA [testschema];", result.Sql);
    }

    [Fact]
    public void SqlProvider_CreateSchema_WithAuthorization_ShouldIncludeAuth()
    {
        // Arrange
        var options = new Dictionary<string, object>
        {
            { "authorization", "dbo" }
        };

        // Act
        var result = _provider.SqlProvider.CreateSchema("testschema", options);

        // Assert
        Assert.Equal("CREATE SCHEMA [testschema] AUTHORIZATION [dbo];", result.Sql);
    }

    [Fact]
    public void SqlProvider_DropSchema_ShouldDropSchemaCorrectly()
    {
        // Act
        var result = _provider.SqlProvider.DropSchema("testschema");

        // Assert
        Assert.Equal("DROP SCHEMA IF EXISTS [testschema];", result.Sql);
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
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        var scoreColumn = columns.First(c => c.ColumnName == "Score");
        Assert.Contains("decimal", scoreColumn.ColumnType, StringComparison.OrdinalIgnoreCase);
        Assert.True(scoreColumn.NullableFlag);
    }

    [Fact]
    public void SqlProvider_ModifyColumn_ShouldGenerateCorrectSql()
    {
        // Arrange
        var column = new DbColumnDesciptor { SchemaName = "testschema", TableName = "testtable", ColumnName = "testcol" };
        var columnTarget = new DbColumnDesciptor { ColumnName = "testcol", ColumnType = "NVARCHAR(100)", NullableFlag = true };

        // Act
        var result = _provider.SqlProvider.ModifyColumn(column, columnTarget);

        // Assert
        Assert.Contains("ALTER TABLE [testschema].[testtable]", result.Sql);
        Assert.Contains("ALTER COLUMN [testcol] NVARCHAR(100) NULL;", result.Sql);
    }
}