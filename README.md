# Holiday Maker

Det här är min individuella projektuppgift i kursen Databaser (MAI26).

Holiday Maker är ett konsolprogram för en resebyrå. Tanken är att en operatör
sitter i telefon med en kund och hjälper hen att boka ett semesterboende.
Operatören kan registrera och välja kunder, söka lediga rum, boka, se kundens
bokningar, ändra antal personer och avboka.

Programmet är skrivet i C# och pratar med en MySQL-databas. All sökning,
filtrering och sortering görs i SQL.

## Vad som finns i projektet

| Sökväg | Vad det är |
|---|---|
| `sql/create_tables.sql` | Skapar databasen `holiday_maker` och tabellerna |
| `sql/testdata.sql` | Testdata: 3 orter, 5 boenden, 20 rum, 5 kunder och 25 bokningar |
| `sql/queries.sql` | Alla SQL-frågor som programmet använder. Jag testade dem i DataGrip först |
| `src/HolidayMaker/` | Koden till programmet |
| `docs/er-diagram.png` | E/R-diagrammet |
| `docs/TESTER.md` | Hur jag har testat |

## Det här behöver du

- .NET SDK 10
- MySQL 8.4. Jag kör den i Docker
- Gärna en databasklient som DataGrip, men det går utan

## Så kommer du igång

### 1. Starta MySQL

Jag använder Docker. Första gången skapar du containern så här (byt
`ditt_losenord` mot ett eget):

```bash
docker run --name mysql-kurs -e MYSQL_ROOT_PASSWORD=ditt_losenord -p 3306:3306 -d mysql:8.4
```

Om du har startat om datorn behöver du öppna Docker Desktop och starta
`mysql-kurs` igen. Det lärde jag mig den hårda vägen.

### 2. Skapa databasen och fyll på med testdata

Kör filerna i den här ordningen, till exempel i DataGrip:

1. `sql/create_tables.sql`
2. `sql/testdata.sql`

Det går också att köra dem från terminalen när containern är igång:

```bash
docker exec -i mysql-kurs mysql -uroot -pditt_losenord < sql/create_tables.sql
docker exec -i mysql-kurs mysql -uroot -pditt_losenord < sql/testdata.sql
```

`testdata.sql` tömmer tabellerna innan den lägger in datan, så den går att köra
igen om man vill börja om från början.

### 3. Starta programmet

```bash
cd src/HolidayMaker
dotnet run
```

Programmet kopplar upp sig mot `localhost:3306` med användaren `root`. När det
startar frågar det efter lösenordet till MySQL. Jag ville inte ha lösenordet i
koden, eftersom koden ligger på GitHub.

Om du inte vill skriva lösenordet varje gång kan du lägga det i en
miljövariabel:

```bash
HOLIDAY_DB_PASSWORD=ditt_losenord dotnet run
```

## Hur man använder programmet

```
1. Registrera ny kund
2. Välj befintlig kund
3. Sök lediga rum och boka
4. Visa kundens bokningar
5. Ändra antal personer i en bokning
6. Avboka
0. Avsluta
```

Datum skriver man som `ÅÅÅÅ-MM-DD`. För att kunna boka, ändra eller avboka
måste man först välja eller registrera en kund.

### Några sökningar att testa

| Sökning | Vad som händer |
|---|---|
| Edinburgh, 2026-12-10 till 2026-12-13, 2 personer | Lediga rum visas. Rum 102 och 103 är bokade och syns inte |
| Samma sökning men med pool | Inget hittas, inget hotell i Edinburgh har pool |
| Edinburgh, 2026-12-13 till 2026-12-15, 2 personer | Rum 102 går att boka samma dag som förra gästen checkar ut |
| Edinburgh, 5 personer | Inget hittas, största rummet tar 4 |

## Datamodellen

![E/R-diagram](docs/er-diagram.png)

Jag har fem tabeller och alla relationer är ett-till-många:

- En ort (`Location`) har flera boenden (`Accommodation`)
- Ett boende har flera rum (`Room`)
- En kund (`Customer`) kan ha flera bokningar (`Booking`)
- Ett rum kan ha flera bokningar, men på olika datum

Jag lade orterna i en egen tabell så att samma stad inte skrivs på flera
ställen. Totalpriset sparas inte i databasen. Det räknas ut från antal nätter
gånger priset per natt när det behövs, så det kan inte bli fel.

## Hur programmet fungerar

### Hur koden är uppdelad

Jag delade upp koden i några mindre filer så att det blir lättare att hitta:

| Fil | Vad den gör |
|---|---|
| `Program.cs` | Huvudmenyn. Kommer ihåg vilken kund som är vald |
| `Database.cs` | Kopplar upp mot databasen |
| `Input.cs` | Tar hand om det användaren skriver och frågar igen om det blir fel |
| `Customers.cs` | Registrera och välja kund |
| `Rooms.cs` | Söka lediga rum |
| `Bookings.cs` | Boka, visa, ändra och avboka |

Allt som pratar med databasen är `async`, så att programmet inte står och
väntar medan databasen svarar.

### Hur sökningen hittar lediga rum

Det här var den svåraste delen att förstå. Sökningen tar fram rummen i orten
som har plats för alla, och plockar sedan bort de rum som har en bokning som
krockar med datumen.

En bokning krockar om den börjar före den nya utcheckningen och slutar efter
den nya incheckningen. Det låter enkelt men fångar alla fall, även när en
gammal bokning ligger helt runt den nya. Eftersom jag använder `<` och `>`
istället för `<=` och `>=` räknas inte utcheckningsdagen, så en ny gäst kan
checka in samma dag som den förra checkar ut.

```sql
AND NOT EXISTS (
    SELECT 1 FROM Booking b
    WHERE b.room_id = r.id
      AND b.checkIn  < @checkOut
      AND b.checkOut > @checkIn)
```

Filtren för pool, restaurang och avstånd är valfria. Om operatören inte vill
filtrera skickas `0` eller `NULL`, och då gör villkoret ingen skillnad.

### Säkerhet

- Allt som användaren skriver skickas som parametrar (`@city`, `@guests` och så
  vidare) och klistras aldrig in direkt i SQL-texten. Det skyddar mot
  SQL-injektion.
- Sorteringen gick inte att skicka som parameter. Därför får operatören bara
  välja 1 eller 2, och koden väljer mellan två färdiga alternativ.
- Lösenordet till databasen finns inte i koden.
- När man ändrar eller avbokar kollar programmet att bokningen tillhör kunden
  som är vald, så man inte råkar ändra fel kunds bokning.

### Regler i databasen

Jag ville att databasen själv skulle säga nej till konstiga värden, inte bara
programmet:

- `NOT NULL` på allt som måste finnas
- `UNIQUE` på kundens e-post, på stadens namn och på boende + rumsnummer
- `CHECK` på omdöme (1–5), avstånd (minst 0), kapacitet och pris (större än 0),
  antal personer (större än 0) och att utcheckning är efter incheckning
- Främmande nycklar mellan tabellerna, så att en bokning alltid hör till en kund
  och ett rum som finns

När en bokning sparas kollar programmet igen att rummet fortfarande är ledigt
och har plats. Det görs i samma SQL-fråga som sparar bokningen
(`INSERT ... SELECT`), så om rummet hunnit bli upptaget sparas ingenting. Samma
sak när man ändrar antal personer: ändringen görs bara om det nya antalet får
plats i rummet.