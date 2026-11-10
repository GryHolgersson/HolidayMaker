using HolidayMaker;
using MySqlConnector;

Database.Configure();

// Kunden som operatören jobbar med just nu (null = ingen vald)
int? customerId = null;
string customerName = "";

bool running = true;
while (running)
{
    Console.WriteLine();
    Console.WriteLine("========= HOLIDAY MAKER =========");
    Console.WriteLine(customerId == null
        ? "Ingen kund vald"
        : $"Vald kund: {customerName} (nr {customerId})");
    Console.WriteLine("---------------------------------");
    Console.WriteLine("1. Registrera ny kund");
    Console.WriteLine("2. Välj befintlig kund");
    Console.WriteLine("3. Sök lediga rum och boka");
    Console.WriteLine("4. Visa kundens bokningar");
    Console.WriteLine("5. Ändra antal personer i en bokning");
    Console.WriteLine("6. Avboka");
    Console.WriteLine("0. Avsluta");

    int choice = Input.ReadInt("Välj: ", 0, 6);

    try
    {
        switch (choice)
        {
            case 1:
                var newCustomer = await Customers.RegisterAsync();
                if (newCustomer != null)
                {
                    customerId = newCustomer.Value.Id;
                    customerName = newCustomer.Value.Name;
                }
                break;

            case 2:
                var chosenCustomer = await Customers.ChooseAsync();
                if (chosenCustomer != null)
                {
                    customerId = chosenCustomer.Value.Id;
                    customerName = chosenCustomer.Value.Name;
                }
                break;

            case 0:
                running = false;
                Console.WriteLine("Hej då!");
                break;

            default:
                Console.WriteLine("Kommer snart! 🚧");
                break;
        }
    }
    catch (MySqlException ex)
    {
        // Om databasen krånglar visas ett meddelande istället för att programmet kraschar
        Console.WriteLine("Något gick fel med databasen: " + ex.Message);
    }
}