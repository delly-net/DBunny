using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using Delly.DBunny;
using Delly.DBunny.Providing.Extension;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Sqlite;
using Xunit;

namespace UnitTest.Sqlite;

public class CrudTests : IDisposable
{
    private readonly SqliteProvider _provider;
    private readonly DbConnection _connection;
    private readonly string _testDbPath;

    public CrudTests()
    {
        _provider = new SqliteProvider();
        _testDbPath = Path.Combine(Path.GetTempPath(), $"testdb_{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={_testDbPath}";
        _connection = _provider.GetDbConnection(connectionString);
        _connection.Open();
    }

    [Fact]
    public async Task Insert_ShouldInsertRecordSuccessfully()
    {
        // Arrange
        await CreateUsersTableAsync();
        var insertSql = new Sqled("INSERT INTO [Users] (Name, Email, Age, CreatedAt) VALUES (@name, @email, @age, @createdAt)")
            .Set("name", "John Doe")
            .Set("email", "john@example.com")
            .Set("age", 30)
            .Set("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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
        await CreateUsersTableAsync();

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
        await CreateUsersTableAsync();
        await InsertUserAsync("Jane Smith", "jane@example.com", 27);

        // Act
        var selectSql = new Sqled("SELECT Name, Email, Age FROM [Users] WHERE Name = @name")
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
        await CreateUsersTableAsync();
        await InsertUserAsync("User1", "user1@example.com", 20);
        await InsertUserAsync("User2", "user2@example.com", 30);
        await InsertUserAsync("User3", "user3@example.com", 40);

        // Act
        var selectSql = new Sqled("SELECT Name, Email, Age FROM [Users] ORDER BY Age");
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
        await CreateUsersTableAsync();
        await InsertUserAsync("UpdateMe", "old@example.com", 25);

        // Act
        var updateSql = new Sqled("UPDATE [Users] SET Email = @newEmail, Age = @newAge WHERE Name = @name")
            .Set("newEmail", "new@example.com")
            .Set("newAge", 30)
            .Set("name", "UpdateMe");
        await ExecuteNonQueryAsync(_connection, updateSql);

        var selectSql = new Sqled("SELECT Email, Age FROM [Users] WHERE Name = @name").Set("name", "UpdateMe");
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
        await CreateUsersTableAsync();
        await InsertUserAsync("MultiUpdate", "multi@example.com", 20, "2024-01-01 00:00:00");

        // Act
        var updateSql = new Sqled("UPDATE [Users] SET Email = @email, Age = @age, CreatedAt = @createdAt WHERE Name = @name")
            .Set("email", "updated@example.com")
            .Set("age", 35)
            .Set("createdAt", "2024-12-31 23:59:59")
            .Set("name", "MultiUpdate");
        await ExecuteNonQueryAsync(_connection, updateSql);

        var selectSql = new Sqled("SELECT Email, Age FROM [Users] WHERE Name = @name").Set("name", "MultiUpdate");
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
        await CreateUsersTableAsync();
        await InsertUserAsync("ExistingUser", "existing@example.com", 30);

        // Act
        var updateSql = new Sqled("UPDATE [Users] SET Email = @email WHERE Name = @name")
            .Set("email", "new@example.com")
            .Set("name", "NonExistingUser");
        var affectedRows = await ExecuteNonQueryWithResultAsync(_connection, updateSql);

        var selectSql = new Sqled("SELECT Email FROM [Users] WHERE Name = @name").Set("name", "ExistingUser");
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
        await CreateUsersTableAsync();
        await InsertUserAsync("ToDelete", "delete@example.com", 25);
        var countBefore = await GetRecordCountAsync("Users");

        // Act
        var deleteSql = new Sqled("DELETE FROM [Users] WHERE Name = @name").Set("name", "ToDelete");
        await ExecuteNonQueryAsync(_connection, deleteSql);
        var countAfter = await GetRecordCountAsync("Users");

        // Assert
        Assert.Equal(1, countBefore);
        Assert.Equal(0, countAfter);
    }

    [Fact]
    public async Task Delete_WithCondition_ShouldDeleteMatchingRecordsOnly()
    {
        // Arrange
        await CreateUsersTableAsync();
        await InsertUserAsync("UserA", "a@example.com", 20);
        await InsertUserAsync("UserB", "b@example.com", 30);
        await InsertUserAsync("UserC", "c@example.com", 40);

        // Act
        var deleteSql = new Sqled("DELETE FROM [Users] WHERE Age >= @minAge").Set("minAge", 30);
        await ExecuteNonQueryAsync(_connection, deleteSql);
        var remainingRecords = await ReadMultipleAsync<UserRecord>(_connection,
            new Sqled("SELECT Name, Age FROM [Users] ORDER BY Name"));

        // Assert
        Assert.Single(remainingRecords);
        Assert.Equal("UserA", remainingRecords[0].Name);
        Assert.Equal(20, remainingRecords[0].Age);
    }

    [Fact]
    public async Task Delete_WithNoMatchingRecord_ShouldNotDeleteAnything()
    {
        // Arrange
        await CreateUsersTableAsync();
        await InsertUserAsync("KeepMe", "keep@example.com", 25);
        var countBefore = await GetRecordCountAsync("Users");

        // Act
        var deleteSql = new Sqled("DELETE FROM [Users] WHERE Name = @name").Set("name", "NonExisting");
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
        await CreateUsersTableAsync();
        var initialCount = await GetRecordCountAsync("Users");
        Assert.Equal(0, initialCount);

        // Insert
        await InsertUserAsync("WorkflowUser", "workflow@example.com", 28);
        var countAfterInsert = await GetRecordCountAsync("Users");
        Assert.Equal(1, countAfterInsert);

        // Read
        var readResult = await ReadSingleAsync<UserRecord>(_connection,
            new Sqled("SELECT Name, Email, Age FROM [Users] WHERE Name = @name").Set("name", "WorkflowUser"));
        Assert.NotNull(readResult);
        Assert.Equal("workflow@example.com", readResult.Email);
        Assert.Equal(28, readResult.Age);

        // Update
        var updateSql = new Sqled("UPDATE [Users] SET Age = @newAge WHERE Name = @name")
            .Set("newAge", 35)
            .Set("name", "WorkflowUser");
        await ExecuteNonQueryAsync(_connection, updateSql);
        var updatedResult = await ReadSingleAsync<UserRecord>(_connection,
            new Sqled("SELECT Age FROM [Users] WHERE Name = @name").Set("name", "WorkflowUser"));
        Assert.Equal(35, updatedResult.Age);

        // Delete
        var deleteSql = new Sqled("DELETE FROM [Users] WHERE Name = @name").Set("name", "WorkflowUser");
        await ExecuteNonQueryAsync(_connection, deleteSql);
        var finalCount = await GetRecordCountAsync("Users");
        Assert.Equal(0, finalCount);
    }

    [Fact]
    public async Task InsertWithSpecialCharacters_ShouldHandleSuccessfully()
    {
        // Arrange
        await CreateUsersTableAsync();
        var specialName = "O'Reilly \"The\" Boss";
        var specialEmail = "test+special@example.com";

        // Act
        await InsertUserAsync(specialName, specialEmail, 45);
        var result = await ReadSingleAsync<UserRecord>(_connection,
            new Sqled("SELECT Name, Email FROM [Users] WHERE Name = @name").Set("name", specialName));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(specialName, result.Name);
        Assert.Equal(specialEmail, result.Email);
    }

    private async Task CreateUsersTableAsync()
    {
        var sql = new Sqled();
        sql.Builder.AppendLine("CREATE TABLE [Users](");
        sql.Builder.Append("    [Id] INTEGER NOT NULL PRIMARY KEY,");
        sql.Builder.Append("    [Name] TEXT(100) NOT NULL,");
        sql.Builder.Append("    [Email] TEXT(255) NOT NULL,");
        sql.Builder.Append("    [Age] INTEGER NULL,");
        sql.Builder.AppendLine("    [CreatedAt] TEXT(32) NOT NULL");
        sql.Builder.AppendLine(");");
        await ExecuteNonQueryAsync(_connection, sql);
    }

    private async Task InsertUserAsync(string name, string email, int age, string? createdAt = null)
    {
        var insertSql = new Sqled("INSERT INTO [Users] (Name, Email, Age, CreatedAt) VALUES (@name, @email, @age, @createdAt)")
            .Set("name", name)
            .Set("email", email)
            .Set("age", age)
            .Set("createdAt", createdAt ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        await ExecuteNonQueryAsync(_connection, insertSql);
    }

    private async Task<int> GetRecordCountAsync(string tableName)
    {
        var sql = new Sqled($"SELECT COUNT(*) FROM [{tableName}]");
        return await ExecuteScalarAsync<int>(_connection, sql);
    }

    private async Task ExecuteNonQueryAsync(DbConnection connection, Sqled sql)
    {
        using var command = _provider.GetDbCommand(connection);
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
        return result != null && result != DBNull.Value ? (T)Convert.ChangeType(result, typeof(T)) : default!;
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
                    if (!reader.IsDBNull(reader.GetOrdinal(prop.Name)))
                    {
                        var value = reader[prop.Name];
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
        public int Age { get; set; }
    }
}