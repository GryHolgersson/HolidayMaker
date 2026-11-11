using MySqlConnector;

namespace HolidayMaker;

// Allt som har med bokningar att göra.
public static class Bookings
{
    // Låter operatören välja ett rum från sökresultatet och boka det åt kunden.
    public static async Task CreateAsync(int customerId, RoomSearch search)
    {
        if (!Input.ReadYesNo("Vill du boka ett av rummen?")) return;

        // Operatören väljer ett rum med numret i listan (1, 2, 3 ...)
        int choice = Input.ReadInt($"Välj rum (1-{search.Rooms.Count}, 0 = avbryt): ", 0, search.Rooms.Count);
        if (choice == 0) return;
        AvailableRoom room = search.Rooms[choice - 1];   // listan börjar på 0, därför -1

        // Sparar bokningen BARA om rummet fortfarande är ledigt och stort nog.
        // Det är den nya kontrollen precis när bokningen sparas.
        const string sql = @"
            INSERT INTO Booking (customer_id, room_id, guestCount, checkIn, checkOut)
            SELECT @customerId, r.id, @guests, @checkIn, @checkOut
            FROM Room r
            WHERE r.id = @roomId
              AND r.capacity >= @guests
              AND NOT EXISTS (
                    SELECT 1 FROM Booking b
                    WHERE b.room_id = r.id
                      AND b.checkIn  < @checkOut
                      AND b.checkOut > @checkIn);";

        await using var connection = await Database.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@customerId", customerId);
        command.Parameters.AddWithValue("@roomId", room.RoomId);
        command.Parameters.AddWithValue("@guests", search.Guests);
        command.Parameters.AddWithValue("@checkIn", search.CheckIn);
        command.Parameters.AddWithValue("@checkOut", search.CheckOut);

        // Hur många rader sparades? 1 = lyckades, 0 = nekades
        int savedRows = await command.ExecuteNonQueryAsync();
        if (savedRows == 0)
        {
            Console.WriteLine("Bokningen kunde inte sparas. Rummet är inte längre ledigt " +
                              "för perioden, eller har inte plats för alla.");
            return;
        }

        long bookingId = command.LastInsertedId;
        int nights = search.CheckOut.DayNumber - search.CheckIn.DayNumber;
        decimal total = nights * room.PricePerNight;

        Console.WriteLine();
        Console.WriteLine("✅ Bokningen är klar!");
        Console.WriteLine($"Bokningsnummer: {bookingId}");
        Console.WriteLine($"Boende:         {room.Accommodation}, {room.City}");
        Console.WriteLine($"Rum:            {room.RoomNumber}");
        Console.WriteLine($"Datum:          {search.CheckIn} till {search.CheckOut} ({nights} nätter)");
        Console.WriteLine($"Personer:       {search.Guests}");
        Console.WriteLine($"Totalpris:      {total:N0} kr ({nights} × {room.PricePerNight:N0} kr)");
    }
}