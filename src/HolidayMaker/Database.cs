using MySqlConnector;

namespace HolidayMaker;

public static class Database
{
    private static string connectionString = " ";

    public static void Configure()
    {
        string? password = Environment.GetEnvironmentVariable("HOLIDAY_DB_PASSWORD");
        if (string.IsNullOrEmpty(password))
        {
            Console.Write("Lösenord till MySQL: ");
            password = Console.ReadLine() ?? " ";
        }
        var builder = new MySqlConnectionStringBuilder
        {
            Server = "localhost",
            Database = "holiday_maker",
            UserID = "root",
            Password = password
        };
        connectionString = builder.ConnectionString;
    }
    public static MySqlConnection Open()
    {
        var connection = new MySqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}