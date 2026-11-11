# ✅ Checklista – Holiday Maker

Inlämning: fredag 13 november på Learnpoint

## Veckoplan
| Dag | Mål |
|---|---|
| Mån 9/11 | Del 2 klar: alla SQL-frågor |
| Tis 10/11 (skolan) | C#: projekt, koppling till databasen, kunder, sök |
| Ons 11/11 (hela dagen) | C#: boka, visa, ändra, avboka, felhantering + testa |
| Tor 12/11 (handledning) | Visa läraren, fråga om det osäkra. README + ER-diagram |
| Fre 13/11 | Reflektion + inlämning |

## 🗄️ Del 1: Databasen
- [x] Skapa git-repo och mappar
- [x] Rita första ER-diagram (DrawSQL)
- [x] Skapa tabellerna
- [x] Lägga till vakter (CHECK, UNIQUE, NOT NULL)
- [x] Spara byggbeskrivningen i `sql/create_tables.sql`
- [x] Testdata i `sql/testdata.sql` (5 boenden, 3 orter, 20 rum, 5 kunder, 25 bokningar)
- [x] Spara i Git

## 🔍 Del 2: SQL-frågor i DataGrip
- [x] Visa rum i en ort (JOIN)
- [x] Sök lediga rum (NOT EXISTS)
- [x] Filter (pool, restaurang, avstånd) och sortering (pris, omdöme)  ⬅️ HÄR
- [x] Lägga till kund och visa kunder
- [x] Skapa bokning
- [x] Visa kundens bokningar med totalpris
- [x] Ändra antal personer
- [x] Avboka
- [x] Spara alla frågor i `sql/queries.sql`

## 💻 Del 3: C#-programmet
- [x] Skapa projektet och koppla till databasen
- [x] Meny: registrera och välja kund
- [ ] Meny: sök lediga rum (med filter och sortering)
- [ ] Meny: boka (kolla tillgänglighet igen innan sparning)
- [ ] Meny: visa bokningar
- [ ] Meny: ändra antal personer
- [ ] Meny: avboka
- [ ] Felhantering (programmet kraschar inte vid fel inmatning)

## 🧪 Del 4: Testa och dokumentera
- [ ] Ledigt rum visas, upptaget filtreras bort
- [ ] Kan checka in samma dag som någon checkar ut
- [ ] Överlappande bokningar nekas (även när en omsluter en annan)
- [ ] För stort sällskap nekas
- [ ] Kombinerade filter och båda sorteringarna fungerar
- [ ] Ändring av antal personer följer kapaciteten
- [ ] Avbokning frigör rummet
- [ ] Datan finns kvar efter omstart

## 📝 Del 5: Inlämning
- [ ] Uppdatera ER-diagrammet och exportera som PDF/bild
- [ ] README
- [ ] Reflektion