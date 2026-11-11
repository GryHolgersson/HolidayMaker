using MySqlConnector;

namespace HolidayMaker;

// Ett ledigt rum som hittades i sökningen.
public record AvailableRoom(
    int RoomId, string Accommodation, string City, string RoomNumber,
    int Capacity, decimal PricePerNight, int Rating);

// Resultatet av en sökning: vad operatören sökte på och vilka rum som hittades.
public record RoomSearch(DateOnly CheckIn, DateOnly CheckOut, int Guests, List<AvailableRoom> Rooms);

// Allt som har med rum att göra.
public static class Rooms
{
    public static async Task<RoomSearch> SearchAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- Sök lediga rum ---");
        string city = Input.ReadText("Ort: ");

        DateOnly checkIn = Input.ReadDate("Incheckning (ÅÅÅÅ-MM-DD): ");
        DateOnly checkOut;
        while (true)
        {
            checkOut = Input.ReadDate("Utcheckning (ÅÅÅÅ-MM-DD): ");
            if (checkOut > checkIn) break;
            Console.WriteLine("Utcheckningen måste vara efter incheckningen.");
        }
        int guests = Input.ReadInt("Antal personer: ", 1, 20);

        Console.WriteLine();
        Console.WriteLine("Filter (tryck bara Enter för att hoppa över avstånden):");
        bool requirePool       = Input.ReadYesNo("Måste ha pool?");
        bool requireRestaurant = Input.ReadYesNo("Måste ha restaurang?");
        int? maxBeach  = Input.ReadOptionalInt("Max avstånd till strand (meter): ");
        int? maxCenter = Input.ReadOptionalInt("Max avstånd till centrum (meter): ");

        Console.WriteLine();
        Console.WriteLine("Sortering: 1 = pris (lägst först), 2 = omdöme (högst först)");
        int sortChoice = Input.ReadInt("Välj sortering: ", 1, 2);

        // ORDER BY kan inte vara en parameter. Koden väljer därför mellan två
        // fasta texter, så att användarens inmatning aldrig hamnar i SQL:en.
        string orderBy = sortChoice == 1
            ? "r.pricePerNight ASC"
            : "a.rating DESC, r.pricePerNight ASC";

        string sql = $@"
            SELECT r.id, a.name, l.city, r.roomNumber, r.capacity, r.pricePerNight, a.rating
            FROM Room r
            JOIN Accommodation a ON a.id = r.accommodation_id
            JOIN Location l      ON l.id = a.location_id
            WHERE l.city = @city
              AND r.capacity >= @guests
              -- Rummet får inte ha någon bokning som krockar med perioden
              AND NOT EXISTS (
                    SELECT 1 FROM Booking b
                    WHERE b.room_id = r.id
                      AND b.checkIn  < @checkOut
                      AND b.checkOut > @checkIn)
              AND (@pool = 0       OR a.pool = 1)
              AND (@restaurant = 0 OR a.restaurant = 1)
              AND (@maxBeach  IS NULL OR a.beachDistance      <= @maxBeach)
              AND (@maxCenter IS NULL OR a.cityCenterDistance <= @maxCenter)
            ORDER BY {orderBy};";

        var rooms = new List<AvailableRoom>();

        await using (var connection = await Database.OpenAsync())
        await using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@city", city);
            command.Parameters.AddWithValue("@guests", guests);
            command.Parameters.AddWithValue("@checkIn", checkIn);
            command.Parameters.AddWithValue("@checkOut", checkOut);
            command.Parameters.AddWithValue("@pool", requirePool ? 1 : 0);
            command.Parameters.AddWithValue("@restaurant", requireRestaurant ? 1 : 0);
            // Inget värde = "spelar ingen roll" = NULL i databasen
            command.Parameters.AddWithValue("@maxBeach", (object?)maxBeach ?? DBNull.Value);
            command.Parameters.AddWithValue("@maxCenter", (object?)maxCenter ?? DBNull.Value);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rooms.Add(new AvailableRoom(
                    reader.GetInt32("id"),
                    reader.GetString("name"),
                    reader.GetString("city"),
                    reader.GetString("roomNumber"),
                    reader.GetInt32("capacity"),
                    reader.GetDecimal("pricePerNight"),
                    reader.GetInt32("rating")));
            }
        }

        PrintRooms(rooms, checkIn, checkOut);
        return new RoomSearch(checkIn, checkOut, guests, rooms);
    }

    // Skriver ut sökresultatet som en numrerad tabell (1, 2, 3 ...).
    private static void PrintRooms(List<AvailableRoom> rooms, DateOnly checkIn, DateOnly checkOut)
    {
        Console.WriteLine();
        if (rooms.Count == 0)
        {
            Console.WriteLine("Inga lediga rum matchar sökningen.");
            return;
        }

        int nights = checkOut.DayNumber - checkIn.DayNumber;
        Console.WriteLine($"{rooms.Count} lediga rum, {checkIn} till {checkOut} ({nights} nätter):");
        Console.WriteLine();
        Console.WriteLine($"{"Nr",-4}{"Boende",-25}{"Ort",-11}{"Rum",-13}{"Platser",8}{"Pris/natt",11}{"Totalt",10}{"Omdöme",8}");
        Console.WriteLine(new string('-', 90));

        for (int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            decimal total = nights * room.PricePerNight;
            Console.WriteLine(
                $"{i + 1,-4}{room.Accommodation,-25}{room.City,-11}{room.RoomNumber,-13}" +
                $"{room.Capacity,8}{room.PricePerNight,11:N0}{total,10:N0}{room.Rating,8}");
        }
    }
}