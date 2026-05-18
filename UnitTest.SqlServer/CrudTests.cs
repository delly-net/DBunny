using System.Data.Common;
using System.Linq;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Core;
using Delly.DBunny.Core.Providing.Extension;
using Delly.DBunny.Core.Sql.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.SqlServer;
using Xunit;

namespace UnitTest.SqlServer;

[Collection("SqlServerTests")]
public class CrudTests : IAsyncLifetime
{
    private readonly IDbProvider _provider;
    private readonly DbConnection _connection;
    private readonly DbConnectionDescriptor _connectionDescriptor;
    private readonly string _testDatabaseName;
    private readonly string _testSchema;

    public CrudTests()
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
            .WithEncrypt(false)
            .WithPooling(true)
            .WithMinPoolSize(0)
            .WithMaxPoolSize(100);

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

        await CreateUsersTableAsync();
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
    public async Task Insert_ShouldInsertRecordSuccessfully()
    {
        // Arrange
        var insertSql = new Sqled($"INSERT INTO [{_testSchema}].[Users] (Name, Email, Age, CreatedAt) VALUES (@name, @email, @age, @createdAt)")
            .Set("name", "John Doe")
            .Set("email", "john@example.com")
            .Set("age", 30)
            .Set("createdAt", DateTime.Now);

        // Act
        await ExecuteNonQueryAsync(_connection, insertSql);
        var count = await GetRecordCountAsync("Users");

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Insert_MultipleRecords_ShouldInsertAllSuccessfully()
    {
        // Arrange
        // Act
        await InsertUserAsync("Alice", "alice@example.com", 25);
        await InsertUserAsync("Bob", "bob@example.com", 35);
        await InsertUserAsync("Charlie", "charlie@example.com", 28);
        var count = await GetRecordCountAsync("Users");

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task Select_ShouldRetrieveRecordSuccessfully()
    {
        // Arrange
        await InsertUserAsync("Jane Smith", "jane@example.com", 27);

        // Act
        var selectSql = new Sqled($"SELECT Name, Email, Age FROM [{_testSchema}].[Users] WHERE Name = @name")
            .Set("name", "Jane Smith");
        var result = await ReadSingleAsync<UserRecord>(_connection, selectSql);

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
        await InsertUserAsync("User1", "user1@example.com", 20);
        await InsertUserAsync("User2", "user2@example.com", 30);
        await InsertUserAsync("User3", "user3@example.com", 40);

        // Act
        var selectSql = new Sqled($"SELECT Name, Email, Age FROM [{_testSchema}].[Users] ORDER BY Age");
        var results = await ReadMultipleAsync<UserRecord>(_connection, selectSql);

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
        await InsertUserAsync("UpdateMe", "old@example.com", 25);

        // Act
        var updateSql = new Sqled($"UPDATE [{_testSchema}].[Users] SET Email = @newEmail, Age = @newAge WHERE Name = @name")
            .Set("newEmail", "new@example.com")
            .Set("newAge", 30)
            .Set("name", "UpdateMe");
        await ExecuteNonQueryAsync(_connection, updateSql);

        var selectSql = new Sqled($"SELECT Email, Age FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "UpdateMe");
        var result = await ReadSingleAsync<UserRecord>(_connection, selectSql);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new@example.com", result.Email);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public async Task Update_MultipleColumns_ShouldUpdateAllSuccessfully()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 1, 1, 0, 0, 0);
        await InsertUserAsync("MultiUpdate", "multi@example.com", 20, testDateTime);

        // Act
        var updateDateTime = new DateTime(2024, 12, 31, 23, 59, 59);
        var updateSql = new Sqled($"UPDATE [{_testSchema}].[Users] SET Email = @email, Age = @age, CreatedAt = @createdAt WHERE Name = @name")
            .Set("email", "updated@example.com")
            .Set("age", 35)
            .Set("createdAt", updateDateTime)
            .Set("name", "MultiUpdate");
        await ExecuteNonQueryAsync(_connection, updateSql);

        var selectSql = new Sqled($"SELECT Email, Age FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "MultiUpdate");
        var result = await ReadSingleAsync<UserRecord>(_connection, selectSql);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updated@example.com", result.Email);
        Assert.Equal(35, result.Age);
    }

    [Fact]
    public async Task Update_WithNoMatchingRecord_ShouldNotUpdateAnything()
    {
        // Arrange
        await InsertUserAsync("ExistingUser", "existing@example.com", 30);

        // Act
        var updateSql = new Sqled($"UPDATE [{_testSchema}].[Users] SET Email = @email WHERE Name = @name")
            .Set("email", "new@example.com")
            .Set("name", "NonExistingUser");
        var affectedRows = await ExecuteNonQueryWithResultAsync(_connection, updateSql);

        var selectSql = new Sqled($"SELECT Email FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "ExistingUser");
        var result = await ReadSingleAsync<UserRecord>(_connection, selectSql);

        // Assert
        Assert.Equal(0, affectedRows);
        Assert.NotNull(result);
        Assert.Equal("existing@example.com", result.Email);
    }

    [Fact]
    public async Task Delete_ShouldDeleteRecordSuccessfully()
    {
        // Arrange
        await InsertUserAsync("ToDelete", "delete@example.com", 25);
        var countBefore = await GetRecordCountAsync("Users");

        // Act
        var deleteSql = new Sqled($"DELETE FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "ToDelete");
        await ExecuteNonQueryAsync(_connection, deleteSql);
        var countAfter = await GetRecordCountAsync("Users");

        // Assert
        Assert.NotEqual(countBefore, countAfter);
    }

    [Fact]
    public async Task Delete_WithCondition_ShouldDeleteMatchingRecordsOnly()
    {
        // Arrange
        await InsertUserAsync("UserA", "a@example.com", 20);
        await InsertUserAsync("UserB", "b@example.com", 30);
        await InsertUserAsync("UserC", "c@example.com", 40);

        // Act
        var deleteSql = new Sqled($"DELETE FROM [{_testSchema}].[Users] WHERE Age >= @minAge").Set("minAge", 30);
        await ExecuteNonQueryAsync(_connection, deleteSql);
        var remainingRecords = await ReadMultipleAsync<UserRecord>(_connection,
            new Sqled($"SELECT Name, Age FROM [{_testSchema}].[Users] ORDER BY Name"));

        // Assert
        Assert.Single(remainingRecords);
        Assert.Equal("UserA", remainingRecords[0].Name);
        Assert.Equal(20, remainingRecords[0].Age);
    }

    [Fact]
    public async Task Delete_WithNoMatchingRecord_ShouldNotDeleteAnything()
    {
        // Arrange
        await InsertUserAsync("KeepMe", "keep@example.com", 25);
        var countBefore = await GetRecordCountAsync("Users");

        // Act
        var deleteSql = new Sqled($"DELETE FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "NonExisting");
        var affectedRows = await ExecuteNonQueryWithResultAsync(_connection, deleteSql);
        var countAfter = await GetRecordCountAsync("Users");

        // Assert
        Assert.Equal(0, affectedRows);
        Assert.Equal(1, countBefore);
        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public async Task FullCrudWorkflow_ShouldCompleteSuccessfully()
    {
        // Arrange & Act - Create
        var initialCount = await GetRecordCountAsync("Users");
        Assert.Equal(0, initialCount);

        // Insert
        await InsertUserAsync("WorkflowUser", "workflow@example.com", 28);
        var countAfterInsert = await GetRecordCountAsync("Users");
        Assert.Equal(1, countAfterInsert);

        // Read
        var readResult = await ReadSingleAsync<UserRecord>(_connection,
            new Sqled($"SELECT Name, Email, Age FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "WorkflowUser"));
        Assert.NotNull(readResult);
        Assert.Equal("workflow@example.com", readResult.Email);
        Assert.Equal(28, readResult.Age);

        // Update
        var updateSql = new Sqled($"UPDATE [{_testSchema}].[Users] SET Age = @newAge WHERE Name = @name")
            .Set("newAge", 35)
            .Set("name", "WorkflowUser");
        await ExecuteNonQueryAsync(_connection, updateSql);
        var updatedResult = await ReadSingleAsync<UserRecord>(_connection,
            new Sqled($"SELECT Age FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "WorkflowUser"));
        Assert.NotNull(updatedResult);
        Assert.Equal(35, updatedResult.Age);

        // Delete
        var deleteSql = new Sqled($"DELETE FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "WorkflowUser");
        await ExecuteNonQueryAsync(_connection, deleteSql);
        var finalCount = await GetRecordCountAsync("Users");
        Assert.Equal(0, finalCount);
    }

    [Fact]
    public async Task InsertWithSpecialCharacters_ShouldHandleSuccessfully()
    {
        // Arrange
        var specialName = "O'Reilly \"The\" Boss";
        var specialEmail = "test+special@example.com";

        // Act
        await InsertUserAsync(specialName, specialEmail, 45);
        var result = await ReadSingleAsync<UserRecord>(_connection,
            new Sqled($"SELECT Name, Email FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", specialName));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(specialName, result.Name);
        Assert.Equal(specialEmail, result.Email);
    }

    [Fact]
    public async Task InsertWithNullNullableColumn_ShouldHandleSuccessfully()
    {
        // Arrange
        // Act
        var insertSql = new Sqled($"INSERT INTO [{_testSchema}].[Users] (Name, Email, Age, CreatedAt) VALUES (@name, @email, @age, @createdAt)")
            .Set("name", "NullAgeUser")
            .Set("email", "nullage@example.com")
            .Set("age", DBNull.Value)
            .Set("createdAt", DateTime.Now);
        await ExecuteNonQueryAsync(_connection, insertSql);

        var selectSql = new Sqled($"SELECT Age FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "NullAgeUser");
        var result = await ExecuteScalarAsync<object?>(_connection, selectSql);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InsertWithDateTime_ShouldStoreAndRetrieveCorrectly()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 6, 15, 14, 30, 45);

        // Act
        await InsertUserAsync("DateTimeUser", "datetime@example.com", 30, testDateTime);
        var selectSql = new Sqled($"SELECT CreatedAt FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", "DateTimeUser");
        var result = await ExecuteScalarAsync<DateTime>(_connection, selectSql);

        // Assert
        Assert.Equal(testDateTime, result);
    }

    [Fact]
    public void ConnectionDescriptor_ShouldHaveCorrectProperties()
    {
        // Assert
        Assert.Equal("Default", _connectionDescriptor.Name);
        Assert.Equal("SQLSERVER", _connectionDescriptor.DatabaseType);
        Assert.Contains("Data Source=", _connectionDescriptor.ConnectionString);
        Assert.Contains("Database=", _connectionDescriptor.ConnectionString);
        Assert.Contains("Pooling=True", _connectionDescriptor.ConnectionString);
    }

    [Fact]
    public async Task InsertLargeData_ShouldHandleSuccessfully()
    {
        // Arrange
        var longName = new string('A', 100);
        var longEmail = "very.long.email.address." + new string('b', 50) + "@example.com";

        // Act
        await InsertUserAsync(longName, longEmail, 50);
        var result = await ReadSingleAsync<UserRecord>(_connection,
            new Sqled($"SELECT Name, Email FROM [{_testSchema}].[Users] WHERE Name = @name").Set("name", longName));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(longName, result.Name);
        Assert.Equal(longEmail, result.Email);
    }

    [Fact]
    public async Task BatchInsert_ShouldInsertMultipleRecordsInTransaction()
    {
        // Arrange
        // Act
        using var transaction = _connection.BeginTransaction();
        try
        {
            await InsertUserAsync("Batch1", "batch1@example.com", 20, null, transaction);
            await InsertUserAsync("Batch2", "batch2@example.com", 30, null, transaction);
            await InsertUserAsync("Batch3", "batch3@example.com", 40, null, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        var count = await GetRecordCountAsync("Users");

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task TransactionRollback_ShouldNotCommitChanges()
    {
        // Arrange
        await TruncateUsersTableAsync();
        await InsertUserAsync("Existing", "existing@example.com", 25);
        var countBefore = await GetRecordCountAsync("Users");

        // Act
        using var transaction = _connection.BeginTransaction();
        try
        {
            await InsertUserAsync("RollbackUser", "rollback@example.com", 35);
            transaction.Rollback();
        }
        catch
        {
            transaction.Rollback();
        }

        var countAfter = await GetRecordCountAsync("Users");

        // Assert
        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public async Task GetIndexes_ShouldReturnCorrectIndexes()
    {
        // Arrange
        var tableName = "TestIndexes";
        await CreateSimpleTableAsync(_testSchema, tableName, "Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY, Email NVARCHAR(255) NOT NULL, Username NVARCHAR(100) NOT NULL");

        // Act
        var indexDesciptor1 = new DbIndexDesciptor { SchemaName = _testSchema, TableName = tableName, IndexName = "Email", UniqueFlag = true, ColumnName = "Email" };
        var indexDesciptor2 = new DbIndexDesciptor { SchemaName = _testSchema, TableName = tableName, IndexName = "Username", UniqueFlag = false, ColumnName = "Username" };
        await ExecuteNonQueryAsync(_connection, _provider.SqlProvider.CreateIndex(indexDesciptor1));
        await ExecuteNonQueryAsync(_connection, _provider.SqlProvider.CreateIndex(indexDesciptor2));

        var indexes = await _provider.GetIndexes(_connection, _testSchema, tableName);

        // Assert
        Assert.Equal(2, indexes.Count);
        Assert.Contains(indexes, i => i.UniqueFlag == true);
        Assert.Contains(indexes, i => i.UniqueFlag == false);
    }

    [Fact]
    public async Task GetColumns_ShouldReturnCorrectColumnTypes()
    {
        // Arrange
        var tableName = "TestColumnTypes";
        var dropSql = new Sqled($"IF OBJECT_ID('[{_testSchema}].[{tableName}]', 'U') IS NOT NULL DROP TABLE [{_testSchema}].[{tableName}]");
        await ExecuteNonQueryAsync(_connection, dropSql);

        var columnDesciptors = new List<DbColumnDesciptor>
        {
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Price", ColumnType = "DECIMAL(18,2)", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "IsActive", ColumnType = "BIT", PrimaryKeyFlag = false, NullableFlag = false },
            new DbColumnDesciptor { SchemaName = _testSchema, TableName = tableName, ColumnName = "Score", ColumnType = "REAL", PrimaryKeyFlag = false, NullableFlag = true },
        };

        await ExecuteNonQueryAsync(_connection, _provider.SqlProvider.CreateTable(_testSchema, tableName, columnDesciptors));

        // Act
        var columns = await _provider.GetColumns(_connection, _testSchema, tableName);

        // Assert
        Assert.Equal(4, columns.Count);

        var priceColumn = columns.First(c => c.ColumnName == "Price");
        Assert.Contains("decimal", priceColumn.ColumnType, StringComparison.OrdinalIgnoreCase);

        var isActiveColumn = columns.First(c => c.ColumnName == "IsActive");
        Assert.Contains("bit", isActiveColumn.ColumnType, StringComparison.OrdinalIgnoreCase);

        var scoreColumn = columns.First(c => c.ColumnName == "Score");
        Assert.Contains("real", scoreColumn.ColumnType, StringComparison.OrdinalIgnoreCase);
        Assert.True(scoreColumn.NullableFlag);
    }

    private async Task CreateUsersTableAsync()
    {
        var tableName = "Users";
        var dropSql = new Sqled($"IF OBJECT_ID('[{_testSchema}].[{tableName}]', 'U') IS NOT NULL DROP TABLE [{_testSchema}].[{tableName}]");
        await ExecuteNonQueryAsync(_connection, dropSql);

        var sql = new Sqled();
        sql.Builder.AppendLine($"CREATE TABLE [{_testSchema}].[{tableName}](");
        sql.Builder.Append("    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,");
        sql.Builder.Append("    [Name] NVARCHAR(100) NOT NULL,");
        sql.Builder.Append("    [Email] NVARCHAR(255) NOT NULL,");
        sql.Builder.Append("    [Age] INT NULL,");
        sql.Builder.AppendLine("    [CreatedAt] DATETIME NOT NULL");
        sql.Builder.AppendLine(");");
        await ExecuteNonQueryAsync(_connection, sql);
    }

    private async Task TruncateUsersTableAsync()
    {
        var sql = new Sqled($"TRUNCATE TABLE [{_testSchema}].[Users]");
        await ExecuteNonQueryAsync(_connection, sql);
    }

    private async Task InsertUserAsync(string name, string email, int? age, DateTime? createdAt = null, DbTransaction? transaction = null)
    {
        var insertSql = new Sqled($"INSERT INTO [{_testSchema}].[Users] (Name, Email, Age, CreatedAt) VALUES (@name, @email, @age, @createdAt)")
            .Set("name", name)
            .Set("email", email)
            .Set("age", age.HasValue ? age.Value : DBNull.Value)
            .Set("createdAt", createdAt ?? DateTime.Now);
        await ExecuteNonQueryAsync(_connection, insertSql, transaction);
    }

    private async Task<int> GetRecordCountAsync(string tableName)
    {
        var sql = new Sqled($"SELECT COUNT(*) FROM [{_testSchema}].[{tableName}]");
        return await ExecuteScalarAsync<int>(_connection, sql);
    }

    private async Task ExecuteNonQueryAsync(DbConnection connection, Sqled sql, DbTransaction? transaction = null)
    {
        using var command = _provider.GetDbCommand(connection);
        if (transaction is not null) { command.Transaction = transaction; }
        command.CommandText = sql.Sql;
        _provider.SetParameters(command, sql.Parameters);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<int> ExecuteNonQueryWithResultAsync(DbConnection connection, Sqled sql)
    {
        using var command = _provider.GetDbCommand(connection);
        command.CommandText = sql.Sql;
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
                    int ordinal;
                    try
                    {
                        ordinal = reader.GetOrdinal(prop.Name);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // Try case-insensitive lookup
                        ordinal = -1;
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (string.Equals(reader.GetName(i), prop.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                ordinal = i;
                                break;
                            }
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

    private async Task CreateSimpleTableAsync(string schema, string tableName, string columns)
    {
        var dropSql = $"IF OBJECT_ID('[{schema}].[{tableName}]', 'U') IS NOT NULL DROP TABLE [{schema}].[{tableName}];";
        await ExecuteNonQueryAsync(_connection, dropSql);
        var sql = $"CREATE TABLE [{schema}].[{tableName}]({columns});";
        await ExecuteNonQueryAsync(_connection, sql);
    }

    private class UserRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}