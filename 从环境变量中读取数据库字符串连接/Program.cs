// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
var connStr = Environment.GetEnvironmentVariable("DefaultDB:ConnStr");
Console.WriteLine(connStr);