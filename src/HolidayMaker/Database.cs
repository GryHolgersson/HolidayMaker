using MySqlConnector;

namespace HolidayMaker;

// Här bor allt som har med anslutningen till databasen att göra.
public static class Database
{
    private static string connectionString = "";

    // Bygger "telefonnumret" till databasen. Lösenordet skrivs inte i koden.
    public static void Configure()
    {
        // Finns lösenordet i en miljövariabel används det, annars frågar vi.
        string? password = Environment.GetEnvironmentVariable("HOLIDAY_DB_PASSWORD");
        if (string.IsNullOrEmpty(password))
        {
            Console.Write("Lösenord till MySQL: ");
            password = Console.ReadLine() ?? "";
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

    // Öppnar en ny anslutning ("lyfter luren") utan att låsa programmet medan vi väntar.
    public static async Task<MySqlConnection> OpenAsync()
    {
        var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}