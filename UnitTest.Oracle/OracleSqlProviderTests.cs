using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
using Delly.DBunny.Oracle;
using Delly.Modeling;
using Xunit;

namespace UnitTest.Oracle;

public class OracleSqlProviderTests
{
    private readonly OracleSqlProvider _provider = new();

    [Fact]
    public void GetParamterSqled_ShouldCreateParameterWithCorrectPrefix()
    {
        // Act
        var result = _provider.GetParamterSqled("id", 123);

        // Assert
        Assert.Contains(":id", result.Sql);
        Assert.NotNull(result.Parameters);
        Assert.Contains(result.Parameters, p => p.Key == "id" && p.Value.Equals(123));
    }

    [Fact]
    public void GetParamterSqled_WithStringValue_ShouldCreateCorrectParameter()
    {
        // Act
        var result = _provider.GetParamterSqled("name", "TestValue");

        // Assert
        Assert.Contains(":name", result.Sql);
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
        Assert.Contains(":createdAt", result.Sql);
        Assert.Contains(result.Parameters, p => p.Key == "createdAt" && p.Value.Equals(dateTime));
    }

    [Fact]
    public void GetTimeParamterSqled_WithNullValue_ShouldHandleGracefully()
    {
        // Act
        var result = _provider.GetTimeParamterSqled("date", null!);

        // Assert
        Assert.Contains(":date", result.Sql);
    }

    [Fact]
    public void AppendOffset_WithTakeOnly_ShouldAddOffsetFetch()
    {
        // Arrange
        var sql = "SELECT * FROM Users".ToSql();

        // Act
        var result = _provider.AppendOffset(sql, 10, null);

        // Assert
        Assert.Contains("OFFSET 0 ROWS", result.Sql);
        Assert.Contains("FETCH NEXT 10 ROWS ONLY", result.Sql);
    }

    [Fact]
    public void AppendOffset_WithTakeAndSkip_ShouldAddOffsetFetchWithCorrectValues()
    {
        // Arrange
        var sql = "SELECT * FROM Users".ToSql();

        // Act
        var result = _provider.AppendOffset(sql, 10, 20);

        // Assert
        Assert.Contains("OFFSET 20 ROWS", result.Sql);
        Assert.Contains("FETCH NEXT 10 ROWS ONLY", result.Sql);
    }

    [Fact]
    public void AppendOffset_WithNullTake_ShouldNotAddOffsetFetch()
    {
        // Arrange
        var sql = "SELECT * FROM Users".ToSql();

        // Act
        var result = _provider.AppendOffset(sql, null, 20);

        // Assert
        Assert.DoesNotContain("OFFSET", result.Sql);
        Assert.DoesNotContain("FETCH", result.Sql);
        Assert.Equal("SELECT * FROM Users", result.Sql);
    }

    [Fact]
    public void GetSpecialTypeName_WithAutoIncrementFlag_ShouldIgnoreFlag()
    {
        // Act & Assert - Oracle uses SEQUENCE for auto-increment, not column types
        var intType = _provider.GetSpecialTypeName(ColumnType.INTEGER, TypeCode.Int32, false);
        var intTypeAuto = _provider.GetSpecialTypeName(ColumnType.INTEGER, TypeCode.Int32, true);

        Assert.Equal("NUMBER(10)", intType);
        Assert.Equal("NUMBER(10)", intTypeAuto);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_VarcharWithLength_ShouldIncludeLength()
    {
        // Act
        var varchar50 = _provider.GetSpecialTypeName(TypeCode.String, 50);
        var varchar100 = _provider.GetSpecialTypeName(TypeCode.String, 100);
        var varchar5000 = _provider.GetSpecialTypeName(TypeCode.String, 5000);

        // Assert
        Assert.Equal("VARCHAR2(50)", varchar50);
        Assert.Equal("VARCHAR2(100)", varchar100);
        Assert.Equal("CLOB", varchar5000);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_VarcharWithoutLength_ShouldUseDefault()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.String);

        // Assert
        Assert.Equal("VARCHAR2(4000)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_DecimalWithPrecision_ShouldIncludePrecision()
    {
        // Act
        var result1 = _provider.GetSpecialTypeName(TypeCode.Decimal, 18, 4);
        var result2 = _provider.GetSpecialTypeName(TypeCode.Decimal, 10, 2);
        var result3 = _provider.GetSpecialTypeName(TypeCode.Decimal, 0, 5);

        // Assert
        Assert.Equal("NUMBER(18,4)", result1);
        Assert.Equal("NUMBER(10,2)", result2);
        Assert.Equal("NUMBER(18,5)", result3);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_DecimalWithLengthOnly_ShouldUseLengthOnly()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Decimal, 10);

        // Assert
        Assert.Equal("NUMBER(10)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_DecimalWithoutParams_ShouldUseDefaults()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Decimal);

        // Assert
        Assert.Equal("NUMBER(18,4)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Boolean_ShouldReturnNumber1()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Boolean);

        // Assert
        Assert.Equal("NUMBER(1)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Single_ShouldReturnBinaryFloat()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Single);

        // Assert
        Assert.Equal("BINARY_FLOAT", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Double_ShouldReturnBinaryDouble()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Double);

        // Assert
        Assert.Equal("BINARY_DOUBLE", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Byte_ShouldReturnNumber3()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Byte);

        // Assert
        Assert.Equal("NUMBER(3)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Int16_ShouldReturnNumber5()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Int16);

        // Assert
        Assert.Equal("NUMBER(5)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Int32_ShouldReturnNumber10()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Int32);

        // Assert
        Assert.Equal("NUMBER(10)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Int64_ShouldReturnNumber19()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Int64);

        // Assert
        Assert.Equal("NUMBER(19)", result);
    }

    [Fact]
    public void GetDatabases_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.GetDatabases());
    }

    [Fact]
    public void CreateDatabase_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.CreateDatabase("testdb", null!));
    }

    [Fact]
    public void DropDatabase_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.DropDatabase("testdb"));
    }

    [Fact]
    public void GetSchemas_ShouldReturnSelectFromAllUsers()
    {
        // Act
        var result = _provider.GetSchemas();

        // Assert
        Assert.Contains("SELECT username FROM all_users", result.Sql);
        Assert.Contains("WHERE username NOT IN", result.Sql);
    }

    [Fact]
    public void GetSchemas_WithName_ShouldReturnSelectWithWhere()
    {
        // Act
        var result = _provider.GetSchemas("myschema");

        // Assert
        Assert.Contains("SELECT username FROM all_users", result.Sql);
        Assert.Contains("WHERE username = 'myschema'", result.Sql);
    }

    [Fact]
    public void CreateSchema_WithoutOptions_ShouldUseDefaults()
    {
        // Act
        var result = _provider.CreateSchema("testschema", null!);

        // Assert
        Assert.Contains("CREATE USER", result.Sql);
        Assert.Contains("\"testschema\"", result.Sql);
        Assert.Contains("IDENTIFIED BY", result.Sql);
        Assert.Contains("DEFAULT TABLESPACE USERS", result.Sql);
        Assert.Contains("TEMPORARY TABLESPACE TEMP", result.Sql);
    }

    [Fact]
    public void CreateSchema_WithOptions_ShouldIncludeOptions()
    {
        // Arrange
        var options = new Dictionary<string, object>
        {
            { "password", "mypassword" },
            { "tablespace", "MYTABLESPACE" },
            { "temp_tablespace", "MYTEMP" }
        };

        // Act
        var result = _provider.CreateSchema("testschema", options);

        // Assert
        Assert.Contains("IDENTIFIED BY \"mypassword\"", result.Sql);
        Assert.Contains("DEFAULT TABLESPACE MYTABLESPACE", result.Sql);
        Assert.Contains("TEMPORARY TABLESPACE MYTEMP", result.Sql);
    }

    [Fact]
    public void DropSchema_ShouldReturnCorrectSql()
    {
        // Act
        var result = _provider.DropSchema("testschema");

        // Assert
        Assert.Contains("DROP USER", result.Sql);
        Assert.Contains("\"testschema\"", result.Sql);
        Assert.Contains("CASCADE", result.Sql);
    }

    [Fact]
    public void HasDatabase_ShouldReturnFalse()
    {
        // Assert
        Assert.False(_provider.HasDatabase);
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
    public void GetTables_ShouldReturnSelectFromAllTables()
    {
        // Act
        var result = _provider.GetTables("myschema");

        // Assert
        Assert.Contains("SELECT table_name FROM all_tables", result.Sql);
        Assert.Contains("WHERE owner = 'myschema'", result.Sql);
    }

    [Fact]
    public void GetTable_ShouldReturnSelectFromAllTables()
    {
        // Arrange
        var tableDesciptor = new DbTableDesciptor { SchemaName = "myschema", TableName = "MyTable" };

        // Act
        var result = _provider.GetTable(tableDesciptor);

        // Assert
        Assert.Contains("SELECT table_name FROM all_tables", result.Sql);
        Assert.Contains("WHERE owner = 'myschema'", result.Sql);
        Assert.Contains("AND table_name = 'MyTable'", result.Sql);
    }

    [Fact]
    public void CreateTableColumnDefine_PrimaryKey_ShouldIncludePrimaryKey()
    {
        // Arrange
        var columnDesciptor = new DbColumnDesciptor
        {
            ColumnName = "Id",
            ColumnType = "NUMBER(10)",
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
    public void DropTable_ShouldIncludePurge()
    {
        // Arrange
        var tableDesciptor = new DbTableDesciptor { SchemaName = "myschema", TableName = "MyTable" };

        // Act
        var result = _provider.DropTable(tableDesciptor);

        // Assert
        Assert.Contains("DROP TABLE", result.Sql);
        Assert.Contains("PURGE", result.Sql);
    }

    [Fact]
    public void CreateIndex_ShouldCreateIndexWithQuotedName()
    {
        // Arrange
        var indexDesciptor = new DbIndexDesciptor
        {
            SchemaName = "myschema",
            TableName = "MyTable",
            IndexName = "MyIndex",
            ColumnName = "MyColumn",
            UniqueFlag = true
        };

        // Act
        var result = _provider.CreateIndex(indexDesciptor);

        // Assert
        Assert.Contains("CREATE UNIQUE INDEX", result.Sql);
        Assert.Contains("\"MyTable_MyColumn_IDX\"", result.Sql);
    }

    [Fact]
    public void DropIndex_ShouldDropIndexWithQuotedName()
    {
        // Arrange
        var indexDesciptor = new DbIndexDesciptor
        {
            SchemaName = "myschema",
            TableName = "MyTable",
            IndexName = "MyIndex",
            ColumnName = "MyColumn"
        };

        // Act
        var result = _provider.DropIndex(indexDesciptor);

        // Assert
        Assert.Contains("DROP INDEX", result.Sql);
        Assert.Contains("\"MyTable_MyColumn_IDX\"", result.Sql);
    }
}