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

## 包

| 包名 | 描述 | 平台 | NuGet |
|------|------|----------|-------|
| [Delly.DBunny.Core](Delly.DBunny.Core/) | 核心库，包含接口和基础类型 | 跨平台 | - |
| [Delly.DBunny.Sqlite](Delly.DBunny.Sqlite/) | SQLite 提供者实现 | 跨平台 | - |
| [Delly.DBunny.MySql](Delly.DBunny.MySql/) | MySQL 提供者实现 | 跨平台 | - |
| [Delly.DBunny.PostgreSql](Delly.DBunny.PostgreSql/) | PostgreSQL 提供者实现 | 跨平台 | - |
| [Delly.DBunny.Oracle](Delly.DBunny.Oracle/) | Oracle 提供者实现 | 跨平台 | - |
| [Delly.DBunny.SqlServer](Delly.DBunny.SqlServer/) | SQL Server 提供者实现 | 跨平台 | - |
| [Delly.DBunny.MsAccess](Delly.DBunny.MsAccess/) | Microsoft Access 提供者 (.accdb & .mdb) | 仅 Windows | - |

## 安装

### 核心包

```bash
dotnet add package Delly.DBunny.Core
```

### SQLite 提供者

```bash
dotnet add package Delly.DBunny.Sqlite
```

### MySQL 提供者

```bash
dotnet add package Delly.DBunny.MySql
```

### PostgreSQL 提供者

```bash
dotnet add package Delly.DBunny.PostgreSql
```

### Oracle 提供者

```bash
dotnet add package Delly.DBunny.Oracle
```

### SQL Server 提供者

```bash
dotnet add package Delly.DBunny.SqlServer
```

### Microsoft Access 提供者

> **仅 Windows:** Access 提供者仅支持 Windows 平台。

```bash
dotnet add package Delly.DBunny.MsAccess
```

## 快速开始

### SQLite 示例

```csharp
using Delly.DBunny;
using Delly.DBunny.Sqlite;
using Delly.DBunny.Sql.Extension;
using System.Data.Common;

// 直接创建 SQLite 提供者
var provider = new SqliteProvider();
var connectionString = "Data Source=mydb.db;Pooling=False";

using var connection = provider.GetDbConnection(connectionString);
connection.Open();

// 创建表
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "TEXT(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true }
};
var createTableSql = provider.SqlProvider.CreateTable(string.Empty, "Users", columnDescriptors);

using var createCommand = provider.GetDbCommand(connection);
createCommand.CommandText = createTableSql.Sql;
await createCommand.ExecuteNonQueryAsync();

// 执行查询
var sql = new Sqled("SELECT * FROM Users WHERE Age > @minAge")
    .Set("minAge", 18);

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

### SQL Server 示例

```csharp
using Delly.DBunny;
using Delly.DBunny.SqlServer;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

// 使用构建器定义连接
var connectionDefine = new SqlServerConnectionDefine()
    .WithServer("localhost")
    .WithDatabase("mydb")
    .WithUserId("sa")
    .WithPassword("your_password")
    .WithTrustServerCertificate(true);

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    SqlServerConnectionDefine.DATABASE_TYPE, "Default");

var provider = new SqlServerProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// 在 dbo schema 中创建表
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INT", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "NVARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INT", PrimaryKeyFlag = false, NullableFlag = true }
};
var createTableSql = provider.SqlProvider.CreateTable("dbo", "Users", columnDescriptors);

using var createCommand = provider.GetDbCommand(connection);
createCommand.CommandText = createTableSql.Sql;
await createCommand.ExecuteNonQueryAsync();

// ... 与 SQLite 相同的查询操作
```

### MySQL 示例

```csharp
using Delly.DBunny;
using Delly.DBunny.MySql;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

var connectionDefine = new MySqlConnectionDefine()
    .WithServer("localhost")
    .WithPort(3306)
    .WithDatabase("mydb")
    .WithUserId("root")
    .WithPassword("password")
    .WithCharset("utf8mb4");

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    MySqlConnectionDefine.DATABASE_TYPE, "Default");

var provider = new MySqlProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// ... 与 SQLite 相同的查询操作（使用反引号引用名称）
```

### PostgreSQL 示例

```csharp
using Delly.DBunny;
using Delly.DBunny.PostgreSql;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

var connectionDefine = new PostgreSqlConnectionDefine()
    .WithHost("localhost")
    .WithPort(5432)
    .WithDatabase("mydb")
    .WithUsername("postgres")
    .WithPassword("password");

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    PostgreSqlConnectionDefine.DATABASE_TYPE, "Default");

var provider = new PostgreSqlProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// ... 与 SQLite 相同的查询操作（使用双引号引用名称）
```

### Oracle 示例

```csharp
using Delly.DBunny;
using Delly.DBunny.Oracle;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

var connectionDefine = new OracleConnectionDefine()
    .WithDataSource("localhost:1521/ORCL")
    .WithUserId("system")
    .WithPassword("password");

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    OracleConnectionDefine.DATABASE_TYPE, "Default");

var provider = new OracleProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// ... 与 SQLite 相同的查询操作（注意：Oracle 使用 : 参数前缀）
```

### Microsoft Access 示例

> **注意:** Access 提供者仅支持 Windows，因为它依赖 Microsoft Access Database Engine (OLE DB)。

```csharp
using Delly.DBunny;
using Delly.DBunny.MsAccess;
using Delly.DBunny.Sql.Extension;
using Delly.DBunny.Connecting.Extension;
using System.Data.Common;

// 对于现代 .accdb 文件
var connectionDefine = new MsAccessConnectionDefine()
    .WithDataSource("mydb.accdb")
    .WithProviderAce()
    .WithPooling(false)
    .WithReadWrite();

// 对于旧版 .mdb 文件（仅 32 位）
var legacyConnectionDefine = new MsAccessConnectionDefine()
    .WithDataSource("mydb.mdb")
    .WithProviderJet()
    .WithPooling(false);

var connectionDescriptor = connectionDefine.GetDbConnectionDescriptor(
    MsAccessConnectionDefine.DATABASE_TYPE, "Default");

var provider = new MsAccessProvider();
using var connection = provider.GetDbConnection(connectionDescriptor.ConnectionString);
connection.Open();

// 注意：Access 使用位置参数 (?)
var insertSql = new Sqled("INSERT INTO [Users] (Name, Age) VALUES (?, ?)")
    .Set("name", "John Doe")
    .Set("age", 30);

// ... 与 SQLite 相同的查询操作
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

- 数据库操作（创建、获取、删除）
- 架构操作（创建、获取、删除）
- 表操作（创建、获取、删除）
- 列操作（创建、重命名、修改、复制、删除）
- 索引操作（创建、获取、删除）
- .NET 类型与数据库类型之间的类型转换

```csharp
// 使用 SQL 提供者创建表
var columnDescriptors = new List<DbColumnDesciptor>
{
    new DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Name", ColumnType = "TEXT(100)", PrimaryKeyFlag = false, NullableFlag = false },
    new DbColumnDesciptor { ColumnName = "Age", ColumnType = "INTEGER", PrimaryKeyFlag = false, NullableFlag = true }
};

var createTableSql = provider.SqlProvider.CreateTable(string.Empty, "Users", columnDescriptors);

using var command = provider.GetDbCommand(connection);
command.CommandText = createTableSql.Sql;
await command.ExecuteNonQueryAsync();
```

## 数据库对比

| 特性 | SQLite | MySQL | PostgreSQL | Oracle | SQL Server | MsAccess |
|------|--------|-------|------------|--------|------------|----------|
| 平台 | 跨平台 | 跨平台 | 跨平台 | 跨平台 | 跨平台 | 仅 Windows |
| 支持数据库 | 否 | 是 | 是 | 否 | 是 | 否 |
| 支持架构 | 否 | 否 | 是 | 是 | 是 | 否 |
| 名称引用 | `[name]` | `` `name` `` | `"name"` | `"name"` | `[name]` | `[name]` |
| 参数前缀 | `@` | `@` | `@` | `:` | `@` | `?` (位置) |
| 主键 | NOT NULL PRIMARY KEY | AUTO_INCREMENT PRIMARY KEY | NOT NULL PRIMARY KEY | NOT NULL PRIMARY KEY | IDENTITY(1,1) PRIMARY KEY | AUTOINCREMENT |
| 十进制类型 | REAL | DECIMAL | NUMERIC | NUMBER | DECIMAL | CURRENCY |
| 日期时间类型 | TEXT | DATETIME | TIMESTAMP | TIMESTAMP | DATETIME | DATETIME |
| 大文本 | TEXT | TEXT | TEXT | CLOB | NVARCHAR(MAX) | LONGTEXT |

## 相关链接

- [文档](#)
- [问题反馈](https://github.com/delly-net/DBunny/issues)
- [发布版本](https://github.com/delly-net/DBunny/releases)

## 许可证

[MIT 许可证](LICENSE)

## 贡献

欢迎贡献！请随时提交 Pull Request。

## 作者

© 2025 [delly.net](https://delly.net)