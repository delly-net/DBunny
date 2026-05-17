using System;

string _testSchema = "test_schema";
var sql1 = $"INSERT INTO `{_testSchema}`.`Users` (Name, Email, Age, CreatedAt) VALUES (@name, @email, @age, @createdAt)";
Console.WriteLine("SQL 1: " + sql1);

// Also test with GetSpecialName pattern
string GetSpecialName(string name) => $"`{name}`";
var sql2 = $"INSERT INTO {GetSpecialName(_testSchema)}.{GetSpecialName("Users")} (Name, Email, Age, CreatedAt) VALUES (@name, @email, @age, @createdAt)";
Console.WriteLine("SQL 2: " + sql2);

// Test hex
Console.WriteLine("SQL 1 hex: " + BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(sql1.Substring(0, 20))));
