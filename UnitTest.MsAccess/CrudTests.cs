using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Versioning;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.MsAccess;
using Xunit;
using Delly.DBunny.Core.Providing.Extension;
using Delly.DBunny.Core.Sql.Extension;
using Delly.DBunny.Core;

namespace UnitTest.MsAccess;

[SupportedOSPlatform("windows")]
public class CrudTests : IDisposable
{
    private readonly IDbProvider _provider;
    private readonly string _testDbPath;
    private readonly DbConnectionDescriptor _connectionDescriptor;
    private DbConnection? _connection;

    public CrudTests()
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

            // Debug: print the connection string
            Console.WriteLine($"Connection String: {_connectionDescriptor.ConnectionString}");
            Console.WriteLine($"Database file exists: {File.Exists(_testDbPath)}");

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
    public async Task Insert_ShouldInsertRecordSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        var insertSql = new Sqled("INSERT INTO [Users] (Name, Email, Age, CreatedAt) VALUES (?, ?, ?, ?)")
            .Set("name", "John Doe")
            .Set("email", "john@example.com")
            .Set("age", 30)
            .Set("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        // Act
        await ExecuteNonQueryAsync(connection, insertSql);
        var count = await GetRecordCountAsync(connection, "Users");

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Insert_MultipleRecords_ShouldInsertAllSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);

        // Act
        await InsertUserAsync(connection, "Alice", "alice@example.com", 25);
        await InsertUserAsync(connection, "Bob", "bob@example.com", 35);
        await InsertUserAsync(connection, "Charlie", "charlie@example.com", 28);
        var count = await GetRecordCountAsync(connection, "Users");

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task Select_ShouldRetrieveRecordSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        await InsertUserAsync(connection, "Jane Smith", "jane@example.com", 27);

        // Act
        var selectSql = new Sqled("SELECT Name, Email, Age FROM [Users] WHERE Name = ?")
            .Set("name", "Jane Smith");
        var result = await ReadSingleAsync<UserRecord>(connection, selectSql);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane Smith", result.Name);
        Assert.Equal("jane@example.com", result.Email);
        Assert.Equal(27, result.Age);
    }

    [Fact]
    public async Task SelectAll_ShouldRetrieveAllRecordsSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        await InsertUserAsync(connection, "User1", "user1@example.com", 20);
        await InsertUserAsync(connection, "User2", "user2@example.com", 30);
        await InsertUserAsync(connection, "User3", "user3@example.com", 40);

        // Act
        var selectSql = new Sqled("SELECT Name, Email, Age FROM [Users] ORDER BY Age");
        var results = await ReadMultipleAsync<UserRecord>(connection, selectSql);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("User1", results[0].Name);
        Assert.Equal(20, results[0].Age);
        Assert.Equal("User2", results[1].Name);
        Assert.Equal(30, results[1].Age);
        Assert.Equal("User3", results[2].Name);
        Assert.Equal(40, results[2].Age);
    }

    [Fact]
    public async Task Update_ShouldUpdateRecordSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        await InsertUserAsync(connection, "UpdateMe", "old@example.com", 25);

        // Act
        var updateSql = new Sqled("UPDATE [Users] SET Email = ?, Age = ? WHERE Name = ?")
            .Set("newEmail", "new@example.com")
            .Set("newAge", 30)
            .Set("name", "UpdateMe");
        await ExecuteNonQueryAsync(connection, updateSql);

        var selectSql = new Sqled("SELECT Name, Email, Age FROM [Users] WHERE Name = ?").Set("name", "UpdateMe");
        var result = await ReadSingleAsync<UserRecord>(connection, selectSql);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new@example.com", result.Email);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public async Task Update_MultipleColumns_ShouldUpdateAllSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        await InsertUserAsync(connection, "MultiUpdate", "multi@example.com", 20, "2024-01-01 00:00:00");

        // Act
        var updateSql = new Sqled("UPDATE [Users] SET Email = ?, Age = ?, CreatedAt = ? WHERE Name = ?")
            .Set("email", "updated@example.com")
            .Set("age", 35)
            .Set("createdAt", "2024-12-31 23:59:59")
            .Set("name", "MultiUpdate");
        await ExecuteNonQueryAsync(connection, updateSql);

        var selectSql = new Sqled("SELECT Name, Email, Age FROM [Users] WHERE Name = ?").Set("name", "MultiUpdate");
        var result = await ReadSingleAsync<UserRecord>(connection, selectSql);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updated@example.com", result.Email);
        Assert.Equal(35, result.Age);
    }

    [Fact]
    public async Task Update_WithNoMatchingRecord_ShouldNotUpdateAnything()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        await InsertUserAsync(connection, "ExistingUser", "existing@example.com", 30);

        // Act
        var updateSql = new Sqled("UPDATE [Users] SET Email = ? WHERE Name = ?")
            .Set("email", "new@example.com")
            .Set("name", "NonExistingUser");
        var affectedRows = await ExecuteNonQueryWithResultAsync(connection, updateSql);

        var selectSql = new Sqled("SELECT Name, Email, Age FROM [Users] WHERE Name = ?").Set("name", "ExistingUser");
        var result = await ReadSingleAsync<UserRecord>(connection, selectSql);

        // Assert
        Assert.Equal(0, affectedRows);
        Assert.NotNull(result);
        Assert.Equal("existing@example.com", result.Email);
    }

    [Fact]
    public async Task Delete_ShouldDeleteRecordSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        await InsertUserAsync(connection, "ToDelete", "delete@example.com", 25);
        var countBefore = await GetRecordCountAsync(connection, "Users");

        // Act
        var deleteSql = new Sqled("DELETE FROM [Users] WHERE Name = ?").Set("name", "ToDelete");
        await ExecuteNonQueryAsync(connection, deleteSql);
        var countAfter = await GetRecordCountAsync(connection, "Users");

        // Assert
        Assert.Equal(1, countBefore);
        Assert.Equal(0, countAfter);
    }

    [Fact]
    public async Task Delete_WithCondition_ShouldDeleteMatchingRecordsOnly()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        await InsertUserAsync(connection, "UserA", "a@example.com", 20);
        await InsertUserAsync(connection, "UserB", "b@example.com", 30);
        await InsertUserAsync(connection, "UserC", "c@example.com", 40);

        // Act
        var deleteSql = new Sqled("DELETE FROM [Users] WHERE Age >= ?").Set("minAge", 30);
        await ExecuteNonQueryAsync(connection, deleteSql);
        var remainingRecords = await ReadMultipleAsync<UserRecord>(connection,
            new Sqled("SELECT Name, Email, Age FROM [Users] ORDER BY Name"));

        // Assert
        Assert.Single(remainingRecords);
        Assert.Equal("UserA", remainingRecords[0].Name);
        Assert.Equal(20, remainingRecords[0].Age);
    }

    [Fact]
    public async Task Delete_WithNoMatchingRecord_ShouldNotDeleteAnything()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        await InsertUserAsync(connection, "KeepMe", "keep@example.com", 25);
        var countBefore = await GetRecordCountAsync(connection, "Users");

        // Act
        var deleteSql = new Sqled("DELETE FROM [Users] WHERE Name = ?").Set("name", "NonExisting");
        var affectedRows = await ExecuteNonQueryWithResultAsync(connection, deleteSql);
        var countAfter = await GetRecordCountAsync(connection, "Users");

        // Assert
        Assert.Equal(0, affectedRows);
        Assert.Equal(1, countBefore);
        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public async Task FullCrudWorkflow_ShouldCompleteSuccessfully()
    {
        // Arrange & Act - Create
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        var initialCount = await GetRecordCountAsync(connection, "Users");
        Assert.Equal(0, initialCount);

        // Insert
        await InsertUserAsync(connection, "WorkflowUser", "workflow@example.com", 28);
        var countAfterInsert = await GetRecordCountAsync(connection, "Users");
        Assert.Equal(1, countAfterInsert);

        // Read
        var readResult = await ReadSingleAsync<UserRecord>(connection,
            new Sqled("SELECT Name, Email, Age FROM [Users] WHERE Name = ?").Set("name", "WorkflowUser"));
        Assert.NotNull(readResult);
        Assert.Equal("workflow@example.com", readResult.Email);
        Assert.Equal(28, readResult.Age);

        // Update
        var updateSql = new Sqled("UPDATE [Users] SET Age = ? WHERE Name = ?")
            .Set("newAge", 35)
            .Set("name", "WorkflowUser");
        await ExecuteNonQueryAsync(connection, updateSql);
        var updatedResult = await ReadSingleAsync<UserRecord>(connection,
            new Sqled("SELECT Age FROM [Users] WHERE Name = ?").Set("name", "WorkflowUser"));
        Assert.NotNull(updatedResult);
        Assert.Equal(35, updatedResult.Age);

        // Delete
        var deleteSql = new Sqled("DELETE FROM [Users] WHERE Name = ?").Set("name", "WorkflowUser");
        await ExecuteNonQueryAsync(connection, deleteSql);
        var finalCount = await GetRecordCountAsync(connection, "Users");
        Assert.Equal(0, finalCount);
    }

    [Fact]
    public async Task InsertWithSpecialCharacters_ShouldHandleSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        var specialName = "O'Reilly \"The\" Boss";
        var specialEmail = "test+special@example.com";

        // Act
        await InsertUserAsync(connection, specialName, specialEmail, 45);
        var result = await ReadSingleAsync<UserRecord>(connection,
            new Sqled("SELECT Name, Email, Age FROM [Users] WHERE Name = ?").Set("name", specialName));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(specialName, result.Name);
        Assert.Equal(specialEmail, result.Email);
    }

    [Fact]
    public async Task InsertWithNullNullableColumn_ShouldHandleSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);

        // Act
        var insertSql = new Sqled("INSERT INTO [Users] (Name, Email, Age, CreatedAt) VALUES (?, ?, ?, ?)")
            .Set("name", "NullAgeUser")
            .Set("email", "nullage@example.com")
            .Set("age", DBNull.Value)
            .Set("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        await ExecuteNonQueryAsync(connection, insertSql);

        var selectSql = new Sqled("SELECT Age FROM [Users] WHERE Name = ?").Set("name", "NullAgeUser");
        var result = await ExecuteScalarAsync<object?>(connection, selectSql);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InsertWithDateTime_ShouldStoreAndRetrieveCorrectly()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        var testDateTime = new DateTime(2024, 6, 15, 14, 30, 45);
        var formattedDate = testDateTime.ToString("yyyy-MM-dd HH:mm:ss");

        // Act
        await InsertUserAsync(connection, "DateTimeUser", "datetime@example.com", 30, formattedDate);
        var selectSql = new Sqled("SELECT CreatedAt FROM [Users] WHERE Name = ?").Set("name", "DateTimeUser");
        var result = await ExecuteScalarAsync<DateTime>(connection, selectSql);

        // Assert
        Assert.Equal(2024, result.Year);
        Assert.Equal(6, result.Month);
        Assert.Equal(15, result.Day);
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
    public async Task InsertLargeData_ShouldHandleSuccessfully()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        var longName = new string('A', 100);
        var longEmail = "very.long.email.address." + new string('b', 50) + "@example.com";

        // Act
        await InsertUserAsync(connection, longName, longEmail, 50);
        var result = await ReadSingleAsync<UserRecord>(connection,
            new Sqled("SELECT Name, Email, Age FROM [Users] WHERE Name = ?").Set("name", longName));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(longName, result.Name);
        Assert.Equal(longEmail, result.Email);
    }

    [Fact]
    public async Task BatchInsert_ShouldInsertMultipleRecordsInTransaction()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);

        // Act
        using var transaction = connection.BeginTransaction();
        try
        {
            // 直接执行 SQL 而不是使用辅助方法
            var insertSql1 = new Sqled("INSERT INTO [Users] (Name, Email, Age, CreatedAt) VALUES (?, ?, ?, ?)")
                .Set("name", "Batch1")
                .Set("email", "batch1@example.com")
                .Set("age", 20)
                .Set("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await ExecuteNonQueryAsync(connection, insertSql1, transaction);

            var insertSql2 = new Sqled("INSERT INTO [Users] (Name, Email, Age, CreatedAt) VALUES (?, ?, ?, ?)")
                .Set("name", "Batch2")
                .Set("email", "batch2@example.com")
                .Set("age", 30)
                .Set("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await ExecuteNonQueryAsync(connection, insertSql2, transaction);

            var insertSql3 = new Sqled("INSERT INTO [Users] (Name, Email, Age, CreatedAt) VALUES (?, ?, ?, ?)")
                .Set("name", "Batch3")
                .Set("email", "batch3@example.com")
                .Set("age", 40)
                .Set("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await ExecuteNonQueryAsync(connection, insertSql3, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        var count = await GetRecordCountAsync(connection, "Users");

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task TransactionRollback_ShouldNotCommitChanges()
    {
        // Arrange
        var connection = GetConnection();
        await CreateUsersTableAsync(connection);
        await InsertUserAsync(connection, "Existing", "existing@example.com", 25);
        var countBefore = await GetRecordCountAsync(connection, "Users");

        // Act
        using var transaction = connection.BeginTransaction();
        try
        {
            var insertSql = new Sqled("INSERT INTO [Users] (Name, Email, Age, CreatedAt) VALUES (?, ?, ?, ?)")
                .Set("name", "RollbackUser")
                .Set("email", "rollback@example.com")
                .Set("age", 35)
                .Set("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await ExecuteNonQueryAsync(connection, insertSql, transaction);
            transaction.Rollback();
        }
        catch
        {
            transaction.Rollback();
        }

        var countAfter = await GetRecordCountAsync(connection, "Users");

        // Assert
        Assert.Equal(countBefore, countAfter);
    }

    private async Task CreateUsersTableAsync(DbConnection connection)
    {
        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Email", ColumnType = "VARCHAR(255)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true },
            new DbColumnDesciptor { ColumnName = "CreatedAt", ColumnType = "DATETIME", PrimaryKeyFlag = false, NullableFlag = false }
        };
        var tableDesciptor = new DbTableDesciptor { TableName = "Users" };
        var createTableSql = _provider.SqlProvider.CreateTable(tableDesciptor, columnDesciptors);

        // Print column definitions for debugging
        foreach (var col in columnDesciptors)
        {
            var colDef = _provider.SqlProvider.CreateTableColumnDefine(col);
            Console.WriteLine($"Column: {col.ColumnName} -> {colDef.Sql}");
        }

        Console.WriteLine($"CREATE TABLE SQL: [{createTableSql.Sql}]");
        await ExecuteNonQueryAsync(connection, createTableSql);
    }

    private async Task InsertUserAsync(DbConnection connection, string name, string email, long? age, string? createdAt = null)
    {
        var insertSql = new Sqled("INSERT INTO [Users] (Name, Email, Age, CreatedAt) VALUES (?, ?, ?, ?)")
            .Set("name", name)
            .Set("email", email)
            .Set("age", age.HasValue ? age.Value : DBNull.Value)
            .Set("createdAt", createdAt ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        await ExecuteNonQueryAsync(connection, insertSql);
    }

    private async Task<int> GetRecordCountAsync(DbConnection connection, string tableName)
    {
        var sql = new Sqled($"SELECT COUNT(*) FROM [{tableName}]");
        return await ExecuteScalarAsync<int>(connection, sql);
    }

    private async Task ExecuteNonQueryAsync(DbConnection connection, Sqled sql, DbTransaction? transaction = null)
    {
        using var command = _provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
        if (transaction != null)
            command.Transaction = transaction;
        _provider.SetParameters(command, sql.Parameters);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<int> ExecuteNonQueryWithResultAsync(DbConnection connection, Sqled sql, DbTransaction? transaction = null)
    {
        using var command = _provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
        if (transaction != null)
            command.Transaction = transaction;
        _provider.SetParameters(command, sql.Parameters);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ExecuteScalarAsync<T>(DbConnection connection, Sqled sql)
    {
        using var command = _provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
        _provider.SetParameters(command, sql.Parameters);
        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return default!;
        return typeof(T) == typeof(string) ? (T)result! : (T)Convert.ChangeType(result, typeof(T))!;
    }

    private async Task<T?> ReadSingleAsync<T>(DbConnection connection, Sqled sql) where T : class, new()
    {
        var results = await ReadMultipleAsync<T>(connection, sql);
        return results.FirstOrDefault();
    }

    private async Task<List<T>> ReadMultipleAsync<T>(DbConnection connection, Sqled sql) where T : new()
    {
        var results = new List<T>();
        await _provider.ReadAsync(connection, sql, async reader =>
        {
            while (await reader.ReadAsync())
            {
                var item = new T();
                var properties = typeof(T).GetProperties();
                foreach (var prop in properties)
                {
                    // OleDb 返回的列名可能是大小写不同的，使用不区分大小写的查找
                    var ordinal = -1;
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (string.Equals(reader.GetName(i), prop.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            ordinal = i;
                            break;
                        }
                    }
                    if (ordinal >= 0 && !reader.IsDBNull(ordinal))
                    {
                        var value = reader[ordinal];
                        prop.SetValue(item, value is DBNull ? null : value);
                    }
                }
                results.Add(item);
            }
        });
        return results;
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

    private class UserRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long Age { get; set; }
    }
}