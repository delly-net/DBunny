using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
using Delly.DBunny.Sqlite;
using Xunit;

namespace UnitTest.Sqlite;

public class SqliteSqlProviderTests
{
    private readonly SqliteSqlProvider _provider = new();

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
    public void GetTimeParamterSqled_WithDateTime_ShouldFormatCorrectly()
    {
        // Arrange
        var dateTime = new DateTime(2026, 5, 19, 14, 30, 45, 123);

        // Act
        var result = _provider.GetTimeParamterSqled("createdAt", dateTime);

        // Assert
        Assert.Contains("@createdAt", result.Sql);
        var paramValue = result.Parameters.FirstOrDefault(p => p.Key == "createdAt").Value as string;
        Assert.Equal("2026-05-19 14:30:45.123", paramValue);
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
    public void GetSpecialTypeName_WithAutoIncrementFlag_ShouldIgnoreFlag()
    {
        // Act & Assert - SQLite doesn't use auto-increment in type name
        var intType = _provider.GetSpecialTypeName(DbColumnType.INTEGER, TypeCode.Int32, false);
        var intTypeAuto = _provider.GetSpecialTypeName(DbColumnType.INTEGER, TypeCode.Int32, true);

        Assert.Equal("INTEGER", intType);
        Assert.Equal("INTEGER", intTypeAuto);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_VarcharWithLength_ShouldIncludeLength()
    {
        // Act
        var varchar50 = _provider.GetSpecialTypeName(TypeCode.String, 50);
        var varchar100 = _provider.GetSpecialTypeName(TypeCode.String, 100);

        // Assert
        Assert.Equal("TEXT(50)", varchar50);
        Assert.Equal("TEXT(100)", varchar100);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_VarcharWithoutLength_ShouldUseDefault()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.String);

        // Assert
        Assert.Equal("TEXT", result);
    }

    [Fact]
    public void GetSpecialTypeName_TypeCode_DecimalWithPrecision_ShouldIncludePrecision()
    {
        // Act
        var result = _provider.GetSpecialTypeName(TypeCode.Decimal, 18, 4);

        // Assert - SQLite doesn't support precision for REAL
        Assert.Equal("REAL", result);
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
    public void HasDatabase_ShouldReturnFalse()
    {
        // Assert
        Assert.False(_provider.HasDatabase);
    }

    [Fact]
    public void HasSchema_ShouldReturnFalse()
    {
        // Assert
        Assert.False(_provider.HasSchema);
    }

    [Fact]
    public void GetSpecialName_ShouldWrapWithSquareBrackets()
    {
        // Act
        var result = _provider.GetSpecialName("MyTable");

        // Assert
        Assert.Equal("[MyTable]", result);
    }

    [Fact]
    public void GetSpecialName_WithSpecialCharacters_ShouldWrapCorrectly()
    {
        // Act
        var result = _provider.GetSpecialName("Table With Spaces");

        // Assert
        Assert.Equal("[Table With Spaces]", result);
    }
}