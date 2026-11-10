using HolidayMaker;
using MySqlConnector;

Database.Configure();

try
{
    using var connection = Database.Open();
    using var command = new MySqlCommand("SELECT COUNT(*) FROM Customer", connection);
    int count = Convert.ToInt32(command.ExecuteScalar());

    Console.WriteLine($"Kontakt med databasen! Antal kunder: {count}");

}
catch (MySqlException ex)
{
    Console.WriteLine("Kunde inte ansluta till databasen: " + ex.Message);
}