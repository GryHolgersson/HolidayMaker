-- =====================================================
-- Holiday Maker: alla SQL-frågor som programmet använder
-- Testade i DataGrip. Värdena efter SET är bara exempel,
-- i C# skickas de in som parametrar (@city, @guests osv.)
-- =====================================================
USE holiday_maker;


-- -----------------------------------------------------
-- 1. Sök lediga rum
-- -----------------------------------------------------
SET @city       = 'Edinburgh';
SET @checkIn    = '2026-12-10';
SET @checkOut   = '2026-12-13';
SET @guests     = 2;
SET @pool       = 0;      -- 1 = måste ha pool,       0 = spelar ingen roll
SET @restaurant = 0;      -- 1 = måste ha restaurang, 0 = spelar ingen roll
SET @maxBeach   = NULL;   -- max meter till strand,    NULL = spelar ingen roll
SET @maxCenter  = NULL;   -- max meter till centrum,   NULL = spelar ingen roll

SELECT a.name AS boende, l.city AS ort, r.roomNumber AS rum,
       r.capacity AS platser, r.pricePerNight AS pris, a.rating AS omdome
FROM Room r
JOIN Accommodation a ON a.id = r.accommodation_id
JOIN Location l      ON l.id = a.location_id
WHERE l.city = @city
  AND r.capacity >= @guests
  -- Rummet får inte ha någon bokning som krockar med perioden.
  -- Krock = gamla bokningen börjar före nya utcheckningen
  --         OCH slutar efter nya incheckningen.
  -- Utcheckningsdagen räknas inte, så 10-13 och 13-15 krockar inte.
  AND NOT EXISTS (
        SELECT 1 FROM Booking b
        WHERE b.room_id = r.id
          AND b.checkIn  < @checkOut
          AND b.checkOut > @checkIn
  )
  AND (@pool = 0       OR a.pool = 1)
  AND (@restaurant = 0 OR a.restaurant = 1)
  AND (@maxBeach  IS NULL OR a.beachDistance      <= @maxBeach)
  AND (@maxCenter IS NULL OR a.cityCenterDistance <= @maxCenter)
ORDER BY r.pricePerNight ASC;
-- Sortering på omdöme istället:
-- ORDER BY a.rating DESC, r.pricePerNight ASC;


-- -----------------------------------------------------
-- 2. Registrera ny kund
-- -----------------------------------------------------
SET @firstName   = 'Simon';
SET @lastName    = 'Ringman';
SET @email       = 'simon.ringman@example.com';
SET @phoneNumber = '070-666 66 66';

INSERT INTO Customer (firstName, lastName, email, phoneNumber)
VALUES (@firstName, @lastName, @email, @phoneNumber);

-- Hämta numret som den nya kunden fick
SELECT LAST_INSERT_ID() AS nytt_kundnummer;


-- -----------------------------------------------------
-- 3. Visa alla kunder (för att välja en befintlig kund)
-- -----------------------------------------------------
SELECT id, firstName, lastName, email, phoneNumber
FROM Customer
ORDER BY lastName, firstName;


-- -----------------------------------------------------
-- 4. Skapa bokning
-- Sparas BARA om rummet är stort nog och ledigt.
-- Det är den nya kontrollen precis när bokningen sparas.
-- -----------------------------------------------------
SET @customerId = 1;
SET @roomId     = 2;
SET @guests     = 2;
SET @checkIn    = '2026-12-13';
SET @checkOut   = '2026-12-15';

INSERT INTO Booking (customer_id, room_id, guestCount, checkIn, checkOut)
SELECT @customerId, r.id, @guests, @checkIn, @checkOut
FROM Room r
WHERE r.id = @roomId
  AND r.capacity >= @guests
  AND NOT EXISTS (
        SELECT 1 FROM Booking b
        WHERE b.room_id = r.id
          AND b.checkIn  < @checkOut
          AND b.checkOut > @checkIn
  );

-- 1 = bokningen sparades, 0 = nekades (krock eller för många personer)
SELECT ROW_COUNT() AS sparade_bokningar;


-- -----------------------------------------------------
-- 5. Bekräftelse: bokningsnummer och totalpris
-- totalpris = antal nätter x pris per natt
-- -----------------------------------------------------
SELECT b.id AS bokningsnummer, a.name AS boende, r.roomNumber AS rum,
       b.checkIn, b.checkOut, b.guestCount AS personer,
       DATEDIFF(b.checkOut, b.checkIn) AS natter,
       DATEDIFF(b.checkOut, b.checkIn) * r.pricePerNight AS totalpris
FROM Booking b
JOIN Room r          ON r.id = b.room_id
JOIN Accommodation a ON a.id = r.accommodation_id
WHERE b.id = LAST_INSERT_ID();



-- -----------------------------------------------------
-- 6. Visa en kunds bokningar
-- -----------------------------------------------------
SET @customerId = 1;

SELECT b.id AS bokningsnummer, a.name AS boende, l.city AS ort, r.roomNumber AS rum,
       b.checkIn, b.checkOut, b.guestCount AS personer,
       DATEDIFF(b.checkOut, b.checkIn) * r.pricePerNight AS totalpris
FROM Booking b
JOIN Room r          ON r.id = b.room_id
JOIN Accommodation a ON a.id = r.accommodation_id
JOIN Location l      ON l.id = a.location_id
WHERE b.customer_id = @customerId
ORDER BY b.checkIn;


-- -----------------------------------------------------
-- 7. Ändra antal personer
-- Ändras BARA om det nya antalet ryms i rummet.
-- -----------------------------------------------------
SET @bookingId = 26;
SET @newGuests = 1;

UPDATE Booking b
JOIN Room r ON r.id = b.room_id
SET b.guestCount = @newGuests
WHERE b.id = @bookingId
  AND r.capacity >= @newGuests;

-- 1 = ändrad, 0 = nekad (eller samma antal som innan)
SELECT ROW_COUNT() AS andrade_bokningar;


-- -----------------------------------------------------
-- 8. Avboka
-- Raden tas bort, så rummet blir ledigt igen för perioden.
-- -----------------------------------------------------
SET @bookingId = 26;

DELETE FROM Booking
WHERE id = @bookingId;