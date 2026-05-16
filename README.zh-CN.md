# DBunny

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard2.0%20%7C%20net5.0%20%7C%20net8.0-purple.svg)](https://dotnet.microsoft.com/)
[![AOT Compatible](https://img.shields.io/badge/AOT-Compatible-success.svg)](https://learn.microsoft.com/zh-cn/dotnet/core/deploying/native-aot/)

一个轻量级的 .NET 数据库抽象层，提供统一的 API 来处理多种数据库类型。DBunny 使用提供者模式来实现数据库特定的实现，同时保持通用接口。

## 特性

- **多数据库支持**：编写与数据库无关的代码，根据需要切换提供者
- **轻量级**：最小开销，简洁清晰的 API
- **提供者模式**：易于扩展新的数据库提供者
- **异步支持**：完整的 async/await 数据库操作支持
- **多目标框架**：支持 .NET Standard 2.0、.NET 5.0 和 .NET 8.0
- **AOT 兼容**：支持 .NET 8.0 的 Native AOT 编译
- **可空引用类型**：.NET 5.0 及更高版本已启用

## 安装

### 核心包

```bash
dotnet add package Delly.DBunny
```

### SQLite 提供者

```bash
dotnet add package Delly.DBunny.Sqlite
```

## 快速开始

```csharp
using Delly.DBunny;
using Delly.DBunny.Connecting.Extension;
using Delly.DBunny.Providing;
using Delly.DBunny.Providing.Extension;
using Delly.DBunny.Sqlite;
using Delly.DBunny.Sql.Extension;
using System.Data.Common;

// 定义连接设置
var connectionDefine = new SqliteConnectionDefine()
    .WithDataSource("Data Source=mydb.db")
    .WithPooling(false)
    .WithForeignKeys(true);

// 创建连接描述器
var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    SqliteConnectionDefine.DATABASE_TYPE, "Default");

// 创建连接工厂
var connectionFactory = new DefaultDbConnectionFactory(connectionDescriptor);

// 创建提供者工厂，使用 SQLite 提供者
var providerFactory = new DefaultDbProviderFactory(new SqliteProvider());

// 获取提供者和连接
var provider = providerFactory.GetProvider(
    connectionFactory.GetDefaultConnection().DatabaseType)!;

using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// 执行查询
var sql = new Sqled("SELECT * FROM Users WHERE Name = @name")
    .Set("name", "John");

using var command = provider.GetDbCommand(connection);
command.CommandText = sql.Sql;
provider.SetParameters(command, sql.Parameters);

// 读取数据
await provider.ReadAsync(connection, sql, async reader =>
{
    while (await reader.ReadAsync())
    {
        var id = reader["Id"];
        var name = reader["Name"];
        Console.WriteLine($"Id: {id}, Name: {name}");
    }
});
```

## 核心概念

### Sqled

SQL 命令的包装类，结合了 SQL 文本和参数：

```csharp
var sql = new Sqled("SELECT * FROM Users WHERE Age > @minAge")
    .Set("minAge", 18);

// 流式 API 构建查询
sql.Append(" AND Status = @status")
    .Set("status", "Active");

// 对于复杂 SQL，直接使用 Builder
var createTableSql = new Sqled();
createTableSql.Builder.AppendLine("CREATE TABLE [Users](");
createTableSql.Builder.Append("    [Id] INTEGER NOT NULL PRIMARY KEY,");
createTableSql.Builder.Append("    [Name] TEXT(100) NOT NULL,");
createTableSql.Builder.AppendLine("    [Age] INTEGER NULL");
createTableSql.Builder.AppendLine(");");
```

### 核心接口

#### IDbProvider

主要的数据库提供者接口，用于：

- 创建数据库连接和命令
- 设置参数
- 执行查询（返回 DataSet）
- 同步或异步读取数据
- 获取数据库元数据（架构、表、列、索引）

```csharp
// 执行非查询操作（INSERT、UPDATE、DELETE）
var insertSql = new Sqled("INSERT INTO [Users] (Name, Age) VALUES (@name, @age)")
    .Set("name", "John Doe")
    .Set("age", 30);

using var command = provider.GetDbCommand(connection);
command.CommandText = insertSql.Sql;
provider.SetParameters(command, insertSql.Parameters);
await command.ExecuteNonQueryAsync();
```

#### ISqlProvider

生成数据库特定的 SQL 语句，用于：

- 架构操作（创建、获取）
- 表操作（创建、获取）
- 列操作（创建、重命名、复制、删除）
- 索引操作（创建、获取）
- .NET 类型与数据库类型之间的类型转换

```csharp
// 使用 SQL 提供者创建表
var columnDefines = new List<Sqled>
{
    provider.SqlProvider.ColumnDefine("Id", "INTEGER", true, false),
    provider.SqlProvider.ColumnDefine("Name", "TEXT(100)", false, false),
    provider.SqlProvider.ColumnDefine("Age", "INTEGER", false, true)
};

var createTableSql = provider.SqlProvider.CreateTable(
    string.Empty, "Users", columnDefines);

await ExecuteNonQueryAsync(connection, createTableSql);
```

## 支持的数据库

| 数据库 | 提供者 | 状态 |
|--------|--------|------|
| [SQLite](https://www.sqlite.org/) | `Delly.DBunny.Sqlite` | ✅ 稳定 |
| [MySQL](https://www.mysql.com/) | 即将推出 | 计划中 |
| [PostgreSQL](https://www.postgresql.org/) | 即将推出 | 计划中 |
| [SQL Server](https://www.microsoft.com/sql-server/) | 即将推出 | 计划中 |

## 相关链接

- [文档](#) *[待更新]*
- [问题反馈](https://github.com/delly-net/DBunny/issues)
- [发布版本](https://github.com/delly-net/DBunny/releases)

## 许可证

[MIT 许可证](LICENSE)

## 贡献

欢迎贡献！请随时提交 Pull Request。

## 作者

© 2025 [delly.net](https://delly.net)