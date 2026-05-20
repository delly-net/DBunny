using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Core;
using Delly.DBunny.Core.Providing.Extension;
using Delly.DBunny.Core.Sql.Extension;
using Delly.DBunny.MsAccess;
using Delly.DBunny.Providing;
using Delly.Modeling;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;

namespace UnitTest.MsAccess;

[SupportedOSPlatform("windows")]
public class TableTests : IDisposable
{
    private readonly IDbProvider _provider;
    private readonly string _testDbPath;
    private readonly DbConnectionDescriptor _connectionDescriptor;
    private DbConnection? _connection;

    public TableTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"testdb_{Guid.NewGuid():N}.accdb");

        // 使用 MsAccessConnectionDefine 定义连接
        var connectionDefine = new MsAccessConnectionDefine()
            .WithDataSource(_testDbPath)
            .WithProviderAce();

        // 创建连接描述器
        _connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(MsAccessConnectionDefine.DATABASE_TYPE, "Default");
        // 创建提供程序工厂
        var providerFactory = new DefaultDbProviderFactory(new MsAccessProvider());
        _provider = providerFactory.GetProvider(_connectionDescriptor.DatabaseType)!;
    }

    private DbConnection GetConnection()
    {
        if (_connection == null || _connection.State == ConnectionState.Closed)
        {
            _connection?.Dispose();
            CreateTestDatabaseFile();
            _connection = _provider.GetDbConnection(_connectionDescriptor.ConnectionString);
            _connection.Open();
        }
        return _connection;
    }

    private void CreateTestDatabaseFile()
    {
        // 先删除已存在的文件
        if (File.Exists(_testDbPath))
        {
            try { File.Delete(_testDbPath); }
            catch { }
        }

        // 使用 ADOX 创建 Access 数据库文件
        try
        {
            var catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
            if (catalogType != null)
            {
                dynamic catalog = Activator.CreateInstance(catalogType) ?? throw new InvalidOperationException("Failed to create ADOX.Catalog instance.");
                // 使用完整路径，避免路径中的空格或特殊字符
                string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"{_testDbPath}\";Jet OLEDB:Engine Type=5;";
                catalog.Create(connectionString);
            }
            else
            {
                throw new InvalidOperationException("ADOX.Catalog is not available. Please ensure the Microsoft Access Database Engine is installed.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create test Access database at {_testDbPath}. Please ensure the Microsoft Access Database Engine 2016 Redistributable (ACE 12.0) is installed.", ex);
        }
    }

    [Fact]
    public async Task CreateTable_WithMultipleColumns_ShouldCreateTableSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        var tableName = "TestUsers";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { ColumnName = "Email", ColumnType = "VARCHAR(255)", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { ColumnName = "CreatedAt", ColumnType = "DATETIME", PrimaryKeyFlag = false, NullableFlag = false }
        };

        var tableDesciptor = new DbTableDesciptor { TableName = tableName };
        var createTableSql = _provider.SqlProvider.CreateTable(tableDesciptor, columnDesciptors);

        // Act
        await ExecuteNonQueryAsync(connection, createTableSql);
        var tables = await _provider.GetTablesAsync(connection, string.Empty);

        // Assert
        Assert.Contains(tables, t => t.TableName == tableName);
    }

    [Fact]
    public async Task DropTable_WhenTableExists_ShouldDropTableSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        var tableName = "TestDropTable";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false }
        };
        var tableDesciptor = new DbTableDesciptor { TableName = tableName };
        var createTableSql = _provider.SqlProvider.CreateTable(tableDesciptor, columnDesciptors);
        await ExecuteNonQueryAsync(connection, createTableSql);

        // Act
        var dropTableSql = _provider.SqlProvider.DropTable(new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName });
        await ExecuteNonQueryAsync(connection, dropTableSql);
        var tables = await _provider.GetTablesAsync(connection, string.Empty);

        // Assert
        Assert.DoesNotContain(tables, t => t.TableName == tableName);
    }

    [Fact]
    public async Task CreateColumn_WhenTableExists_ShouldAddColumnSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        var tableName = "TestProducts";
        await CreateSimpleTableAsync(connection, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act
        var columnDesciptor = new DbColumnDesciptor { SchemaName = string.Empty, TableName = tableName, ColumnName = "Price", ColumnType = "DOUBLE", PrimaryKeyFlag = false, NullableFlag = true };
        var addColumnSql = _provider.SqlProvider.CreateColumn(columnDesciptor);
        await ExecuteNonQueryAsync(connection, addColumnSql);
        var columns = await _provider.GetColumnsAsync(connection, new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName });

        // Assert
        Assert.Contains(columns, c => c.ColumnName == "Price");
    }

    [Fact]
    public async Task DropColumn_WhenColumnExists_ShouldDropColumnSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        var tableName = "TestOrders";
        await CreateSimpleTableAsync(connection, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OrderDate", ColumnType = "DATETIME", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Status", ColumnType = "VARCHAR(50)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act
        var dropColumnSql = _provider.SqlProvider.DropColumn(new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName }, "Status");
        await ExecuteNonQueryAsync(connection, dropColumnSql);
        var columns = await _provider.GetColumnsAsync(connection, new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName });

        // Assert
        Assert.DoesNotContain(columns, c => c.ColumnName == "Status");
    }

    [Fact]
    public async Task RenameColumn_ShouldThrowNotSupportedException()
    {
        // Arrange
        var connection = GetConnection();
        var tableName = "TestCustomers";
        await CreateSimpleTableAsync(connection, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "OldName", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            var renameColumnSql = _provider.SqlProvider.RenameColumn(new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName }, "OldName", "NewName");
            await ExecuteNonQueryAsync(connection, renameColumnSql);
        });
    }

    [Fact]
    public async Task CreateIndex_ShouldCreateIndexSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        var tableName = "TestEmployees";
        await CreateSimpleTableAsync(connection, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Email", ColumnType = "VARCHAR(255)", PrimaryKeyFlag = false, NullableFlag = false });

        // Act
        var indexDesciptor = new DbIndexDesciptor { SchemaName = string.Empty, TableName = tableName, IndexName = "Email", UniqueFlag = true, ColumnName = "Email" };
        var createIndexSql = _provider.SqlProvider.CreateIndex(indexDesciptor);
        await ExecuteNonQueryAsync(connection, createIndexSql);
        var indexes = await _provider.GetIndexesAsync(connection, new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName });

        // Assert
        Assert.Contains(indexes, i => i.IndexName.Contains("Email"));
    }

    [Fact]
    public async Task DropIndex_WhenIndexExists_ShouldDropIndexSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        var tableName = "TestDropIndex";
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Value", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = false }
        };
        var tableDesciptor = new DbTableDesciptor { TableName = tableName };
        var createTableSql = _provider.SqlProvider.CreateTable(tableDesciptor, columnDesciptors);
        await ExecuteNonQueryAsync(connection, createTableSql);

        var indexDesciptor = new DbIndexDesciptor { SchemaName = string.Empty, TableName = tableName, IndexName = "Value", UniqueFlag = false, ColumnName = "Value" };
        var createIndexSql = _provider.SqlProvider.CreateIndex(indexDesciptor);
        await ExecuteNonQueryAsync(connection, createIndexSql);

        // Act
        var indexesBefore = await _provider.GetIndexesAsync(connection, new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName });

        var dropIndexSql = _provider.SqlProvider.DropIndex(new DbIndexDesciptor { SchemaName = string.Empty, TableName = tableName, ColumnName = "Value", IndexName = "" });
        await ExecuteNonQueryAsync(connection, dropIndexSql);
        var indexesAfter = await _provider.GetIndexesAsync(connection, new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName });

        // Assert
        var indexCountBefore = indexesBefore.Count;
        var indexCountAfter = indexesAfter.Count;
        Assert.Equal(indexCountBefore - 1, indexCountAfter);

        // Cleanup
        var dropTableSql = _provider.SqlProvider.DropTable(new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName });
        await ExecuteNonQueryAsync(connection, dropTableSql);
    }

    [Fact]
    public async Task GetColumns_ShouldReturnAllColumnsWithMetadata()
    {
        // Arrange
        var connection = GetConnection();
        var tableName = "TestItems";
        await CreateSimpleTableAsync(connection, tableName,
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Quantity", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true });

        // Act
        var columns = await _provider.GetColumnsAsync(connection, new DbTableDesciptor { SchemaName = string.Empty, TableName = tableName });

        // Assert
        Assert.Equal(3, columns.Count);

        var idColumn = columns.First(c => c.ColumnName == "Id");
        Assert.Contains("INTEGER", idColumn.ColumnType, StringComparison.OrdinalIgnoreCase);
        Assert.False(idColumn.NullableFlag);

        var nameColumn = columns.First(c => c.ColumnName == "Name");
        // Access 返回 CHAR 或 WCHAR 作为 VARCHAR 的实际类型
        Assert.True(nameColumn.ColumnType.Contains("CHAR", StringComparison.OrdinalIgnoreCase),
            $"Expected CHAR or WCHAR, got {nameColumn.ColumnType}");
        Assert.False(nameColumn.NullableFlag);

        var quantityColumn = columns.First(c => c.ColumnName == "Quantity");
        Assert.Contains("INTEGER", quantityColumn.ColumnType, StringComparison.OrdinalIgnoreCase);
        Assert.True(quantityColumn.NullableFlag);
    }

    [Fact]
    public async Task GetTables_ShouldReturnAllTables()
    {
        // Arrange
        var connection = GetConnection();
        await CreateSimpleTableAsync(connection, "Table1", new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false });
        await CreateSimpleTableAsync(connection, "Table2", new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false });
        await CreateSimpleTableAsync(connection, "Table3", new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false });

        // Act
        var tables = await _provider.GetTablesAsync(connection, string.Empty);

        // Assert
        Assert.Contains(tables, t => t.TableName == "Table1");
        Assert.Contains(tables, t => t.TableName == "Table2");
        Assert.Contains(tables, t => t.TableName == "Table3");
    }

    [Fact]
    public async Task Sqled_WithParameters_ShouldExecuteCorrectly()
    {
        // Arrange
        var connection = GetConnection();
        await CreateSimpleTableAsync(connection, "TestParams",
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Value", ColumnType = "LONG", PrimaryKeyFlag = false, NullableFlag = false });

        // Act - VALUE 是 Access 保留关键字，需要用方括号括起来
        var insertSql = new Sqled("INSERT INTO [TestParams] (Name, [Value]) VALUES (?, ?)")
            .Set("name", "TestRecord")
            .Set("value", 42);
        await ExecuteNonQueryAsync(connection, insertSql);

        var selectSql = new Sqled("SELECT [Value] FROM [TestParams] WHERE Name = ?").Set("name", "TestRecord");
        var result = await ExecuteScalarAsync<int>(connection, selectSql);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void ConnectionDescriptor_ShouldHaveCorrectProperties()
    {
        // Assert
        Assert.Equal("Default", _connectionDescriptor.Name);
        Assert.Equal("MSACCESS", _connectionDescriptor.DatabaseType);
        Assert.Contains("Provider=", _connectionDescriptor.ConnectionString);
        Assert.Contains("Data Source=", _connectionDescriptor.ConnectionString);
    }

    [Fact]
    public void ConnectionDescriptor_ShouldContainTestDbPath()
    {
        // Assert
        Assert.Contains(_testDbPath, _connectionDescriptor.ConnectionString);
    }

    [Fact]
    public void Provider_ShouldHaveCorrectDatabaseType()
    {
        // Assert
        Assert.Equal("MSACCESS", _provider.DatabaseType);
        Assert.NotNull(_provider.SqlProvider);
    }

    [Fact]
    public void SqlProvider_HasDatabase_ShouldBeFalse()
    {
        // Assert
        Assert.False(_provider.SqlProvider.HasDatabase);
    }

    [Fact]
    public void SqlProvider_HasSchema_ShouldBeFalse()
    {
        // Assert
        Assert.False(_provider.SqlProvider.HasSchema);
    }

    [Fact]
    public void SqlProvider_GetSpecialName_ShouldQuoteWithSquareBrackets()
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
        Assert.Equal("INTEGER", intType);
        Assert.Equal("LONG", longType);
        Assert.Equal("DOUBLE", doubleType);
        Assert.Equal("CURRENCY", decimalType);
        Assert.Equal("LONGTEXT", stringType);
        Assert.Equal("DATETIME", dateTimeType);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_ColumnType_ShouldReturnCorrectTypes()
    {
        // Act
        var tinyType = _provider.SqlProvider.GetSpecialTypeName(ColumnType.BOOL);
        var intType = _provider.SqlProvider.GetSpecialTypeName(ColumnType.INTEGER);
        var longType = _provider.SqlProvider.GetSpecialTypeName(ColumnType.LONG);
        var decimalType = _provider.SqlProvider.GetSpecialTypeName(ColumnType.DECIMAL);
        var varcharType = _provider.SqlProvider.GetSpecialTypeName(ColumnType.VARCHAR);
        var textType = _provider.SqlProvider.GetSpecialTypeName(ColumnType.TEXT);
        var timeType = _provider.SqlProvider.GetSpecialTypeName(ColumnType.TIME);

        // Assert
        Assert.Equal("SMALLINT", tinyType);
        Assert.Equal("INTEGER", intType);
        Assert.Equal("LONG", longType);
        Assert.Equal("CURRENCY", decimalType);
        Assert.Equal("LONGTEXT", varcharType);
        Assert.Equal("LONGTEXT", textType);
        Assert.Equal("DATETIME", timeType);
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_VarcharWithLength_ShouldIncludeLength()
    {
        // Act
        var varchar50 = _provider.SqlProvider.GetSpecialTypeName(ColumnType.VARCHAR, 50);
        var varchar100 = _provider.SqlProvider.GetSpecialTypeName(ColumnType.VARCHAR, 100);
        var varchar300 = _provider.SqlProvider.GetSpecialTypeName(ColumnType.VARCHAR, 300);

        // Assert
        Assert.Equal("VARCHAR(50)", varchar50);
        Assert.Equal("VARCHAR(100)", varchar100);
        // 超过255字符使用LONGTEXT
        Assert.Equal("LONGTEXT", varchar300);
    }

    [Fact]
    public void SqlProvider_CreateTableColumnDefine_ShouldReturnCorrectDefinition()
    {
        // Act
        var primaryKeyColumn = _provider.SqlProvider.CreateTableColumnDefine(
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false });
        var nullableColumn = _provider.SqlProvider.CreateTableColumnDefine(
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false });
        var nullableTrueColumn = _provider.SqlProvider.CreateTableColumnDefine(
            new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true });

        // Assert - Access uses COUNTER instead of AUTOINCREMENT for auto-increment
        Assert.Equal("[Id] COUNTER NOT NULL", primaryKeyColumn.Sql);
        Assert.Equal("[Name] VARCHAR(100) NOT NULL", nullableColumn.Sql);
        Assert.Equal("[Age] INTEGER NULL", nullableTrueColumn.Sql);
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
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.CreateDatabase("testdb", null!));
    }

    [Fact]
    public void SqlProvider_DropDatabase_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.DropDatabase("testdb"));
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
    public void SqlProvider_GetSpecialTypeName_UnsupportedTypeCode_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.GetSpecialTypeName(TypeCode.Object));
    }

    [Fact]
    public void SqlProvider_GetSpecialTypeName_UnsupportedDbType_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.GetSpecialTypeName(ColumnType.UNSET));
    }

    [Fact]
    public void SqlProvider_ModifyColumn_ShouldThrowNotSupportedException()
    {
        // Arrange
        var column = new DbColumnDesciptor { SchemaName = string.Empty, TableName = "TestTable", ColumnName = "TestColumn" };
        var columnTarget = new DbColumnDesciptor { ColumnName = "TestColumn", ColumnType = "LONGTEXT", NullableFlag = true };

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.SqlProvider.ModifyColumn(column, columnTarget));
    }

    private async Task CreateSimpleTableAsync(DbConnection connection, string tableName, params DbColumnDesciptor[] columns)
    {
        var columnDesciptors = columns.ToList();
        var tableDesciptor = new DbTableDesciptor { TableName = tableName };
        var createTableSql = _provider.SqlProvider.CreateTable(tableDesciptor, columnDesciptors);
        await ExecuteNonQueryAsync(connection, createTableSql);
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