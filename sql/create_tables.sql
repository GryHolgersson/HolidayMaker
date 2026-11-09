-- Holiday Maker: skapar databasen och alla tabeller
DROP DATABASE IF EXISTS holiday_maker;
CREATE DATABASE holiday_maker;
USE holiday_maker;

CREATE TABLE Customer
(
    id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    firstName   VARCHAR(255) NOT NULL,
    lastName    VARCHAR(255) NOT NULL,
    email       VARCHAR(255) NOT NULL,
    phoneNumber VARCHAR(255) NOT NULL,
    CONSTRAINT customer_email_unique UNIQUE (email)
);

CREATE TABLE Location
(
    id   INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    city VARCHAR(255) NOT NULL,
    CONSTRAINT location_city_unique UNIQUE (city)
);

CREATE TABLE Accommodation
(
    id                 INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name               VARCHAR(255) NOT NULL,
    location_id        INT UNSIGNED NOT NULL,
    pool               TINYINT(1)   NOT NULL,
    restaurant         TINYINT(1)   NOT NULL,
    beachDistance      INT          NOT NULL,
    cityCenterDistance INT          NOT NULL,
    rating             INT          NOT NULL,
    description        VARCHAR(255) NULL,
    CONSTRAINT accommodation_location_fk
        FOREIGN KEY (location_id) REFERENCES Location (id),
    CONSTRAINT accommodation_pool_check       CHECK (pool IN (0, 1)),
    CONSTRAINT accommodation_restaurant_check CHECK (restaurant IN (0, 1)),
    CONSTRAINT accommodation_beach_check      CHECK (beachDistance >= 0),
    CONSTRAINT accommodation_center_check     CHECK (cityCenterDistance >= 0),
    CONSTRAINT accommodation_rating_check     CHECK (rating BETWEEN 1 AND 5)
);

CREATE TABLE Room
(
    id               INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    accommodation_id INT UNSIGNED  NOT NULL,
    roomNumber       VARCHAR(255)  NOT NULL,
    capacity         INT           NOT NULL,
    pricePerNight    DECIMAL(8, 2) NOT NULL,
    CONSTRAINT room_accommodation_roomnumber_unique
        UNIQUE (accommodation_id, roomNumber),
    CONSTRAINT room_accommodation_fk
        FOREIGN KEY (accommodation_id) REFERENCES Accommodation (id),
    CONSTRAINT room_capacity_check CHECK (capacity > 0),
    CONSTRAINT room_price_check    CHECK (pricePerNight > 0)
);

CREATE TABLE Booking
(
    id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    customer_id INT UNSIGNED NOT NULL,
    room_id     INT UNSIGNED NOT NULL,
    guestCount  INT          NOT NULL,
    checkIn     DATE         NOT NULL,
    checkOut    DATE         NOT NULL,
    CONSTRAINT booking_customer_fk
        FOREIGN KEY (customer_id) REFERENCES Customer (id),
    CONSTRAINT booking_room_fk
        FOREIGN KEY (room_id) REFERENCES Room (id),
    CONSTRAINT booking_guestcount_check CHECK (guestCount > 0),
    CONSTRAINT booking_dates_check      CHECK (checkOut > checkIn)
);