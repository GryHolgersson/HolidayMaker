using MySqlConnector;

namespace HolidayMaker;

// Allt som har med kunder att göra.
public static class Customers
{
    // Registrerar en ny kund. Returnerar kundens nummer och namn, eller null om det misslyckades.
    public static async Task<(int Id, string Name)?> RegisterAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- Registrera ny kund ---");
        string firstName = Input.ReadText("Förnamn: ");
        string lastName  = Input.ReadText("Efternamn: ");
        string email     = Input.ReadText("E-post: ");
        string phone     = Input.ReadText("Telefon: ");

        const string sql = @"
            INSERT INTO Customer (firstName, lastName, email, phoneNumber)
            VALUES (@firstName, @lastName, @email, @phoneNumber);";

        await using var connection = await Database.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        // Parametrar: det användaren skrev skickas som värden, aldrig som SQL-kod
        command.Parameters.AddWithValue("@firstName", firstName);
        command.Parameters.AddWithValue("@lastName", lastName);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@phoneNumber", phone);

        try
        {
            await command.ExecuteNonQueryAsync();
            int id = (int)command.LastInsertedId;   // numret den nya kunden fick
            string name = $"{firstName} {lastName}";
            Console.WriteLine($"Kunden {name} registrerades med kundnummer {id}.");
            return (id, name);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            // UNIQUE-vakten på e-post sa nej
            Console.WriteLine("Det finns redan en kund med den e-postadressen.");
            return null;
        }
    }

    // Visar alla kunder och låter operatören välja en.
    public static async Task<(int Id, string Name)?> ChooseAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- Välj kund ---");

        const string sql = @"
            SELECT id, firstName, lastName, email, phoneNumber
            FROM Customer
            ORDER BY lastName, firstName;";

        // Sparar kunderna så att vi kan kolla att operatören väljer ett nummer som finns
        var customers = new Dictionary<int, string>();

        await using (var connection = await Database.OpenAsync())
        await using (var command = new MySqlCommand(sql, connection))
        await using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32("id");
                string name = $"{reader.GetString("firstName")} {reader.GetString("lastName")}";
                customers[id] = name;
                Console.WriteLine($"{id,4}. {name,-22} {reader.GetString("email"),-32} {reader.GetString("phoneNumber")}");
            }
        } // här stängs anslutningen, innan vi väntar på att operatören skriver

        if (customers.Count == 0)
        {
            Console.WriteLine("Det finns inga kunder ännu.");
            return null;
        }

        int chosen = Input.ReadInt("Ange kundnummer (0 = avbryt): ", 0, int.MaxValue);
        if (chosen == 0) return null;

        if (!customers.ContainsKey(chosen))
        {
            Console.WriteLine("Det finns ingen kund med det numret.");
            return null;
        }

        Console.WriteLine($"Vald kund: {customers[chosen]}");
        return (chosen, customers[chosen]);
    }
}