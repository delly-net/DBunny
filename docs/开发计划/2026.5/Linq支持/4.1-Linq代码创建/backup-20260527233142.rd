# 4.1 Linq代码创建

## 任务信息

- **任务号**: 4.1
- **任务名称**: Linq代码创建
- **责任人**: 郑琳
- **任务类型**: 功能开发
- **任务描述**: 在 `Delly.DBunny.Linq` 子项目中实现 `EntityQueryProvider` 和 `SqlQueryable` 类及其依赖对象

## 任务目标

在 DBunny 项目中实现 LINQ 支持，允许用户使用 LINQ 语法进行数据库查询操作，同时保持与现有 DBunny API 的兼容性，并支持 AOT 编译。

## 实现要求

### 项目配置

1. 更新 `Delly.DBunny.Linq.csproj` 的 TargetFrameworks 为 `netstandard2.0;net5.0;net8.0`
2. 添加项目引用到 `Delly.DBunny.Core`
3. 为 .NET 5.0+ 目标框架启用可空引用类型
4. 添加 AOT 支持：
```xml
<IsAotCompatible>true</IsAotCompatible>
<PublishAot>true</PublishAot>
```

### 目录结构

```
Delly.DBunny.Linq/
├── Dependency/           # 依赖接口
│   ├── IExpressionProvider.cs
│   ├── IQueryExpressionRelay.cs
│   ├── IExpressionCompiler.cs
│   └── IEntityModel.cs
├── EntityModeling/       # 实体建模
│   ├── IEntityModelFactory.cs
│   └── DefaultEntityModelFactory.cs
├── Query/                # 查询相关
│   ├── EntityQueryProvider.cs
│   ├── EntityQueryable.cs
│   ├── Relay/
│   │   ├── QueryExpressionRelay.cs
│   │   └── QueryExpressionRelayBuilder.cs
│   ├── Expression/
│   │   ├── QueryRootExpression.cs
│   │   ├── QueryWhereExpression.cs
│   │   ├── QuerySelectExpression.cs
│   │   ├── QueryOrderByExpression.cs
│   │   └── QueryOrderByDescendingExpression.cs
│   └── Extension/
│       └── QueryProviderExtension.cs
├── Compiler/             # 表达式编译器
│   ├── CompilerFactory.cs
│   ├── StatementBuilder.cs
│   ├── VariableBuilder.cs
│   └── ExpressionCompiler.cs
└── Extension/            # 扩展方法
    └── QueryProviderExtension.cs
```

### 核心接口

#### 1. IExpressionProvider

```csharp
namespace Delly.DBunny.Linq.Dependency
{
    /// <summary>
    /// 表达式提供者接口
    /// </summary>
    public interface IExpressionProvider
    {
        /// <summary>
        /// 获取数据库特定名称
        /// </summary>
        /// <param name="name">原始名称</param>
        /// <returns>数据库特定名称</returns>
        string GetName(string name);

        /// <summary>
        /// 获取参数名前缀
        /// </summary>
        string ParameterPrefix { get; }
    }
}
```

#### 2. IQueryExpressionRelay

```csharp
namespace Delly.DBunny.Linq.Dependency
{
    /// <summary>
    /// 表达式中继接口
    /// </summary>
    public interface IQueryExpressionRelay
    {
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="entityModelFactory">实体模型工厂</param>
        /// <param name="provider">表达式提供者</param>
        /// <param name="expression">表达式</param>
        /// <returns>表达式中继</returns>
        static IQueryExpressionRelay Create(
            IEntityModelFactory entityModelFactory,
            IExpressionProvider provider,
            System.Linq.Expressions.Expression expression)
        {
            var relay = new QueryExpressionRelay(entityModelFactory, provider, expression);
            relay.Analyse();
            return relay;
        }

        /// <summary>
        /// 设置变量构建器
        /// </summary>
        /// <param name="variable">变量构建器</param>
        void SetVariable(VariableBuilder variable);

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns>查询中继</returns>
        IQueryRelay Build();
    }
}
```

#### 3. IEntityModel

```csharp
namespace Delly.DBunny.Linq.Dependency
{
    /// <summary>
    /// 实体模型接口
    /// </summary>
    public interface IEntityModel
    {
        /// <summary>
        /// 实体类型
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// 表名
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// 获取列名
        /// </summary>
        /// <param name="propertyInfo">属性信息</param>
        /// <returns>列名</returns>
        string GetColumnName(System.Reflection.PropertyInfo propertyInfo);
    }
}
```

#### 4. IEntityModelFactory

```csharp
namespace Delly.DBunny.Linq.Dependency
{
    /// <summary>
    /// 实体模型工厂接口
    /// </summary>
    public interface IEntityModelFactory
    {
        /// <summary>
        /// 获取模型
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <returns>实体模型</returns>
#if NETSTANDARD2_0
        IEntityModel GetModel(Type entityType);
#else
        IEntityModel? GetModel(Type entityType);
#endif

        /// <summary>
        /// 获取模型
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>实体模型</returns>
        IEntityModel GetModel<T>();
    }
}
```

### 核心类实现

#### 1. VariableBuilder

```csharp
namespace Delly.DBunny.Linq.Compiler
{
    /// <summary>
    /// 变量构建器，用于生成 SQL 参数
    /// </summary>
    public sealed class VariableBuilder
    {
        private readonly Dictionary<string, object> _parameters;
        private int _parameterIndex;

        /// <summary>
        /// 变量构建器
        /// </summary>
        public VariableBuilder()
        {
#if NET8_0
            _parameters = [];
#else
            _parameters = new Dictionary<string, object>();
#endif
            _parameterIndex = 0;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="value">参数值</param>
        /// <returns>参数名</returns>
        public string AddParameter(object value)
        {
            var paramName = $"p{_parameterIndex++}";
            _parameters[paramName] = value;
            return paramName;
        }

        /// <summary>
        /// 获取所有参数
        /// </summary>
        /// <returns>参数集合</returns>
        public IReadOnlyDictionary<string, object> GetParameters() => _parameters;
    }
}
```

#### 2. Expression 类体系

```csharp
namespace Delly.DBunny.Linq.Query.Expression
{
    /// <summary>
    /// 查询根表达式
    /// </summary>
    internal class QueryRootExpression : System.Linq.Expressions.Expression
    {
        private readonly IEntityModel _model;

        /// <summary>
        /// 查询根表达式
        /// </summary>
        /// <param name="model">实体模型</param>
        public QueryRootExpression(IEntityModel model)
        {
            _model = model;
        }

        /// <summary>
        /// 实体模型
        /// </summary>
        public IEntityModel Model => _model;

        /// <summary>
        /// 表达式类型
        /// </summary>
        public override Type Type => typeof(IEnumerable);

        /// <summary>
        /// 节点类型
        /// </summary>
        public override System.Linq.Expressions.ExpressionType NodeType =>
            System.Linq.Expressions.ExpressionType.Extension;

        // TODO: 实现其他必需方法
    }

    /// <summary>
    /// Where 表达式
    /// </summary>
    internal class QueryWhereExpression : System.Linq.Expressions.Expression
    {
        private readonly IEntityModel _model;
        private readonly System.Linq.Expressions.Expression _source;
        private readonly System.Linq.Expressions.Expression _predicate;

        /// <summary>
        /// Where 表达式
        /// </summary>
        public QueryWhereExpression(
            Type type,
            IEntityModel model,
            System.Linq.Expressions.Expression source,
            System.Linq.Expressions.Expression predicate)
        {
            Type = type;
            _model = model;
            _source = source;
            _predicate = predicate;
        }

        /// <summary>
        /// 实体模型
        /// </summary>
        public IEntityModel Model => _model;

        /// <summary>
        /// 源表达式
        /// </summary>
        public System.Linq.Expressions.Expression Source => _source;

        /// <summary>
        /// 谓词表达式
        /// </summary>
        public System.Linq.Expressions.Expression Predicate => _predicate;

        /// <summary>
        /// 表达式类型
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public override System.Linq.Expressions.ExpressionType NodeType =>
            System.Linq.Expressions.ExpressionType.Extension;
    }
}
```

#### 3. EntityQueryable<T>

```csharp
namespace Delly.DBunny.Linq.Query
{
    /// <summary>
    /// 实体可查询对象
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
#if NET8_0
    public class EntityQueryable<[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)] T> : IOrderedQueryable<T>
#else
    public class EntityQueryable<T> : IOrderedQueryable<T>
#endif
    {
        /// <summary>
        /// 元素类型
        /// </summary>
        public Type ElementType => typeof(T);

        /// <summary>
        /// 表达式
        /// </summary>
        public System.Linq.Expressions.Expression Expression { get; }

        /// <summary>
        /// 查询提供者
        /// </summary>
        public IQueryProvider Provider { get; }

        /// <summary>
        /// 实体可查询对象
        /// </summary>
        /// <param name="provider">查询提供者</param>
        /// <param name="expression">表达式</param>
        public EntityQueryable(EntityQueryProvider provider, System.Linq.Expressions.Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        /// <summary>
        /// 实体可查询对象
        /// </summary>
        /// <param name="provider">查询提供者</param>
        /// <param name="entityModelFactory">实体模型工厂</param>
        public EntityQueryable(
            EntityQueryProvider provider,
            IEntityModelFactory entityModelFactory)
        {
            Provider = provider;
            var model = entityModelFactory.GetModel<T>();
            Expression = new QueryRootExpression(model);
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var list = Provider.Execute<IEnumerable<T>>(Expression);
            return list?.GetEnumerator() ?? Enumerable.Empty<T>().GetEnumerator();
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
```

#### 4. EntityQueryProvider

```csharp
namespace Delly.DBunny.Linq.Query
{
    /// <summary>
    /// 实体查询提供者
    /// </summary>
    public sealed class EntityQueryProvider : IQueryProvider
    {
        private readonly IDbProvider _dbProvider;
        private readonly IDbConnectionDefine _connection;
        private readonly IEntityModelFactory _entityModelFactory;
        private readonly IExpressionProvider _expressionProvider;
        private static readonly Type _enumerableType = typeof(IEnumerable<>);
        private static readonly Type _queryableType = typeof(Queryable);

        /// <summary>
        /// 实体查询提供者
        /// </summary>
        /// <param name="dbProvider">数据库提供者</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="entityModelFactory">实体模型工厂</param>
        /// <param name="expressionProvider">表达式提供者</param>
        public EntityQueryProvider(
            IDbProvider dbProvider,
            IDbConnectionDefine connection,
            IEntityModelFactory entityModelFactory,
            IExpressionProvider expressionProvider)
        {
            _dbProvider = dbProvider;
            _connection = connection;
            _entityModelFactory = entityModelFactory;
            _expressionProvider = expressionProvider;
        }

        /// <summary>
        /// 创建查询
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>可查询对象</returns>
        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            // TODO: 实现动态类型查询
            throw new NotImplementedException();
        }

#if NET8_0
        /// <summary>
        /// 创建查询
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>可查询对象</returns>
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
        public IQueryable<TElement> CreateQuery<
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)] TElement>(
            System.Linq.Expressions.Expression expression)
#else
        /// <summary>
        /// 创建查询
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>可查询对象</returns>
        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
#endif
        {
            // TODO: 处理表达式，注入自定义表达式类型
            return new EntityQueryable<TElement>(this, expression);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>执行结果</returns>
        public object Execute(System.Linq.Expressions.Expression expression)
        {
            // TODO: 实现非泛型执行
            throw new NotImplementedException();
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>执行结果</returns>
#if NET8_0
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
        public TResult Execute<
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
                System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)] TResult>(
            System.Linq.Expressions.Expression expression)
#else
        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
#endif
        {
            // TODO: 使用 IQueryExpressionRelay 构建并执行 SQL
            var variableBuilder = new VariableBuilder();
            var relay = IQueryExpressionRelay.Create(_entityModelFactory, _expressionProvider, expression);
            relay.SetVariable(variableBuilder);
            var queryRelay = relay.Build();
            var (sql, parameters) = queryRelay.Build();

            // 执行查询并返回结果
            var sqled = new Sqled(sql, parameters);
            var connectionString = _connection.ConnectionString;
            var connection = _dbProvider.GetDbConnection(connectionString);

            return ExecuteQuery<TResult>(connection, sqled);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sqled">SQL 命令</param>
        /// <returns>执行结果</returns>
        private TResult ExecuteQuery<TResult>(System.Data.Common.DbConnection connection, Sqled sqled)
        {
            // TODO: 实现查询执行逻辑
            throw new NotImplementedException();
        }
    }
}
```

#### 5. QueryExpressionRelay

```csharp
namespace Delly.DBunny.Linq.Query.Relay
{
    /// <summary>
    /// 查询表达式中继
    /// </summary>
    internal sealed class QueryExpressionRelay : IQueryExpressionRelay
    {
        private readonly IEntityModelFactory _entityModelFactory;
        private readonly IExpressionProvider _expressionProvider;
        private readonly System.Linq.Expressions.Expression _expression;
        private VariableBuilder _variableBuilder;
        private IQueryRelay _relay;

        /// <summary>
        /// 查询表达式中继
        /// </summary>
        public QueryExpressionRelay(
            IEntityModelFactory entityModelFactory,
            IExpressionProvider expressionProvider,
            System.Linq.Expressions.Expression expression)
        {
            _entityModelFactory = entityModelFactory;
            _expressionProvider = expressionProvider;
            _expression = expression;
        }

        /// <summary>
        /// 分析
        /// </summary>
        public void Analyse()
        {
            // TODO: 分析表达式树，构建查询中继
        }

        /// <summary>
        /// 设置变量构建器
        /// </summary>
        /// <param name="variable">变量构建器</param>
        public void SetVariable(VariableBuilder variable)
        {
            _variableBuilder = variable;
        }

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns>查询中继</returns>
        public IQueryRelay Build() => _relay;
    }
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
var dbProvider = factory.GetProvider("SQLITE");

// 创建实体模型工厂
var entityModelFactory = new DefaultEntityModelFactory();

// 创建表达式提供者
var expressionProvider = new DefaultExpressionProvider(dbProvider.SqlProvider);

// 使用 LINQ 查询
var queryProvider = new EntityQueryProvider(
    dbProvider,
    connection,
    entityModelFactory,
    expressionProvider);

var users = new EntityQueryable<User>(queryProvider, entityModelFactory)
    .Where(u => u.Age > 18)
    .OrderBy(u => u.Name)
    .ToList();
```

### AOT 兼容说明

1. **DynamicallyAccessedMembers**：标记所有需要动态访问的类型
2. **RequiresUnreferencedCode**：标记使用反射的代码
3. **UnconditionalSuppressMessage**：抑制 AOT 警告
4. **IsAotCompatible**：在项目文件中启用 AOT 支持

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
- AOT 兼容标记

## 参考文件

- [IDbProvider.cs](../../../../../Delly.DBunny.Core/IDbProvider.cs)
- [Sqled.cs](../../../../../Delly.DBunny.Core/Sqled.cs)
- [DbProviderExtension.cs](../../../../../Delly.DBunny.Core/Providing/Extension/DbProviderExtension.cs)
- [代码规范.md](../../../开发指南/代码规范.md)