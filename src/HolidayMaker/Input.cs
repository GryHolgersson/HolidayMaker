namespace HolidayMaker;

// Hjälpmetoder för inmatning. Frågar igen tills svaret är giltigt,
// så att vanliga skrivfel inte kraschar programmet.
public static class Input
{
    // Text som inte får vara tom
    public static string ReadText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string text = (Console.ReadLine() ?? "").Trim();
            if (text != "") return text;
            Console.WriteLine("Du måste skriva något. Försök igen.");
        }
    }

    // Heltal mellan min och max
    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int number) && number >= min && number <= max)
                return number;
            Console.WriteLine($"Skriv ett heltal mellan {min} och {max}.");
        }
    }

    // Heltal som man får hoppa över med Enter (används för valfria filter)
    public static int? ReadOptionalInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string text = (Console.ReadLine() ?? "").Trim();
            if (text == "") return null;
            if (int.TryParse(text, out int number) && number >= 0) return number;
            Console.WriteLine("Skriv ett heltal (0 eller mer), eller tryck Enter för att hoppa över.");
        }
    }

    // Datum i formatet ÅÅÅÅ-MM-DD
    public static DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string text = (Console.ReadLine() ?? "").Trim();
            if (DateOnly.TryParseExact(text, "yyyy-MM-dd", out DateOnly date))
                return date;
            Console.WriteLine("Skriv datumet som ÅÅÅÅ-MM-DD, till exempel 2026-12-10.");
        }
    }

    // Ja eller nej
    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt + " (j/n): ");
            string answer = (Console.ReadLine() ?? "").Trim().ToLower();
            if (answer == "j") return true;
            if (answer == "n") return false;
            Console.WriteLine("Svara j eller n.");
        }
    }
}