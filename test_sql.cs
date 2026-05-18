using System;
using Delly.DBunny.MsAccess;

var provider = new MsAccessSqlProvider();
var columns = new List<Delly.DBunny.DbColumnDesciptor>
{
    new Delly.DBunny.DbColumnDesciptor { ColumnName = "Id", ColumnType = "INTEGER", PrimaryKeyFlag = true, NullableFlag = false },
    new Delly.DBunny.DbColumnDesciptor { ColumnName = "Name", ColumnType = "VARCHAR(100)", PrimaryKeyFlag = false, NullableFlag = false }
};
var sql = provider.CreateTable(string.Empty, "Users", columns);
Console.WriteLine(sql.Sql);
