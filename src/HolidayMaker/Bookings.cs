using MySqlConnector;

namespace HolidayMaker;

// En bokning som tillhör en kund, så som den visas i listan.
public record CustomerBooking(
    int BookingId, string Accommodation, string City, string RoomNumber,
    DateOnly CheckIn, DateOnly CheckOut, int Guests, int Capacity, decimal Total);

// Allt som har med bokningar att göra.
public static class Bookings
{
    // ---------------------------------------------------------------
    // BOKA
    // ---------------------------------------------------------------

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

    // ---------------------------------------------------------------
    // VISA
    // ---------------------------------------------------------------

    // Hämtar och visar alla bokningar för kunden. Returnerar listan.
    public static async Task<List<CustomerBooking>> ShowAsync(int customerId)
    {
        const string sql = @"
            SELECT b.id, a.name, l.city, r.roomNumber, b.checkIn, b.checkOut,
                   b.guestCount, r.capacity,
                   DATEDIFF(b.checkOut, b.checkIn) * r.pricePerNight AS total
            FROM Booking b
            JOIN Room r          ON r.id = b.room_id
            JOIN Accommodation a ON a.id = r.accommodation_id
            JOIN Location l      ON l.id = a.location_id
            WHERE b.customer_id = @customerId
            ORDER BY b.checkIn;";

        var bookings = new List<CustomerBooking>();

        await using (var connection = await Database.OpenAsync())
        await using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@customerId", customerId);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                bookings.Add(new CustomerBooking(
                    reader.GetInt32("id"),
                    reader.GetString("name"),
                    reader.GetString("city"),
                    reader.GetString("roomNumber"),
                    DateOnly.FromDateTime(reader.GetDateTime("checkIn")),
                    DateOnly.FromDateTime(reader.GetDateTime("checkOut")),
                    reader.GetInt32("guestCount"),
                    reader.GetInt32("capacity"),
                    reader.GetDecimal("total")));
            }
        }

        Console.WriteLine();
        if (bookings.Count == 0)
        {
            Console.WriteLine("Kunden har inga bokningar.");
            return bookings;
        }

        Console.WriteLine($"{"Nr",-4}{"Bokn.nr",-9}{"Boende",-25}{"Ort",-11}{"Rum",-13}{"In",-12}{"Ut",-12}{"Pers",5}{"Totalt",10}");
        Console.WriteLine(new string('-', 101));
        for (int i = 0; i < bookings.Count; i++)
        {
            var b = bookings[i];
            Console.WriteLine(
                $"{i + 1,-4}{b.BookingId,-9}{b.Accommodation,-25}{b.City,-11}{b.RoomNumber,-13}" +
                $"{b.CheckIn,-12}{b.CheckOut,-12}{b.Guests,5}{b.Total,10:N0}");
        }
        return bookings;
    }

    // ---------------------------------------------------------------
    // ÄNDRA ANTAL PERSONER
    // ---------------------------------------------------------------

    public static async Task ChangeGuestsAsync(int customerId)
    {
        var bookings = await ShowAsync(customerId);
        if (bookings.Count == 0) return;

        int choice = Input.ReadInt($"Välj bokning (1-{bookings.Count}, 0 = avbryt): ", 0, bookings.Count);
        if (choice == 0) return;
        var booking = bookings[choice - 1];

        int newGuests = Input.ReadInt(
            $"Nytt antal personer (rummet har plats för {booking.Capacity}): ", 1, 20);

        // Ändras BARA om det nya antalet ryms i rummet,
        // och bara om bokningen tillhör den valda kunden.
        const string sql = @"
            UPDATE Booking b
            JOIN Room r ON r.id = b.room_id
            SET b.guestCount = @newGuests
            WHERE b.id = @bookingId
              AND b.customer_id = @customerId
              AND r.capacity >= @newGuests;";

        await using var connection = await Database.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@newGuests", newGuests);
        command.Parameters.AddWithValue("@bookingId", booking.BookingId);
        command.Parameters.AddWithValue("@customerId", customerId);

        int rows = await command.ExecuteNonQueryAsync();
        if (rows == 0)
            Console.WriteLine($"Ändringen nekades: rummet har bara plats för {booking.Capacity} personer.");
        else
            Console.WriteLine($"✅ Bokning {booking.BookingId} gäller nu {newGuests} personer.");
    }

    // ---------------------------------------------------------------
    // AVBOKA
    // ---------------------------------------------------------------

    public static async Task CancelAsync(int customerId)
    {
        var bookings = await ShowAsync(customerId);
        if (bookings.Count == 0) return;

        int choice = Input.ReadInt($"Välj bokning att avboka (1-{bookings.Count}, 0 = avbryt): ", 0, bookings.Count);
        if (choice == 0) return;
        var booking = bookings[choice - 1];

        if (!Input.ReadYesNo($"Vill du verkligen avboka bokning {booking.BookingId} " +
                             $"({booking.Accommodation}, {booking.CheckIn} till {booking.CheckOut})?"))
            return;

        // Tar bort bokningen, bara om den tillhör den valda kunden.
        const string sql = @"
            DELETE FROM Booking
            WHERE id = @bookingId
              AND customer_id = @customerId;";

        await using var connection = await Database.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@bookingId", booking.BookingId);
        command.Parameters.AddWithValue("@customerId", customerId);

        int rows = await command.ExecuteNonQueryAsync();
        if (rows == 0)
            Console.WriteLine("Bokningen kunde inte hittas. Den kan redan vara avbokad.");
        else
            Console.WriteLine($"✅ Bokning {booking.BookingId} är avbokad. Rummet är ledigt igen för perioden.");
    }
}