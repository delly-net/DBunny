using Delly.DBunny.Connecting;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Core;
using Delly.DBunny.Core.Providing.Extension;
using Delly.DBunny.PostgreSql;
using Delly.DBunny.Providing;
using System.Data.Common;

namespace UnitTest.PostgreSql;

public class PostgreSqlTestFixture : IDisposable
{
    public IDbProvider Provider { get; }
    public DbConnectionDescriptor ConnectionDescriptor { get; }
    public DbConnection Connection => _connection!;
    private DbConnection? _connection;
    private readonly string _testDatabaseName = $"testdb_{Guid.NewGuid():N}";
    private readonly string _originalDatabase = "postgres";

    public PostgreSqlTestFixture()
    {
        // 使用 PostgreSQL 连接字符串（需要配置实际的连接信息）
        // 默认使用 Testcontainers 或本地 PostgreSQL 实例
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "192.168.56.103";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "123456";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "postgres";

        // 使用 PostgreSqlConnectionDefine 定义连接
        var connectionDefine = new PostgreSqlConnectionDefine()
            .WithHost(host)
            .WithPort(int.Parse(port))
            .WithDatabase(database)
            .WithUsername(username)
            .WithPassword(password)
            .WithPooling(false)
            .WithSslMode("Disable");

        // 创建连接描述器
        ConnectionDescriptor = connectionDefine.GetDbConnectionDescriptor(PostgreSqlConnectionDefine.DATABASE_TYPE, "Default");
        // 创建连接工厂
        var connectionFactory = new DefaultDbConnectionFactory(ConnectionDescriptor);
        // 创建提供程序工厂
        var providerFactory = new DefaultDbProviderFactory(new PostgreSqlProvider());

        // 通过 Provider 获取连接
        Provider = providerFactory.GetProvider(connectionFactory.GetDefaultConnection().DatabaseType)!;
        _connection = Provider.GetDbConnection(ConnectionDescriptor.ConnectionString);
        _connection.Open();

        // 创建测试数据库
        CreateTestDatabase();
    }

    private void CreateTestDatabase()
    {
        // 先删除已存在的测试数据库
        var dropDbSql = $"DROP DATABASE IF EXISTS \"{_testDatabaseName}\";";
        using var dropCommand = Provider.GetDbCommand(_connection!);
        dropCommand.CommandText = dropDbSql;
        try { dropCommand.ExecuteNonQuery(); } catch { }

        var createDbSql = $"CREATE DATABASE \"{_testDatabaseName}\";";
        using var command = Provider.GetDbCommand(_connection!);
        command.CommandText = createDbSql;
        command.ExecuteNonQuery();

        // 关闭当前连接，切换到新数据库
        _connection!.Close();

        // 更新连接字符串使用新数据库
        var newConnectionString = ConnectionDescriptor.ConnectionString.Replace(
            $"Database={_originalDatabase}",
            $"Database={_testDatabaseName}"
        );

        var newConnection = Provider.GetDbConnection(newConnectionString);
        newConnection.Open();

        // 替换连接引用
        _connection.Dispose();
        _connection = newConnection;
    }

    public void Dispose()
    {
        try
        {
            _connection?.Close();
            _connection?.Dispose();

            // 删除测试数据库
            var adminConnectionString = ConnectionDescriptor.ConnectionString.Replace(
                $"Database={_testDatabaseName}",
                $"Database={_originalDatabase}"
            );

            using var adminConnection = Provider.GetDbConnection(adminConnectionString);
            adminConnection.Open();

            var dropDbSql = $"DROP DATABASE IF EXISTS \"{_testDatabaseName}\";";
            using var command = Provider.GetDbCommand(adminConnection);
            command.CommandText = dropDbSql;
            try { command.ExecuteNonQuery(); } catch { }

            adminConnection.Close();
            adminConnection.Dispose();
        }
        catch
        {
            // 忽略清理错误
        }
    }
}