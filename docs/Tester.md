# Testdokumentation – Holiday Maker

Alla tester är gjorda manuellt, först med SQL-frågorna i DataGrip (mån 9/11)
och sedan i konsolprogrammet (ons 11/11). Testdatan kommer från `sql/testdata.sql`.

| # | Krav | Hur jag testade | Resultat |
|---|---|---|---|
| 1 | Ett ledigt rum visas och ett upptaget filtreras bort | Sökte Edinburgh 10–13/12, 2 personer. Rum 102 och 103 är bokade under perioden. | ✅ 3 lediga rum visades (Castle View 2 och 3, Royal Mile 201). 102 och 103 visades inte. |
| 2 | Ett rum kan bokas samma dag som en tidigare kund checkar ut | Royal Mile rum 102 är bokat 10–13/12. Sökte 13–15/12. Bokade också rum 102 13–15/12 i DataGrip. | ✅ Rum 102 visades i sökningen och bokningen sparades. |
| 3 | Överlappande bokningar nekas, även när den ena perioden omsluter den andra | Rum 103 är bokat 5–20/12. Sökte 11–12/12 i programmet. Försökte boka rum 103 10–12/12 i DataGrip. Körde också samma bokning två gånger. | ✅ Rum 103 visades inte i sökningen. Bokningsförsöken gav 0 sparade rader. |
| 4 | Ett för stort sällskap kan inte boka rummet | Sökte Edinburgh med 5 personer (största rummet tar 4). Försökte boka rum 101 (plats för 1) för 2 personer i DataGrip. | ✅ "Inga lediga rum matchar sökningen". Bokningen gav 0 sparade rader. |
| 5 | Kombinerade sökfilter och båda sorteringarna fungerar | Edinburgh 10–13/12, 2 pers: restaurang = j, pool = j, restaurang + max 300 m till centrum, sortering 1 och 2. | ✅ Restaurang: 1 rum (201). Pool: inga rum. Restaurang + centrum: 1 rum (201). Sortering 1: billigast först. Sortering 2: högst omdöme först. |
| 6 | Ändring av antal personer följer rummets kapacitet | Castle View rum 3 (plats för 2): ändrade till 1 och sedan till 3. | ✅ Ändring till 1 sparades. Ändring till 3 nekades med meddelande om kapaciteten. |
| 7 | Avbokning frigör rummet | Avbokade bokning 28 (Castle View rum 3, 10–13/12) och sökte samma period igen. | ✅ Bokningen försvann ur kundens lista och rummet syntes igen i sökningen. |
| 8 | Uppgifterna finns kvar efter att applikationen startats om | Avslutade programmet, startade det igen och visade Annas bokningar. | ✅ Bokning 27 (gjord tidigare i programmet) fanns kvar. |

## Övriga tester

| Vad | Resultat |
|---|---|
| CHECK-villkor: lägga in omdöme 7 | ✅ Nekades av databasen (`accommodation_rating_check`) |
| UNIQUE på e-post: registrera samma e-post två gånger | ✅ Nekades med meddelandet "Det finns redan en kund med den e-postadressen" |
| Felaktig inmatning i menyn (text, för stora tal) | ✅ Programmet säger till och frågar igen, ingen krasch |
| Felaktigt datumformat och utcheckning före incheckning | ✅ Programmet säger till och frågar igen |
| Välja ett nummer som inte finns i listan | ✅ Programmet frågar igen |
| Visa, ändra eller avboka utan vald kund | ✅ "Välj eller registrera en kund först" |