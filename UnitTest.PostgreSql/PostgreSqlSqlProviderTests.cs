using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
using Delly.DBunny.PostgreSql;
using Delly.Modeling;
using Xunit;

namespace UnitTest.PostgreSql;

public class PostgreSqlSqlProviderTests
{
    private readonly PostgreSqlSqlProvider _provider = new();

    [Fact]
    public void GetParamterSqled_ShouldCreateParameterWithCorrectPrefix()
    {
        // Act
        var result = _provider.GetParamterSqled("id", 123);

        // Assert
        Assert.Contains("@id", result.Sql);
        Assert.NotNull(result.Parameters);
        Assert.Contains(result.Parameters, p => p.Key == "id" && p.Value.Equals(123));
    }

    [Fact]
    public void GetParamterSqled_WithStringValue_ShouldCreateCorrectParameter()
    {
        // Act
        var result = _provider.GetParamterSqled("name", "TestValue");

        // Assert
        Assert.Contains("@name", result.Sql);
        Assert.Contains(result.Parameters, p => p.Key == "name" && p.Value.Equals("TestValue"));
    }

    [Fact]
    public void GetTimeParamterSqled_WithDateTime_ShouldPassThrough()
    {
        // Arrange
        var dateTime = new DateTime(2026, 5, 19, 14, 30, 45, 123);

        // Act
        var result = _provider.GetTimeParamterSqled("createdAt", dateTime);

        // Assert
        Assert.Contains("@createdAt", result.Sql);
        Assert.Contains(result.Parameters, p => p.Key == "createdAt" && p.Value.Equals(dateTime));
    }

    [Fact]
    public void GetTimeParamterSqled_WithNullValue_ShouldHandleGracefully()
    {
        // Act
        var result = _provider.GetTimeParamterSqled("date", null!);

        // Assert
        Assert.Contains("@date", result.Sql);
    }

    [Fact]
    public void AppendOffset_WithOnlyTake_ShouldAddLimit()
    {
        // Arrange
        var sql = "SELECT * FROM Users".ToSql();

        // Act
        var result = _provider.AppendOffset(sql, 10, null);

        // Assert
        Assert.Contains("LIMIT 10", result.Sql);
        Assert.DoesNotContain("OFFSET", result.Sql);
    }

    [Fact]
    public void AppendOffset_WithTakeAndSkip_ShouldAddLimitAndOffset()
    {
        // Arrange
        var sql = "SELECT * FROM Users".ToSql();

        // Act
        var result = _provider.AppendOffset(sql, 10, 20);

        // Assert
        Assert.Contains("LIMIT 10 OFFSET 20", result.Sql);
    }

    [Fact]
    public void AppendOffset_WithNullTake_ShouldReturnOriginalSql()
    {
        // Arrange
        var sql = "SELECT * FROM Users".ToSql();

        // Act
        var result = _provider.AppendOffset(sql, null, 20);

        // Assert
        Assert.DoesNotContain("LIMIT", result.Sql);
        Assert.DoesNotContain("OFFSET", result.Sql);
        Assert.Equal("SELECT * FROM Users", result.Sql);
    }

    [Fact]
    public void AppendOffset_WithZeroSkip_ShouldNotAddOffset()
    {
        // Arrange
        var sql = "SELECT * FROM Users".ToSql();

        // Act
        var result = _provider.AppendOffset(sql, 10, 0);

        // Assert
        Assert.Contains("LIMIT 10", result.Sql);
        Assert.DoesNotContain("OFFSET", result.Sql);
    }

    [Fact]
    public void GetSpecialTypeName_WithAutoIncrementFlag_Integer32_ShouldReturnSerial()
    {
        // Act
        var result = _provider.GetSpecialTypeName(ColumnType.INTEGER, TypeCode.Int32, true);

        // Assert
        Assert.Equal("SERIAL", result);
    }

    [Fact]
    public void GetSpecialTypeName_WithAutoIncrementFlag_Integer64_ShouldReturnBigSerial()
    {
        // Act
        var result = _provider.GetSpecialTypeName(ColumnType.LONG, TypeCode.Int64, true);

        // Assert
        Assert.Equal("BIGSERIAL", result);
    }

    [Fact]
    public void GetSpecialTypeName_WithAutoIncrementFlag_Integer16_ShouldReturnSmallSerial()
    {
        // Act
        var result = _provider.GetSpecialTypeName(ColumnType.BOOL, TypeCode.Int16, true);

        // Assert
        Assert.Equal("SMALLSERIAL", result);
    }

    [Fact]
    public void GetSpecialTypeName_WithoutAutoIncrementFlag_ShouldReturnRegularType()
    {
        // Act
        var result = _provider.GetSpecialTypeName(ColumnType.INTEGER, TypeCode.Int32, false);

        // Assert
        Assert.Equal("INTEGER", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_VarcharWithLength_ShouldIncludeLength()
    {
        // Act
        var varchar50 = _provider.GetSpecialTypeName(TypeCode.String, 50);
        var varchar100 = _provider.GetSpecialTypeName(TypeCode.String, 100);
        var varchar300 = _provider.GetSpecialTypeName(TypeCode.String, 300);

        // Assert
        Assert.Equal("VARCHAR(50)", varchar50);
        Assert.Equal("VARCHAR(100)", varchar100);
        Assert.Equal("TEXT", varchar300);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_VarcharWithoutLength_ShouldUseDefault()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.String);

        // Assert
        Assert.Equal("VARCHAR(255)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_DecimalWithPrecision_ShouldIncludePrecision()
    {
        // Act
        var result1 = _provider.GetSpecialTypeName(TypeCode.Decimal, 18, 4);
        var result2 = _provider.GetSpecialTypeName(TypeCode.Decimal, 10, 2);
        var result3 = _provider.GetSpecialTypeName(TypeCode.Decimal, 0, 5);

        // Assert
        Assert.Equal("NUMERIC(18,4)", result1);
        Assert.Equal("NUMERIC(10,2)", result2);
        Assert.Equal("NUMERIC(18,5)", result3);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_DecimalWithLengthOnly_ShouldUseDefaultPrecision()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Decimal, 10);

        // Assert
        Assert.Equal("NUMERIC(10,4)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_DecimalWithoutParams_ShouldUseDefaults()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Decimal);

        // Assert
        Assert.Equal("NUMERIC(18,4)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Boolean_ShouldReturnBoolean()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Boolean);

        // Assert
        Assert.Equal("BOOLEAN", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Double_ShouldReturnDoublePrecision()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Double);

        // Assert
        Assert.Equal("DOUBLE PRECISION", result);
    }

    [Fact]
    public void GetDatabases_ShouldReturnSelectFromPgDatabase()
    {
        // Act
        var result = _provider.GetDatabases();

        // Assert
        Assert.Contains("SELECT datname FROM pg_database", result.Sql);
        Assert.Contains("WHERE datistemplate = false", result.Sql);
    }

    [Fact]
    public void CreateDatabase_WithoutOptions_ShouldReturnBasicCreate()
    {
        // Act
        var result = _provider.CreateDatabase("testdb", null!);

        // Assert
        Assert.Contains("CREATE DATABASE", result.Sql);
        Assert.Contains("\"testdb\"", result.Sql);
    }

    [Fact]
    public void CreateDatabase_WithOptions_ShouldIncludeOptions()
    {
        // Arrange
        var options = new Dictionary<string, object>
        {
            { "owner", "testuser" },
            { "encoding", "UTF8" },
            { "template", "template0" }
        };

        // Act
        var result = _provider.CreateDatabase("testdb", options);

        // Assert
        Assert.Contains("OWNER \"testuser\"", result.Sql);
        Assert.Contains("ENCODING 'UTF8'", result.Sql);
        Assert.Contains("TEMPLATE \"template0\"", result.Sql);
    }

    [Fact]
    public void DropDatabase_ShouldReturnCorrectSql()
    {
        // Act
        var result = _provider.DropDatabase("testdb");

        // Assert
        Assert.Equal("DROP DATABASE IF EXISTS \"testdb\";", result.Sql);
    }

    [Fact]
    public void GetSchemas_ShouldReturnSelectFromInformationSchema()
    {
        // Act
        var result = _provider.GetSchemas();

        // Assert
        Assert.Contains("SELECT schema_name FROM information_schema.schemata", result.Sql);
        Assert.Contains("WHERE schema_name NOT IN ('pg_catalog', 'information_schema')", result.Sql);
    }

    [Fact]
    public void GetSchemas_WithName_ShouldReturnSelectWithWhere()
    {
        // Act
        var result = _provider.GetSchemas("myschema");

        // Assert
        Assert.Contains("SELECT schema_name FROM information_schema.schemata", result.Sql);
        Assert.Contains("WHERE schema_name = 'myschema'", result.Sql);
    }

    [Fact]
    public void CreateSchema_WithoutOptions_ShouldReturnBasicCreate()
    {
        // Act
        var result = _provider.CreateSchema("testschema", null!);

        // Assert
        Assert.Contains("CREATE SCHEMA", result.Sql);
        Assert.Contains("\"testschema\"", result.Sql);
    }

    [Fact]
    public void CreateSchema_WithOptions_ShouldIncludeAuthorization()
    {
        // Arrange
        var options = new Dictionary<string, object>
        {
            { "authorization", "testuser" }
        };

        // Act
        var result = _provider.CreateSchema("testschema", options);

        // Assert
        Assert.Contains("AUTHORIZATION \"testuser\"", result.Sql);
    }

    [Fact]
    public void DropSchema_ShouldReturnCorrectSql()
    {
        // Act
        var result = _provider.DropSchema("testschema");

        // Assert
        Assert.Equal("DROP SCHEMA IF EXISTS \"testschema\" CASCADE;", result.Sql);
    }

    [Fact]
    public void HasDatabase_ShouldReturnTrue()
    {
        // Assert
        Assert.True(_provider.HasDatabase);
    }

    [Fact]
    public void HasSchema_ShouldReturnTrue()
    {
        // Assert
        Assert.True(_provider.HasSchema);
    }

    [Fact]
    public void GetSpecialName_ShouldWrapWithDoubleQuotes()
    {
        // Act
        var result = _provider.GetSpecialName("MyTable");

        // Assert
        Assert.Equal("\"MyTable\"", result);
    }

    [Fact]
    public void GetSpecialName_WithSpecialCharacters_ShouldWrapCorrectly()
    {
        // Act
        var result = _provider.GetSpecialName("Table-With-Special");

        // Assert
        Assert.Equal("\"Table-With-Special\"", result);
    }

    [Fact]
    public void GetTables_ShouldReturnSelectFromInformationSchema()
    {
        // Act
        var result = _provider.GetTables("myschema");

        // Assert
        Assert.Contains("SELECT table_name FROM information_schema.tables", result.Sql);
        Assert.Contains("WHERE table_schema = 'myschema'", result.Sql);
        Assert.Contains("AND table_type = 'BASE TABLE'", result.Sql);
    }

    [Fact]
    public void GetTable_ShouldReturnSelectFromInformationSchema()
    {
        // Arrange
        var tableDesciptor = new DbTableDesciptor { SchemaName = "myschema", TableName = "MyTable" };

        // Act
        var result = _provider.GetTable(tableDesciptor);

        // Assert
        Assert.Contains("SELECT table_name FROM information_schema.tables", result.Sql);
        Assert.Contains("WHERE table_schema = 'myschema'", result.Sql);
        Assert.Contains("AND table_name = 'MyTable'", result.Sql);
    }

    [Fact]
    public void CreateTableColumnDefine_PrimaryKey_ShouldIncludePrimaryKey()
    {
        // Arrange
        var columnDesciptor = new DbColumnDesciptor
        {
            ColumnName = "Id",
            ColumnType = "INTEGER",
            PrimaryKeyFlag = true,
            NullableFlag = false
        };

        // Act
        var result = _provider.CreateTableColumnDefine(columnDesciptor);

        // Assert
        Assert.Contains("PRIMARY KEY", result.Sql);
        Assert.Contains("NOT NULL", result.Sql);
    }

    [Fact]
    public void DropTable_ShouldIncludeCascade()
    {
        // Arrange
        var tableDesciptor = new DbTableDesciptor { SchemaName = "myschema", TableName = "MyTable" };

        // Act
        var result = _provider.DropTable(tableDesciptor);

        // Assert
        Assert.Contains("DROP TABLE IF EXISTS", result.Sql);
        Assert.Contains("CASCADE", result.Sql);
    }
}