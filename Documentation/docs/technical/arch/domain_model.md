# Domain Model and Core Entities

The Domain layer is dedicated to representing the real-world business concepts of the **Asiaporeair** airline and airport management system. It contains the **Entities** that hold data and enforce basic, inherent business rules.

## 1. Core Entities Overview

The model centers around a few key entity groups:

* **Users & Roles (Identity):** `User`, `Admin`, `SuperAdmin`, `Supervisor`, `Pilot`, `Attendant`, `Passenger`.
* **Flight Operations:** `AircraftType`, `AircraftConfig`, `Aircraft`, `Airport`, `Airline`, `Route`, `FlightSchedule`, `FlightInstance`, `etc..`.
* **Commercial/Booking:** `Booking`, `BookingPassenger`, `Ticket`, `Payment`, `AncillaryProduct`, `CabinClass`, `Seat`, `etc..`.

## 2. Relationship Highlights

The system is highly relational, ensuring data integrity across operational and commercial aspects.

| Entities | Relationship | Description |
| :--- | :--- | :--- |
| `FlightSchedule` & `FlightInstance` | One-to-Many | A Schedule defines a recurring flight (e.g., daily), while an Instance is a concrete flight on a specific date. |
| `Booking` & `Ticket` | One-to-Many | A single booking can contain multiple tickets (one per passenger). |
| `User` & `Passenger` | One-to-Many | A registered `User` can create multiple `Passenger` records (for themselves and others). |
| `AircraftType` & `Aircraft` | One-to-Many | An AircraftType  has many Aircrafts  . |

## 3. Entities Reference

The `Domain` project includes several crucial entities and value objects.

* **`AppUser`:** Extends `IdentityUser` for authentication and authorization.
* **`Booking`:** Represents a confirmed reservation. Key fields include `BookingCode`, `Status`, `TotalAmount`.
* **`Airport`:** Represents a location. Key fields include `IataCode` (Primary Key), `Name`, `City`.
* **`FlightInstance`:** Represents the actual flight flown. Key fields include `ScheduledDeparture`, `ActualDeparture`, `AircraftId`.

> **Note:** The complete database schema (including all foreign keys and constraints) is documented separately in the Data Persistence section [database-ERD](/Asiaporeair/technical/database/erd/).
 
