# Holiday Maker

Holiday Maker är ett konsolprogram för en resebyrå. En operatör sitter i
telefon med en kund och hjälper hen att boka ett semesterboende. Programmet är
skrivet i C# och använder en MySQL-databas.

## Innehåll

| Sökväg | Vad det är |
|---|---|
| `sql/create_tables.sql` | Skapar databasen och tabellerna |
| `sql/testdata.sql` | Testdata: 3 orter, 5 boenden, 20 rum, 5 kunder, 25 bokningar |
| `sql/queries.sql` | Alla SQL-frågor som programmet använder |
| `src/HolidayMaker/` | Koden till programmet |
| `docs/er-diagram.png` | E/R-diagrammet |
| `docs/TESTER.md` | Hur jag har testat |
| `docs/REFLEKTION.md` | Min reflektion |

## Så kommer du igång

Du behöver .NET SDK 10 och MySQL 8.4. Jag kör MySQL i Docker.

**1. Starta MySQL** (byt `ditt_losenord` mot ett eget):

```bash
docker run --name mysql-kurs -e MYSQL_ROOT_PASSWORD=ditt_losenord -p 3306:3306 -d mysql:8.4
```

Efter en omstart av datorn måste du starta `mysql-kurs` igen i Docker Desktop.

**2. Skapa databasen.** Kör filerna i den här ordningen, till exempel i DataGrip:

1. `sql/create_tables.sql`
2. `sql/testdata.sql`

`testdata.sql` kan köras igen om du vill återställa datan.

**3. Starta programmet** från projektmappen:

```bash
dotnet run --project src/HolidayMaker/HolidayMaker.csproj
```

Programmet ansluter till `localhost:3306` som `root` och frågar efter
lösenordet till MySQL när det startar.

## Använda programmet

Välj eller registrera en kund först. Sedan kan du söka lediga rum, boka, se
kundens bokningar, ändra antal personer och avboka. Datum skrivs som `ÅÅÅÅ-MM-DD`.

Prova till exempel att söka på **Edinburgh, 2026-12-10 till 2026-12-13, 2
personer**. Rum 102 och 103 är redan bokade och syns inte.

## Datamodellen

![E/R-diagram](docs/er-diagram.png)

- En ort har flera boenden
- Ett boende har flera rum
- En kund kan ha flera bokningar
- Ett rum kan ha flera bokningar, på olika datum