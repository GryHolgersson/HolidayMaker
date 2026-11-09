-- Holiday Maker: testdata. 
USE holiday_maker;


-- Töm alla tabeller först, så att filen kan köras flera gånger
SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE Booking;
TRUNCATE TABLE Room;
TRUNCATE TABLE Accommodation;
TRUNCATE TABLE Customer;
TRUNCATE TABLE Location;
SET FOREIGN_KEY_CHECKS = 1;

-- Orter
INSERT INTO Location (city) VALUES
('Edinburgh'),  -- id 1
('Orlando'),    -- id 2
('Kreta');      -- id 3

-- Boenden: namn, ort, pool, restaurang, strand (m), centrum (m), omdöme, beskrivning
INSERT INTO Accommodation (name, location_id, pool, restaurant, beachDistance, cityCenterDistance, rating, description) VALUES
('Royal Mile Hotel',        1, 0, 1,  5000,  200, 4, 'Mitt på Royal Mile nära slottet'), -- id 1
('Castle View Guesthouse',  1, 0, 0,  6000,  800, 3, 'Mysigt pensionat med slottsutsikt'), -- id 2
('Sunshine Resort Orlando', 2, 1, 1, 85000, 8000, 5, 'Stort resort nära nöjesparkerna'), -- id 3
('Lake Eola Inn',           2, 1, 0, 90000,  300, 3, 'Enkelt hotell i centrala Orlando'), -- id 4
('Chania Beach Villa',      3, 1, 1,    50, 1200, 2, 'Enkelt boende vid stranden');       -- id 5

-- Rum: boende, rumsnummer, max personer, pris per natt
INSERT INTO Room (accommodation_id, roomNumber, capacity, pricePerNight) VALUES
(1, '101', 1,  750.00),   -- rum id 1
(1, '102', 2, 1200.00),   -- rum id 2
(1, '103', 2, 1250.00),   -- rum id 3
(1, '201', 4, 2100.00),   -- rum id 4
(2, '1',   1,  450.00),   -- rum id 5
(2, '2',   2,  650.00),   -- rum id 6
(2, '3',   2,  700.00),   -- rum id 7
(2, '4',   3,  900.00),   -- rum id 8
(3, '11',  2, 1100.00),   -- rum id 9
(3, '12',  2, 1150.00),   -- rum id 10
(3, '21',  4, 1900.00),   -- rum id 11
(3, '22',  6, 2800.00),   -- rum id 12
(4, 'A1',  1,  900.00),   -- rum id 13
(4, 'A2',  2, 1400.00),   -- rum id 14
(4, 'B1',  2, 1450.00),   -- rum id 15
(4, 'B2',  4, 2300.00),   -- rum id 16
(5, 'Olive',   2,  980.00),  -- rum id 17
(5, 'Lemon',   2,  950.00),  -- rum id 18
(5, 'Sunset',  4, 1600.00),  -- rum id 19
(5, 'Harbour', 1,  600.00);  -- rum id 20

-- Kunder (påhittade)
INSERT INTO Customer (firstName, lastName, email, phoneNumber) VALUES
('Anna',   'Svensson',  'anna.svensson@example.com',  '070-111 11 11'),
('Erik',   'Johansson', 'erik.johansson@example.com', '070-222 22 22'),
('Fatima', 'Ali',       'fatima.ali@example.com',     '070-333 33 33'),
('Lars',   'Nilsson',   'lars.nilsson@example.com',   '070-444 44 44'),
('Maja',   'Berg',      'maja.berg@example.com',      '070-555 55 55');

-- Bokningar: kund, rum, antal personer, incheckning, utcheckning
INSERT INTO Booking (customer_id, room_id, guestCount, checkIn, checkOut) VALUES
(1,  2, 2, '2026-12-10', '2026-12-13'),  -- TEST: checkar ut 13/12, ny kund ska kunna checka in 13/12
(2,  1, 1, '2026-12-11', '2026-12-12'),  -- TEST: kort bokning, en sökning 10-15/12 ska INTE visa rum 1
(3,  3, 2, '2026-12-05', '2026-12-20'),  -- TEST: lång bokning, omsluter t.ex. 10-12/12
(4,  4, 4, '2026-12-01', '2026-12-03'),
(5,  4, 3, '2026-12-20', '2026-12-27'),
(1,  5, 1, '2026-12-02', '2026-12-06'),
(2,  6, 2, '2026-12-14', '2026-12-16'),
(3,  7, 2, '2026-12-24', '2026-12-26'),
(4,  8, 3, '2026-12-08', '2026-12-15'),
(5,  9, 2, '2026-12-01', '2026-12-04'),
(1,  9, 2, '2026-12-04', '2026-12-07'),  -- direkt efter förra bokningen i samma rum
(2, 10, 1, '2026-12-18', '2026-12-22'),
(3, 11, 4, '2026-12-10', '2026-12-17'),
(4, 12, 6, '2026-12-27', '2026-12-31'),
(5, 13, 1, '2026-12-03', '2026-12-05'),
(1, 14, 2, '2026-12-12', '2026-12-14'),
(2, 15, 2, '2026-12-06', '2026-12-09'),
(3, 16, 3, '2026-12-19', '2026-12-23'),
(4, 17, 2, '2026-12-01', '2026-12-08'),
(5, 18, 2, '2026-12-09', '2026-12-11'),
(1, 19, 4, '2026-12-15', '2026-12-18'),
(2, 20, 1, '2026-12-28', '2026-12-30'),
(3,  1, 1, '2026-12-20', '2026-12-22'),
(4,  6, 2, '2026-12-01', '2026-12-05'),
(5, 14, 1, '2026-12-22', '2026-12-26');