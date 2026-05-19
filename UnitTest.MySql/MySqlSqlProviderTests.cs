using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
using Delly.DBunny.MySql;
using Xunit;

namespace UnitTest.MySql;

public class MySqlSqlProviderTests
{
    private readonly MySqlSqlProvider _provider = new();

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
        Assert.Contains("LIMIT 20, 10", result.Sql);
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
    public void GetSpecialTypeName_WithAutoIncrementFlag_ShouldIgnoreFlag()
    {
        // Act & Assert - MySQL handles AUTO_INCREMENT in column definition, not type
        var intType = _provider.GetSpecialTypeName(DbColumnType.INTEGER, TypeCode.Int32, false);
        var intTypeAuto = _provider.GetSpecialTypeName(DbColumnType.INTEGER, TypeCode.Int32, true);

        Assert.Equal("INT", intType);
        Assert.Equal("INT", intTypeAuto);
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
        Assert.Equal("DECIMAL(18,4)", result1);
        Assert.Equal("DECIMAL(10,2)", result2);
        Assert.Equal("DECIMAL(18,5)", result3);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_DecimalWithLengthOnly_ShouldUseDefaultPrecision()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Decimal, 10);

        // Assert
        Assert.Equal("DECIMAL(10,4)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_DecimalWithoutParams_ShouldUseDefaults()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Decimal);

        // Assert
        Assert.Equal("DECIMAL(18,4)", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_Boolean_ShouldReturnTinyInt()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Boolean);

        // Assert
        Assert.Equal("TINYINT(1)", result);
    }

    [Fact]
    public void GetDatabases_ShouldReturnShowDatabases()
    {
        // Act
        var result = _provider.GetDatabases();

        // Assert
        Assert.Equal("SHOW DATABASES", result.Sql);
    }

    [Fact]
    public void CreateDatabase_WithoutOptions_ShouldIncludeDefaultCharacterSet()
    {
        // Act
        var result = _provider.CreateDatabase("testdb", null!);

        // Assert
        Assert.Contains("CREATE DATABASE IF NOT EXISTS", result.Sql);
        Assert.Contains("`testdb`", result.Sql);
        Assert.Contains("CHARACTER SET utf8mb4", result.Sql);
        Assert.Contains("COLLATE utf8mb4_unicode_ci", result.Sql);
    }

    [Fact]
    public void CreateDatabase_WithOptions_ShouldIncludeOptions()
    {
        // Arrange
        var options = new Dictionary<string, object>
        {
            { "character_set", "utf8" },
            { "collation", "utf8_general_ci" }
        };

        // Act
        var result = _provider.CreateDatabase("testdb", options);

        // Assert
        Assert.Contains("CHARACTER SET utf8", result.Sql);
        Assert.Contains("COLLATE utf8_general_ci", result.Sql);
    }

    [Fact]
    public void DropDatabase_ShouldReturnCorrectSql()
    {
        // Act
        var result = _provider.DropDatabase("testdb");

        // Assert
        Assert.Equal("DROP DATABASE IF EXISTS `testdb`;", result.Sql);
    }

    [Fact]
    public void GetSchemas_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.GetSchemas());
    }

    [Fact]
    public void GetSchemas_WithName_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.GetSchemas("testschema"));
    }

    [Fact]
    public void CreateSchema_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.CreateSchema("testschema", null!));
    }

    [Fact]
    public void DropSchema_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _provider.DropSchema("testschema"));
    }

    [Fact]
    public void HasDatabase_ShouldReturnTrue()
    {
        // Assert
        Assert.True(_provider.HasDatabase);
    }

    [Fact]
    public void HasSchema_ShouldReturnFalse()
    {
        // Assert
        Assert.False(_provider.HasSchema);
    }

    [Fact]
    public void GetSpecialName_ShouldWrapWithBackticks()
    {
        // Act
        var result = _provider.GetSpecialName("MyTable");

        // Assert
        Assert.Equal("`MyTable`", result);
    }

    [Fact]
    public void GetSpecialName_WithSpecialCharacters_ShouldWrapCorrectly()
    {
        // Act
        var result = _provider.GetSpecialName("Table-With-Special");

        // Assert
        Assert.Equal("`Table-With-Special`", result);
    }

    [Fact]
    public void GetTables_ShouldReturnShowTablesFromSchema()
    {
        // Act
        var result = _provider.GetTables("mydb");

        // Assert
        Assert.Equal("SHOW TABLES FROM `mydb`", result.Sql);
    }
}