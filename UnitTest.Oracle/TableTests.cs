using Delly.DBunny;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.Providing.Extension;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Oracle;
using System.Collections.Generic;
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
        _testSchema = $"TESTDB";

        var dataSource = Environment.GetEnvironmentVariable("ORACLE_TEST_DATA_SOURCE") ?? "192.168.56.103:1521/FREE";
        var userId = Environment.GetEnvironmentVariable("ORACLE_TEST_USER_ID") ?? "system";
        var password = Environment.GetEnvironmentVariable("ORACLE_TEST_PASSWORD") ?? "Oracle123";

        // 使用 OracleConnectionDefine 定义连接
        var connectionDefine = new OracleConnectionDefine()
            .WithDataSource(dataSource)
            .WithUserId(userId)
            .WithPassword(password)
            .WithPooling(true)
            .WithMinPoolSize(0)
            .WithMaxPoolSize(100);

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
            var dropSchemaSql = _provider.SqlProvider.DropSchema(_testSchema);
            await ExecuteNonQueryAsync(_connection, dropSchemaSql);
        }
        catch { }

        var options = new Dictionary<string, object>
        {
            { "password", "password123" }
        };
        var createSchemaSql = _provider.SqlProvider.CreateSchema(_testSchema, options);
        await ExecuteNonQueryAsync(_connection, createSchemaSql);

        var grantPrivilegesSql = new Sqled($"GRANT CONNECT, RESOURCE, CREATE VIEW, CREATE SEQUENCE, CREATE TRIGGER, CREATE ANY INDEX, SELECT ANY TABLE, SELECT ANY DICTIONARY TO \"{_testSchema}\"");
        await ExecuteNonQueryAsync(_connection, grantPrivilegesSql);

        var grantQuotaSql = new Sqled($"ALTER USER \"{_testSchema}\" QUOTA UNLIMITED ON USERS");
        await ExecuteNonQueryAsync(_connection, grantQuotaSql);
    }

    public async Task DisposeAsync()
    {
        try
        {
            // 删除测试 Schema
            var dropSchemaSql = _provider.SqlProvider.DropSchema(_testSchema);
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
        await CreateSimpleTableAsync(tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR2(100)", PrimaryKeyFlag = false, NullableFlag = false });

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
        await CreateSimpleTableAsync(tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OrderDate", ColumnType = "TIMESTAMP", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Status", ColumnType = "VARCHAR2(50)", PrimaryKeyFlag = false, NullableFlag = false });

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
        await CreateSimpleTableAsync(tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OldName", ColumnType = "VARCHAR2(100)", PrimaryKeyFlag = false, NullableFlag = false });

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
        await CreateSimpleTableAsync(tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Email", ColumnType = "VARCHAR2(255)", PrimaryKeyFlag = false, NullableFlag = false });

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
        await CreateSimpleTableAsync(tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR2(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Quantity", ColumnType = "NUMBER(10)", PrimaryKeyFlag = false, NullableFlag = true });

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
        await CreateSimpleTableAsync("Table1",
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false });
        await CreateSimpleTableAsync("Table2",
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false });
        await CreateSimpleTableAsync("Table3",
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false });

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
        await CreateSimpleTableAsync("TestParams",
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR2(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Value", ColumnType = "NUMBER(10)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act
        var insertSql = new Sqled($"INSERT INTO \"{_testSchema}\".\"TestParams\" (\"Id\", \"Name\", \"Value\") VALUES (:id, :name, :value)")
            .Set("id", 1)
            .Set("name", "TestRecord")
            .Set("value", 42);
        await ExecuteNonQueryAsync(_connection, insertSql);

        var selectSql = new Sqled($"SELECT \"Value\" FROM \"{_testSchema}\".\"TestParams\" WHERE \"Name\" = :name").Set("name", "TestRecord");
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
        await CreateSimpleTableAsync(tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OriginalValue", ColumnType = "VARCHAR2(100)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act
        var insertSql = new Sqled($"INSERT INTO \"{_testSchema}\".\"{tableName}\" (\"Id\", \"OriginalValue\") VALUES (:id, :value)")
            .Set("id", 1)
            .Set("value", "Test Value");
        await ExecuteNonQueryAsync(_connection, insertSql);

        var copyColumnSql = _provider.SqlProvider.CopyColumn(_testSchema, tableName, "OriginalValue", "CopiedValue", "VARCHAR2(100)");
        var alterTableSql = new Sqled($"ALTER TABLE \"{_testSchema}\".\"{tableName}\" ADD (\"CopiedValue\" VARCHAR2(100))");
        await ExecuteNonQueryAsync(_connection, alterTableSql);
        await ExecuteNonQueryAsync(_connection, copyColumnSql);

        var selectSql = new Sqled($"SELECT \"CopiedValue\" FROM \"{_testSchema}\".\"{tableName}\" WHERE \"Id\" = :id").Set("id", 1);
        var result = await ExecuteScalarAsync<string>(_connection, selectSql);

        // Assert
        Assert.Equal("Test Value", result);
    }

    private async Task CreateSimpleTableAsync(string tableName, params DbColumnDesciptor[] columns)
    {
        var columnDesciptors = columns.ToList();
        var createTableSql = _provider.SqlProvider.CreateTable(_testSchema, tableName, columnDesciptors);
        await ExecuteNonQueryAsync(_connection, createTableSql);
    }

    private async Task ExecuteNonQueryAsync(DbConnection connection, Sqled sql)
    {
        using var command = _provider.GetDbCommand(connection);
        // Oracle doesn't support semicolons in single command execution
        command.CommandText = sql.Sql.TrimEnd(';');
        _provider.SetParameters(command, sql.Parameters);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ExecuteScalarAsync<T>(DbConnection connection, Sqled sql)
    {
        using var command = _provider.GetDbCommand(connection);
        // Oracle doesn't support semicolons in single command execution
        command.CommandText = sql.Sql.TrimEnd(';');
        _provider.SetParameters(command, sql.Parameters);
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
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false }
        };
        var createTableSql = _provider.SqlProvider.CreateTable(_testSchema, tableName, columnDesciptors);
        await ExecuteNonQueryAsync(_connection, createTableSql);

        // Act
        var dropTableSql = _provider.SqlProvider.DropTable(_testSchema, tableName);
        await ExecuteNonQueryAsync(_connection, dropTableSql);
        var tables = await _provider.GetTables(_connection, _testSchema);

        // Assert
        Assert.DoesNotContain(tables, t => t.TableName.ToUpper() == tableName.ToUpper());
    }

    [Fact]
    public async Task DropIndex_WhenIndexExists_ShouldDropIndexSuccessfully()
    {
        // Arrange
        var tableName = "TestDropIndex";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Value", ColumnType = "NUMBER(10)", PrimaryKeyFlag = false, NullableFlag = false }
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
        Assert.DoesNotContain(indexes, i => i.IndexName.ToUpper() == $"{tableName}_Value_IDX".ToUpper());
    }

    [Fact]
    public void Provider_ShouldHaveCorrectDatabaseType()
    {
        // Assert
        Assert.Equal("ORACLE", _provider.DatabaseType);
        Assert.NotNull(_provider.SqlProvider);
    }

    [Fact]
    public void SqlProvider_HasDatabase_ShouldBeFalse()
    {
        // Assert
        Assert.False(_provider.SqlProvider.HasDatabase);
    }

    [Fact]
    public void SqlProvider_HasSchema_ShouldBeTrue()
    {
        // Assert
        Assert.True(_provider.SqlProvider.HasSchema);
    }

    [Fact]
    public void SqlProvider_GetSpecialName_ShouldQuoteWithDoubleQuotes()
    {
        // Arrange
        var testName = "MyTable";

        // Act
        var result = _provider.SqlProvider.GetSpecialName(testName);

        // Assert
        Assert.Equal("\"MyTable\"", result);
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
        Assert.Equal("NUMBER(1)", boolType);
        Assert.Equal("NUMBER(10)", intType);
        Assert.Equal("NUMBER(19)", longType);
        Assert.Equal("BINARY_DOUBLE", doubleType);
        Assert.Equal("NUMBER(18,4)", decimalType);
        Assert.Equal("VARCHAR2(4000)", stringType);
        Assert.Equal("TIMESTAMP", dateTimeType);
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
        Assert.Equal("NUMBER(3)", tinyType);
        Assert.Equal("NUMBER(10)", intType);
        Assert.Equal("NUMBER(19)", longType);
        Assert.Equal("NUMBER(18,4)", decimalType);
        Assert.Equal("VARCHAR2(255)", varcharType);
        Assert.Equal("CLOB", textType);
        Assert.Equal("TIMESTAMP", timeType);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_VarcharWithLength_ShouldIncludeLength()
    {
        // Act
        var varchar50 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR, 50);
        var varchar100 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR, 100);
        var varchar5000 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.VARCHAR, 5000);

        // Assert
        Assert.Equal("VARCHAR2(50)", varchar50);
        Assert.Equal("VARCHAR2(100)", varchar100);
        Assert.Equal("CLOB", varchar5000); // > 4000 becomes CLOB
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_DecimalWithPrecision_ShouldIncludePrecision()
    {
        // Act
        var decimal182 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.DECIMAL, 18, 2);
        var decimal104 = _provider.SqlProvider.GetSpecialTypeName(DbColumnType.DECIMAL, 10, 4);

        // Assert
        Assert.Equal("NUMBER(18,2)", decimal182);
        Assert.Equal("NUMBER(10,4)", decimal104);
    }

    [Fact]
    public void SqlProvider_CreateTableColumnDefine_ShouldReturnCorrectDefinition()
    {
        // Act
        var primaryKeyColumn = _provider.SqlProvider.CreateTableColumnDefine("Id", "NUMBER(10)", true, false);
        var nullableColumn = _provider.SqlProvider.CreateTableColumnDefine("Name", "VARCHAR2(100)", false, false);
        var nullableTrueColumn = _provider.SqlProvider.CreateTableColumnDefine("Age", "NUMBER(10)", false, true);

        // Assert
        Assert.Equal("\"Id\" NUMBER(10) NOT NULL PRIMARY KEY", primaryKeyColumn.Sql);
        Assert.Equal("\"Name\" VARCHAR2(100) NOT NULL", nullableColumn.Sql);
        Assert.Equal("\"Age\" NUMBER(10) NULL", nullableTrueColumn.Sql);
    }

    [Fact]
    public void SqlProvider_CreateSchema_WithOptions_ShouldIncludeOptions()
    {
        // Act
        var options = new Dictionary<string, object>
        {
            { "password", "testpass123" },
            { "tablespace", "USERS" },
            { "temp_tablespace", "TEMP" }
        };
        var result = _provider.SqlProvider.CreateSchema("testschema", options);

        // Assert
        Assert.Contains("CREATE USER \"testschema\" IDENTIFIED BY \"testpass123\"", result.Sql);
        Assert.Contains("DEFAULT TABLESPACE USERS", result.Sql);
        Assert.Contains("TEMPORARY TABLESPACE TEMP", result.Sql);
    }

    [Fact]
    public void SqlProvider_DropSchema_ShouldGenerateCorrectSql()
    {
        // Act
        var result = _provider.SqlProvider.DropSchema("testschema");

        // Assert
        Assert.Equal("DROP USER \"testschema\" CASCADE;", result.Sql);
    }

    [Fact]
    public void SqlProvider_GetDatabases_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.GetDatabases());
    }

    [Fact]
    public void SqlProvider_CreateDatabase_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.CreateDatabase("testdb", null));
    }

    [Fact]
    public void SqlProvider_DropDatabase_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.DropDatabase("testdb"));
    }

    [Fact]
    public async Task ModifyColumn_WhenTableExists_ShouldModifyColumnSuccessfully()
    {
        // Arrange
        var tableName = "TestModifyColumn";
        await CreateSimpleTableAsync(tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "NUMBER(10)", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Score", ColumnType = "NUMBER(10)", PrimaryKeyFlag = false, NullableFlag = false });

        var column = new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Score" };
        var columnTarget = new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Score", ColumnType = "NUMBER(10,2)", NullableFlag = true };

        // Act
        var modifyColumnSql = _provider.SqlProvider.ModifyColumn(column, columnTarget);
        await ExecuteNonQueryAsync(_connection, modifyColumnSql);
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        var scoreColumn = columns.First(c => c.ColumnName.ToUpper() == "SCORE");
        Assert.Contains("NUMBER", scoreColumn.ColumnType);
        Assert.True(scoreColumn.NullableFlag);
    }

    [Fact]
    public void SqlProvider_ModifyColumn_ShouldGenerateCorrectSql()
    {
        // Arrange
        var column = new DbColumnDesciptor { SchemaName = "testschema", TableName = "testtable", ColumnName = "testcol" };
        var columnTarget = new DbColumnDesciptor { ColumnName = "testcol", ColumnType = "VARCHAR2(100)", NullableFlag = true };

        // Act
        var result = _provider.SqlProvider.ModifyColumn(column, columnTarget);

        // Assert
        Assert.Equal("ALTER TABLE \"testschema\".\"testtable\" MODIFY (\"testcol\" VARCHAR2(100) NULL);", result.Sql);
    }
}