using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Delly.DBunny;
using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
using Delly.DBunny.Filting;
using Delly.DBunny.Providing;
using Delly.DBunny.Sqlite;
using Delly.DBunny.Working;
using Delly.DBunny.Working.Extension;
using Eazy.Data.Work.Extension;
using Xunit;

namespace UnitTest.Sqlite;

/// <summary>
/// Unit tests for DefaultDbWork and IDbWork extension methods
/// </summary>
public class DbWorkTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly DbConnectionDescriptor _connectionDescriptor;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbProviderFactory _providerFactory;
    private readonly IDbFilterFactory _filterFactory;
    private readonly IDbManager _manager;

    public DbWorkTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"testdb_work_{Guid.NewGuid():N}.db");

        // Setup connection descriptor
        var connectionDefine = new SqliteConnectionDefine()
            .WithDataSource(_testDbPath)
            .WithPooling(false)
            .WithForeignKeys(true);

        _connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(SqliteConnectionDefine.DATABASE_TYPE, "Default");
        _connectionFactory = new DefaultDbConnectionFactory(_connectionDescriptor);
        _providerFactory = new DefaultDbProviderFactory(new SqliteProvider());
        _filterFactory = new DefaultDbFilterFactory();
        _manager = new DefaultDbManager(_connectionFactory, _filterFactory, _providerFactory);
    }

    #region DefaultDbWork Properties Tests

    [Fact]
    public void DefaultDbWork_Manager_ShouldReturnCorrectManager()
    {
        // Arrange & Act
        using var work = _manager.CreateWork();

        // Assert
        Assert.Same(_manager, work.Manager);
    }

    [Fact]
    public void DefaultDbWork_Provider_ShouldReturnCorrectProvider()
    {
        // Arrange & Act
        using var work = _manager.CreateWork();

        // Assert
        Assert.NotNull(work.Provider);
        Assert.Equal("SQLITE", work.Provider.DatabaseType);
    }

    [Fact]
    public void DefaultDbWork_ConnectionDescriptor_ShouldReturnCorrectDescriptor()
    {
        // Arrange & Act
        using var work = _manager.CreateWork();

        // Assert
        Assert.Same(_connectionDescriptor, work.ConnectionDescriptor);
        Assert.Equal("Default", work.ConnectionDescriptor.Name);
        Assert.Equal("SQLITE", work.ConnectionDescriptor.DatabaseType);
    }

    [Fact]
    public void DefaultDbWork_Filters_ShouldReturnEmptyWhenNoFilters()
    {
        // Arrange & Act
        using var work = _manager.CreateWork();

        // Assert
        Assert.Empty(work.Filters);
    }

    [Fact]
    public void DefaultDbWork_Filters_ShouldReturnProvidedFilters()
    {
        // Arrange
        var filter1 = new TestDbFilter("Filter1");
        var filter2 = new TestDbFilter("Filter2");
        var filterFactory = new DefaultDbFilterFactory(filter1, filter2);
        var managerWithFilters = new DefaultDbManager(_connectionFactory, filterFactory, _providerFactory);

        // Act
        using var work = managerWithFilters.CreateWork();

        // Assert
        Assert.Equal(2, work.Filters.Count());
        Assert.Contains(filter1, work.Filters);
        Assert.Contains(filter2, work.Filters);
    }

    [Fact]
    public void DefaultDbWork_Sqleds_ShouldReturnEmptyListInitially()
    {
        // Arrange & Act
        using var work = _manager.CreateWork();

        // Assert
        Assert.NotNull(work.Sqleds);
        Assert.Empty(work.Sqleds);
    }

    [Fact]
    public void DefaultDbWork_Sqleds_ShouldAllowAddingAndClearing()
    {
        // Arrange
        using var work = _manager.CreateWork();
        var sql1 = new Sqled("SELECT 1");
        var sql2 = new Sqled("SELECT 2");

        // Act
        work.Sqleds.Add(sql1);
        work.Sqleds.Add(sql2);

        // Assert
        Assert.Equal(2, work.Sqleds.Count);
        Assert.Same(sql1, work.Sqleds[0]);
        Assert.Same(sql2, work.Sqleds[1]);

        // Act - Clear
        work.Sqleds.Clear();

        // Assert
        Assert.Empty(work.Sqleds);
    }

    #endregion

    #region Connect / ConnectAsync Tests

    [Fact]
    public void Connect_ShouldReturnOpenConnection()
    {
        // Arrange
        using var work = _manager.CreateWork();

        // Act
        using var connection = work.Connect();

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task ConnectAsync_ShouldReturnOpenConnection()
    {
        // Arrange
        using var work = _manager.CreateWork();

        // Act
        using var connection = await work.ConnectAsync();

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public void Connect_MultipleCalls_ShouldCreateDifferentConnections()
    {
        // Arrange
        using var work = _manager.CreateWork();

        // Act
        using var connection1 = work.Connect();
        using var connection2 = work.Connect();

        // Assert
        Assert.NotSame(connection1, connection2);
    }

    #endregion

    #region GetSqlCommand Tests

    [Fact]
    public void GetSqlCommand_ShouldSetCommandTextAndParameters()
    {
        // Arrange
        using var work = _manager.CreateWork();
        using var connection = work.Connect();
        var sql = new Sqled("SELECT * FROM Test WHERE Id = @id").Set("id", 42);

        // Act
        var command = work.GetSqlCommand(connection, sql);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(sql.Sql, command.CommandText);
        Assert.Single(command.Parameters);
        var param = command.Parameters[0];
        Assert.Equal("@id", param.ParameterName);
        Assert.Equal(42, param.Value);
    }

    [Fact]
    public void GetSqlCommand_ShouldApplySqlLoadingFilter()
    {
        // Arrange
        var filter = new TestDbFilter("TestFilter") { SqlLoadingTransform = s => new Sqled(s.Sql + " AND Active = 1") };
        var filterFactory = new DefaultDbFilterFactory(filter);
        var managerWithFilter = new DefaultDbManager(_connectionFactory, filterFactory, _providerFactory);
        using var work = managerWithFilter.CreateWork();
        using var connection = work.Connect();
        var sql = new Sqled("SELECT * FROM Test");

        // Act
        var command = work.GetSqlCommand(connection, sql);

        // Assert
        Assert.Contains("AND Active = 1", command.CommandText);
    }

    [Fact]
    public void GetSqlCommand_ShouldApplyCommandCreatingFilter()
    {
        // Arrange
        var filterCalled = false;
        var filter = new TestDbFilter("TestFilter")
        {
            CommandCreatingCallback = cmd =>
            {
                filterCalled = true;
                return null; // Return null to use provider's command
            }
        };
        var filterFactory = new DefaultDbFilterFactory(filter);
        var managerWithFilter = new DefaultDbManager(_connectionFactory, filterFactory, _providerFactory);
        using var work = managerWithFilter.CreateWork();
        using var connection = work.Connect();
        var sql = new Sqled("SELECT 1");

        // Act
        work.GetSqlCommand(connection, sql);

        // Assert
        Assert.True(filterCalled);
    }

    [Fact]
    public void GetSqlCommand_ShouldApplyCommandExecutingFilter()
    {
        // Arrange
        var customCommandExecuted = false;
        var filter = new TestDbFilter("TestFilter")
        {
            CommandExecutingCallback = cmd =>
            {
                customCommandExecuted = true;
                return cmd;
            }
        };
        var filterFactory = new DefaultDbFilterFactory(filter);
        var managerWithFilter = new DefaultDbManager(_connectionFactory, filterFactory, _providerFactory);
        using var work = managerWithFilter.CreateWork();
        using var connection = work.Connect();
        var sql = new Sqled("SELECT 1");

        // Act
        var command = work.GetSqlCommand(connection, sql);

        // Assert
        Assert.True(customCommandExecuted);
        Assert.NotNull(command);
    }

    [Fact]
    public void GetSqlCommand_ShouldApplyMultipleFiltersInOrder()
    {
        // Arrange
        var filter1 = new TestDbFilter("Filter1") { SqlLoadingTransform = s => new Sqled(s.Sql + " AND A = 1") };
        var filter2 = new TestDbFilter("Filter2") { SqlLoadingTransform = s => new Sqled(s.Sql + " AND B = 2") };
        var filterFactory = new DefaultDbFilterFactory(filter1, filter2);
        var managerWithFilter = new DefaultDbManager(_connectionFactory, filterFactory, _providerFactory);
        using var work = managerWithFilter.CreateWork();
        using var connection = work.Connect();
        var sql = new Sqled("SELECT * FROM Test");

        // Act
        var command = work.GetSqlCommand(connection, sql);

        // Assert
        Assert.Contains("AND A = 1", command.CommandText);
        Assert.Contains("AND B = 2", command.CommandText);
    }

    #endregion

    #region Commit / CommitAsync Tests

    [Fact]
    public async Task Commit_WithNoSqleds_ShouldReturnZero()
    {
        // Arrange
        using var work = _manager.CreateWork();
        await CreateTestTableAsync();

        // Act
        var result = work.Commit();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Commit_WithSingleSqled_ShouldExecuteAndReturnRowCount()
    {
        // Arrange
        using var work = _manager.CreateWork();
        await CreateTestTableAsync();
        work.Sqleds.Add(new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test1")
            .Set("value", 100));

        // Act
        var result = work.Commit();

        // Assert
        Assert.Equal(1, result);
        Assert.Empty(work.Sqleds); // Sqleds should be cleared after commit
        var count = await GetRecordCountAsync("TestTable");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Commit_WithMultipleSqleds_ShouldExecuteAllInTransaction()
    {
        // Arrange
        using var work = _manager.CreateWork();
        await CreateTestTableAsync();
        work.Sqleds.Add(new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test1")
            .Set("value", 100));
        work.Sqleds.Add(new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test2")
            .Set("value", 200));
        work.Sqleds.Add(new Sqled("UPDATE [TestTable] SET Value = @value WHERE Name = @name")
            .Set("value", 150)
            .Set("name", "Test1"));

        // Act
        var result = work.Commit();

        // Assert
        Assert.Equal(3, result); // 2 inserts + 1 update
        Assert.Empty(work.Sqleds);
        var count = await GetRecordCountAsync("TestTable");
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task Commit_WithInvalidSql_ShouldRollbackAndThrowSqlException()
    {
        // Arrange
        using var work = _manager.CreateWork();
        await CreateTestTableAsync();
        work.Sqleds.Add(new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Valid")
            .Set("value", 100));
        work.Sqleds.Add(new Sqled("INSERT INTO [NonExistentTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Invalid")
            .Set("value", 200));

        // Act & Assert
        var exception = Assert.Throws<SqlException>(() => work.Commit());
        Assert.NotNull(exception.Sqled);
        Assert.Contains("NonExistentTable", exception.Message);

        // Verify rollback - no records should be inserted
        var count = await GetRecordCountAsync("TestTable");
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task CommitAsync_WithNoSqleds_ShouldReturnZero()
    {
        // Arrange
        using var work = _manager.CreateWork();
        await CreateTestTableAsync();

        // Act
        var result = await work.CommitAsync();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CommitAsync_WithSingleSqled_ShouldExecuteAndReturnRowCount()
    {
        // Arrange
        using var work = _manager.CreateWork();
        await CreateTestTableAsync();
        work.Sqleds.Add(new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test1")
            .Set("value", 100));

        // Act
        var result = await work.CommitAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Empty(work.Sqleds);
        var count = await GetRecordCountAsync("TestTable");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CommitAsync_WithMultipleSqleds_ShouldExecuteAllInTransaction()
    {
        // Arrange
        using var work = _manager.CreateWork();
        await CreateTestTableAsync();
        work.Sqleds.Add(new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test1")
            .Set("value", 100));
        work.Sqleds.Add(new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test2")
            .Set("value", 200));
        work.Sqleds.Add(new Sqled("UPDATE [TestTable] SET Value = @value WHERE Name = @name")
            .Set("value", 150)
            .Set("name", "Test1"));

        // Act
        var result = await work.CommitAsync();

        // Assert
        Assert.Equal(3, result);
        Assert.Empty(work.Sqleds);
        var count = await GetRecordCountAsync("TestTable");
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CommitAsync_WithInvalidSql_ShouldRollbackAndThrowSqlException()
    {
        // Arrange
        using var work = _manager.CreateWork();
        await CreateTestTableAsync();
        work.Sqleds.Add(new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Valid")
            .Set("value", 100));
        work.Sqleds.Add(new Sqled("INSERT INTO [NonExistentTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Invalid")
            .Set("value", 200));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<SqlException>(() => work.CommitAsync());
        Assert.NotNull(exception.Sqled);
        Assert.Contains("NonExistentTable", exception.Message);

        // Verify rollback
        var count = await GetRecordCountAsync("TestTable");
        Assert.Equal(0, count);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ShouldCallReleaseWorkOnManager()
    {
        // Arrange
        var managerWithFilter = new DefaultDbManager(_connectionFactory, _filterFactory, _providerFactory);

        // Act - Create work via manager which sets it as current
        using (var work = managerWithFilter.CreateWork("Default"))
        {
            // Work is set as current
            Assert.NotNull(managerWithFilter.GetCurrentWork());
        }

        // Assert - After dispose, current work should be null
        Assert.Null(managerWithFilter.GetCurrentWork());
    }

    [Fact]
    public void Dispose_MultipleCalls_ShouldNotThrow()
    {
        // Arrange
        using var work = _manager.CreateWork();

        // Act - Dispose multiple times
        work.Dispose();
        work.Dispose();

        // Assert - Should not throw
    }

    [Fact]
    public void Dispose_WithSqledsInList_ShouldClearSqleds()
    {
        // Arrange
        var work = _manager.CreateWork();
        work.Sqleds.Add(new Sqled("SELECT 1"));
        work.Sqleds.Add(new Sqled("SELECT 2"));

        // Act
        work.Dispose();

        // Assert - Dispose should not affect the Sqleds list directly
        Assert.Equal(2, work.Sqleds.Count);
    }

    #endregion

    #region ExecuteNonQuery Extension Tests

    [Fact]
    public async Task ExecuteNonQuery_ShouldExecuteSingleStatement()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test1")
            .Set("value", 100);

        // Act
        var result = work.ExecuteNonQuery(sql);

        // Assert
        Assert.Equal(1, result);
        var count = await GetRecordCountAsync("TestTable");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ExecuteNonQuery_ShouldClearPreviousSqleds()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        work.Sqleds.Add(new Sqled("SELECT 1"));
        var sql = new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test1")
            .Set("value", 100);

        // Act
        work.ExecuteNonQuery(sql);

        // Assert
        Assert.Empty(work.Sqleds);
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_ShouldExecuteSingleStatement()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test1")
            .Set("value", 100);

        // Act
        var result = await work.ExecuteNonQueryAsync(sql);

        // Assert
        Assert.Equal(1, result);
        var count = await GetRecordCountAsync("TestTable");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_ShouldClearPreviousSqleds()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        work.Sqleds.Add(new Sqled("SELECT 1"));
        var sql = new Sqled("INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)")
            .Set("name", "Test1")
            .Set("value", 100);

        // Act
        await work.ExecuteNonQueryAsync(sql);

        // Assert
        Assert.Empty(work.Sqleds);
    }

    #endregion

    #region GetDataSet Extension Tests

    [Fact]
    public async Task GetDataSet_ShouldReturnDataSetWithData()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        await InsertTestRecordAsync("Test1", 100);
        var sql = new Sqled("SELECT Name, Value FROM [TestTable]");

        // Act
        var dataSet = work.GetDataSet(sql);

        // Assert
        Assert.NotNull(dataSet);
        Assert.Single(dataSet.Tables);
        var table = dataSet.Tables[0];
        Assert.Equal(1, table.Rows.Count);
        Assert.Equal("Test1", table.Rows[0]["Name"]);
        Assert.Equal(100L, table.Rows[0]["Value"]);
    }

    [Fact]
    public async Task GetDataSet_WithNoResults_ShouldReturnEmptyDataSet()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable]");

        // Act
        var dataSet = work.GetDataSet(sql);

        // Assert
        Assert.NotNull(dataSet);
        Assert.Single(dataSet.Tables);
        Assert.Empty(dataSet.Tables[0].Rows);
    }

    [Fact]
    public async Task GetDataSetAsync_ShouldReturnDataSetWithData()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        await InsertTestRecordAsync("Test1", 100);
        var sql = new Sqled("SELECT Name, Value FROM [TestTable]");

        // Act
        var dataSet = await work.GetDataSetAsync(sql);

        // Assert
        Assert.NotNull(dataSet);
        Assert.Single(dataSet.Tables);
        var table = dataSet.Tables[0];
        Assert.Equal(1, table.Rows.Count);
        Assert.Equal("Test1", table.Rows[0]["Name"]);
        Assert.Equal(100L, table.Rows[0]["Value"]);
    }

    #endregion

    #region GetDataTable Extension Tests

    [Fact]
    public async Task GetDataTable_ShouldReturnDataTableWithData()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        await InsertTestRecordAsync("Test1", 100);
        var sql = new Sqled("SELECT Name, Value FROM [TestTable]");

        // Act
        var dataTable = work.GetDataTable(sql);

        // Assert
        Assert.NotNull(dataTable);
        Assert.Equal(1, dataTable.Rows.Count);
        Assert.Equal("Test1", dataTable.Rows[0]["Name"]);
        Assert.Equal(100L, dataTable.Rows[0]["Value"]);
    }

    [Fact]
    public async Task GetDataTable_WithNoResults_ShouldReturnEmptyDataTable()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable]");

        // Act
        var dataTable = work.GetDataTable(sql);

        // Assert
        Assert.NotNull(dataTable);
        Assert.Empty(dataTable.Rows);
    }

    [Fact]
    public async Task GetDataTableAsync_ShouldReturnDataTableWithData()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        await InsertTestRecordAsync("Test1", 100);
        var sql = new Sqled("SELECT Name, Value FROM [TestTable]");

        // Act
        var dataTable = await work.GetDataTableAsync(sql);

        // Assert
        Assert.NotNull(dataTable);
        Assert.Equal(1, dataTable.Rows.Count);
        Assert.Equal("Test1", dataTable.Rows[0]["Name"]);
        Assert.Equal(100L, dataTable.Rows[0]["Value"]);
    }

    #endregion

    #region ExecuteReader Extension Tests

    [Fact]
    public async Task ExecuteReader_ShouldAllowReadingData()
    {
        // Arrange
        await CreateTestTableAsync();
        await InsertTestRecordAsync("Test1", 100);
        await InsertTestRecordAsync("Test2", 200);
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable] ORDER BY Name");

        var results = new List<(string Name, long Value)>();

        // Act
        work.ExecuteReader(sql, reader =>
        {
            while (reader.Read())
            {
                results.Add((reader.GetString(0), reader.GetInt64(1)));
            }
        });

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Test1", results[0].Name);
        Assert.Equal(100, results[0].Value);
        Assert.Equal("Test2", results[1].Name);
        Assert.Equal(200, results[1].Value);
    }

    [Fact]
    public async Task ExecuteReaderAsync_ShouldAllowReadingData()
    {
        // Arrange
        await CreateTestTableAsync();
        await InsertTestRecordAsync("Test1", 100);
        await InsertTestRecordAsync("Test2", 200);
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable] ORDER BY Name");

        var results = new List<(string Name, long Value)>();

        // Act
        await work.ExecuteReaderAsync(sql, async reader =>
        {
            while (await reader.ReadAsync())
            {
                results.Add((reader.GetString(0), reader.GetInt64(1)));
            }
        });

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Test1", results[0].Name);
        Assert.Equal(100, results[0].Value);
        Assert.Equal("Test2", results[1].Name);
        Assert.Equal(200, results[1].Value);
    }

    #endregion

    #region Any Extension Tests

    [Fact]
    public async Task Any_WithData_ShouldReturnTrue()
    {
        // Arrange
        await CreateTestTableAsync();
        await InsertTestRecordAsync("Test1", 100);
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT 1 FROM [TestTable] WHERE Name = @name").Set("name", "Test1");

        // Act
        var result = work.Any(sql);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Any_WithNoData_ShouldReturnFalse()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT 1 FROM [TestTable] WHERE Name = @name").Set("name", "NonExistent");

        // Act
        var result = work.Any(sql);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AnyAsync_WithData_ShouldReturnTrue()
    {
        // Arrange
        await CreateTestTableAsync();
        await InsertTestRecordAsync("Test1", 100);
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT 1 FROM [TestTable] WHERE Name = @name").Set("name", "Test1");

        // Act
        var result = await work.AnyAsync(sql);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AnyAsync_WithNoData_ShouldReturnFalse()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT 1 FROM [TestTable] WHERE Name = @name").Set("name", "NonExistent");

        // Act
        var result = await work.AnyAsync(sql);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region FirstOrDefault Extension Tests

    [Fact]
    public async Task FirstOrDefault_WithData_ShouldReturnMappedObject()
    {
        // Arrange
        await CreateTestTableAsync();
        await InsertTestRecordAsync("Test1", 100);
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable] WHERE Name = @name").Set("name", "Test1");
        var mapper = new TestRecordMapper();

        // Act
        var result = work.FirstOrDefault(sql, mapper);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test1", result!.Name);
        Assert.Equal(100, result.Value);
    }

    [Fact]
    public async Task FirstOrDefault_WithNoData_ShouldReturnNull()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable] WHERE Name = @name").Set("name", "NonExistent");
        var mapper = new TestRecordMapper();

        // Act
        var result = work.FirstOrDefault(sql, mapper);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithData_ShouldReturnMappedObject()
    {
        // Arrange
        await CreateTestTableAsync();
        await InsertTestRecordAsync("Test1", 100);
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable] WHERE Name = @name").Set("name", "Test1");
        var mapper = new TestRecordMapper();

        // Act
        var result = await work.FirstOrDefaultAsync(sql, mapper);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test1", result!.Name);
        Assert.Equal(100, result.Value);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithNoData_ShouldReturnNull()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable] WHERE Name = @name").Set("name", "NonExistent");
        var mapper = new TestRecordMapper();

        // Act
        var result = await work.FirstOrDefaultAsync(sql, mapper);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region List Extension Tests

    [Fact]
    public async Task List_WithData_ShouldReturnMappedList()
    {
        // Arrange
        await CreateTestTableAsync();
        await InsertTestRecordAsync("Test1", 100);
        await InsertTestRecordAsync("Test2", 200);
        await InsertTestRecordAsync("Test3", 300);
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable] ORDER BY Name");
        var mapper = new TestRecordMapper();

        // Act
        var result = work.List(sql, mapper);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Test1", result[0].Name);
        Assert.Equal(100, result[0].Value);
        Assert.Equal("Test2", result[1].Name);
        Assert.Equal(200, result[1].Value);
        Assert.Equal("Test3", result[2].Name);
        Assert.Equal(300, result[2].Value);
    }

    [Fact]
    public async Task List_WithNoData_ShouldReturnEmptyList()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable]");
        var mapper = new TestRecordMapper();

        // Act
        var result = work.List(sql, mapper);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListAsync_WithData_ShouldReturnMappedList()
    {
        // Arrange
        await CreateTestTableAsync();
        await InsertTestRecordAsync("Test1", 100);
        await InsertTestRecordAsync("Test2", 200);
        await InsertTestRecordAsync("Test3", 300);
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable] ORDER BY Name");
        var mapper = new TestRecordMapper();

        // Act
        var result = await work.ListAsync(sql, mapper);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Test1", result[0].Name);
        Assert.Equal(100, result[0].Value);
        Assert.Equal("Test2", result[1].Name);
        Assert.Equal(200, result[1].Value);
        Assert.Equal("Test3", result[2].Name);
        Assert.Equal(300, result[2].Value);
    }

    [Fact]
    public async Task ListAsync_WithNoData_ShouldReturnEmptyList()
    {
        // Arrange
        await CreateTestTableAsync();
        using var work = _manager.CreateWork("Default");
        var sql = new Sqled("SELECT Name, Value FROM [TestTable]");
        var mapper = new TestRecordMapper();

        // Act
        var result = await work.ListAsync(sql, mapper);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region Helper Methods

    private async Task CreateTestTableAsync()
    {
        var provider = _providerFactory.GetProvider(SqliteConnectionDefine.DATABASE_TYPE)!;
        using var connection = provider.GetDbConnection(_connectionDescriptor.ConnectionString);
        await connection.OpenAsync();
        using var command = provider.GetDbCommand(connection);
        command.CommandText = "CREATE TABLE IF NOT EXISTS [TestTable] (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Value INTEGER)";
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    private async Task InsertTestRecordAsync(string name, long value)
    {
        var provider = _providerFactory.GetProvider(SqliteConnectionDefine.DATABASE_TYPE)!;
        using var connection = provider.GetDbConnection(_connectionDescriptor.ConnectionString);
        await connection.OpenAsync();
        using var command = provider.GetDbCommand(connection);
        command.CommandText = "INSERT INTO [TestTable] (Name, Value) VALUES (@name, @value)";
        provider.SetParameters(command, new[] { new KeyValuePair<string, object>("name", name), new KeyValuePair<string, object>("value", value) });
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    private async Task<int> GetRecordCountAsync(string tableName)
    {
        var provider = _providerFactory.GetProvider(SqliteConnectionDefine.DATABASE_TYPE)!;
        using var connection = provider.GetDbConnection(_connectionDescriptor.ConnectionString);
        await connection.OpenAsync();
        using var command = provider.GetDbCommand(connection);
        command.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
        var result = await command.ExecuteScalarAsync();
        await connection.CloseAsync();
        return Convert.ToInt32(result);
    }

    #endregion

    #region Test Classes

    private class TestDbFilter : IDbFilter
    {
        public string Name { get; }
        public Func<Sqled, Sqled>? SqlLoadingTransform { get; set; }
        public Func<DbCommand?, DbCommand?>? CommandCreatingCallback { get; set; }
        public Func<DbCommand, DbCommand>? CommandExecutingCallback { get; set; }

        public TestDbFilter(string name)
        {
            Name = name;
        }

        public Sqled SqlLoading(Sqled sqled)
        {
            return SqlLoadingTransform?.Invoke(sqled) ?? sqled;
        }

#if NETSTANDARD2_0
        public DbCommand CommandCreating(DbCommand command)
#else
        public DbCommand? CommandCreating(DbCommand? command)
#endif
        {
            return CommandCreatingCallback?.Invoke(command);
        }

        public DbCommand CommandExecuting(DbCommand command)
        {
            return CommandExecutingCallback?.Invoke(command) ?? command;
        }
    }

    private class TestRecord
    {
        public string Name { get; set; } = string.Empty;
        public long Value { get; set; }
    }

    private class TestRecordMapper : IDbMapper<TestRecord>
    {
        public TestRecord Map(DbDataReader reader)
        {
            return new TestRecord
            {
                Name = reader.GetString(0),
                Value = reader.GetInt64(1)
            };
        }
    }

    #endregion

    #region IDisposable Implementation

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && File.Exists(_testDbPath))
            {
                try
                {
                    File.Delete(_testDbPath);
                }
                catch
                {
                    // Ignore file deletion errors
                }
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}