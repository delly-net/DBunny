using Delly.DBunny.MsAccess;

var provider = new MsAccessSqlProvider();
var columns = new List<Delly.DBunny.DbColumnDesciptor>
{
    new Delly.DBunny.DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
    new Delly.DBunny.DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false }
};
var sql = provider.CreateTable(string.Empty, "Users", columns);
Console.WriteLine("SQL: " + sql.Sql);

// Also test CreateTableColumnDefine
var col1 = provider.CreateTableColumnDefine("Id", "INTEGER", true, false);
Console.WriteLine("Col1: " + col1.Sql);

var col2 = provider.CreateTableColumnDefine("Name", "VARCHAR(100)", false, false);
Console.WriteLine("Col2: " + col2.Sql);