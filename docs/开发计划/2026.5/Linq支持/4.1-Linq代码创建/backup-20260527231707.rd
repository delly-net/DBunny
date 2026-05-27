# 4.1 Linq代码创建

## 任务信息

- **任务号**: 4.1
- **任务名称**: Linq代码创建
- **责任人**: 郑琳
- **任务类型**: 功能开发
- **任务描述**: 在 `Delly.DBunny.Linq` 子项目中实现 `EntityQueryProvider` 和 `SqlQueryable` 类及其依赖对象

## 任务目标

在 DBunny 项目中实现 LINQ 支持，允许用户使用 LINQ 语法进行数据库查询操作，同时保持与现有 DBunny API 的兼容性。

## 实现要求

### 项目配置

1. 更新 `Delly.DBunny.Linq.csproj` 的 TargetFrameworks 为 `netstandard2.0;net5.0;net8.0`
2. 添加项目引用到 `Delly.DBunny.Core`
3. 为 .NET 5.0+ 目标框架启用可空引用类型

### 核心类实现

#### 1. SqlQueryable<T>

实现可查询的 LINQ 数据源，继承 `IQueryable<T>`：

```csharp
public class SqlQueryable<T> : IOrderedQueryable<T>
{
    public Type ElementType { get; }
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }

    public SqlQueryable(IQueryProvider provider)
    {
        Provider = provider;
        ElementType = typeof(T);
        Expression = Expression.Constant(this);
    }

    public SqlQueryable(IQueryProvider provider, Expression expression)
    {
        Provider = provider;
        Expression = expression;
        ElementType = typeof(T);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Provider.Execute<IEnumerable>(Expression).GetEnumerator();
    }
}
```

#### 2. EntityQueryProvider

实现 LINQ 查询提供者，负责将 LINQ 表达式转换为 SQL：

```csharp
public class EntityQueryProvider : IQueryProvider
{
    private readonly IDbProvider _dbProvider;
    private readonly IDbConnectionDefine _connection;

    public EntityQueryProvider(IDbProvider dbProvider, IDbConnectionDefine connection)
    {
        _dbProvider = dbProvider;
        _connection = connection;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        // TODO: 实现动态类型查询
        throw new NotImplementedException();
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new SqlQueryable<TElement>(this, expression);
    }

    public object Execute(Expression expression)
    {
        // TODO: 实现非泛型执行
        throw new NotImplementedException();
    }

    public TResult Execute<TResult>(Expression expression)
    {
        // TODO: 将表达式转换为 SQL 并执行
        throw new NotImplementedException();
    }
}
```

#### 3. ExpressionVisitor

实现表达式树访问器，将 LINQ 表达式转换为 SQL：

```csharp
internal class DbExpressionVisitor : ExpressionVisitor
{
    private readonly StringBuilder _sqlBuilder;
    private readonly Dictionary<string, object> _parameters;
    private readonly ISqlProvider _sqlProvider;

    public DbExpressionVisitor(ISqlProvider sqlProvider)
    {
        _sqlBuilder = new StringBuilder();
        _parameters = new Dictionary<string, object>();
        _sqlProvider = sqlProvider;
    }

    public (string Sql, Dictionary<string, object> Parameters) GetResult()
    {
        return (_sqlBuilder.ToString(), _parameters);
    }

    // TODO: 实现 VisitMethodCall 方法处理 Where, Select, OrderBy 等
    // TODO: 实现 VisitBinary 方法处理比较运算符
    // TODO: 实现 VisitMember 方法处理属性访问
}
```

### LINQ 操作支持

需要支持的 LINQ 操作：
- `Where()` - 条件过滤
- `Select()` - 投影
- `OrderBy()` / `OrderByDescending()` - 排序
- `ThenBy()` / `ThenByDescending()` - 多级排序
- `Skip()` / `Take()` - 分页
- `First()` / `FirstOrDefault()` - 单条查询
- `ToList()` - 列表查询

### 使用示例

```csharp
// 创建连接定义
var connection = new SqliteConnectionDefine()
    .WithDataSource("mydb.db");

// 获取提供者
var factory = new DefaultDbProviderFactory();
factory.Append(new SqliteProvider());
var provider = factory.GetProvider("SQLITE");

// 使用 LINQ 查询
var queryProvider = new EntityQueryProvider(provider, connection);
var users = new SqlQueryable<User>(queryProvider)
    .Where(u => u.Age > 18)
    .OrderBy(u => u.Name)
    .ToList();
```

## TODO 确认项

- [ ] 是否需要支持分组操作 `GroupBy()`
- [ ] 是否需要支持聚合函数 `Count()`, `Sum()`, `Avg()`, `Max()`, `Min()`
- [ ] 是否需要支持关联查询 `Join()`, `GroupJoin()`
- [ ] 是否需要支持异步操作 `ToListAsync()`, `FirstOrDefaultAsync()`
- [ ] 是否需要支持批量操作 `InsertRange()`, `UpdateRange()`, `DeleteRange()`
- [ ] 表名映射策略约定
- [ ] 是否需要实体映射配置
- [ ] 是否需要支持复杂类型映射

## 代码规范

遵循 [代码规范](../../../开发指南/代码规范.md)：
- 所有公共函数必须有完整的 XML 文档注释
- 常量使用 UPPER_CASE 命名
- 接口使用 I 前缀
- 支持多目标框架：netstandard2.0, net5.0, net8.0
- .NET 5.0+ 启用可空引用类型

## 参考文件

- [IDbProvider.cs](../../../../../Delly.DBunny.Core/IDbProvider.cs)
- [Sqled.cs](../../../../../Delly.DBunny.Core/Sqled.cs)
- [DbProviderExtension.cs](../../../../../Delly.DBunny.Core/Providing/Extension/DbProviderExtension.cs)
- [代码规范.md](../../../开发指南/代码规范.md)