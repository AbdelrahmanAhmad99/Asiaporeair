# Database Schema Reference

This reference catalog details every table and its columns in the **AsiaporeairDB**, complementing the visual ERD by providing specific data type and constraint information.

## 1. Core Identity Tables

| Table Name | Column Name | Data Type | Key/Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| **`AspNetUsers`** | `Id` | `NVARCHAR(450)` | PK | Identity key for authentication. |
| | `Email` | `NVARCHAR(256)` | UNIQUE | User's login email. |
| | `FirstName` | `NVARCHAR(50)` | NOT NULL | User's given name. |
| | `LastName` | `NVARCHAR(50)` | NOT NULL | User's family name. |
| **`Admin`** | `AdminId` | `INT` | PK | Primary key for the Admin profile. |
| | `AppUserId` | `NVARCHAR(450)` | FK, UNIQUE | Links to `AspNetUsers`. |

## 2. Flight Operations Tables

| Table Name | Column Name | Data Type | Key/Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| **`Airport`** | `IataCode` | `VARCHAR(3)` | PK | The unique 3-letter IATA code. |
| | `City` | `NVARCHAR(100)` | NOT NULL | City name. |
| | `CountryId` | `VARCHAR(3)` | FK | Links to `Country.IsoCode`. |
| **`FlightSchedule`** | `ScheduleId` | `INT` | PK | Recurring flight pattern ID. |
| | `RouteId` | `INT` | FK | The predefined route (A-B). |
| | `DaysOfWeek` | `NVARCHAR(7)` | NOT NULL | Bitmask or string indicating operational days. |
| **`FlightInstance`** | `InstanceId` | `INT` | PK | Specific flight on a date/time. |
| | `ScheduleId` | `INT` | FK | Links to the recurring schedule. |
| | `ScheduledDeparture` | `DATETIME2` | NOT NULL | The planned departure time. |
| | `AircraftId` | `INT` | FK | The assigned physical aircraft. |

## 3. Booking and Commercial Tables

| Table Name | Column Name | Data Type | Key/Constraint | Description |
| :--- | :--- | :--- | :--- | :--- |
| **`Booking`** | `BookingId` | `INT` | PK | The primary booking record ID. |
| | `BookingCode` | `VARCHAR(6)` | UNIQUE | The PNR (Passenger Name Record) code. |
| | `BookingDate` | `DATETIME2` | NOT NULL | When the booking was created. |
| | `Status` | `VARCHAR(20)` | NOT NULL | e.g., 'Confirmed', 'Pending', 'Cancelled'. |
| **`Ticket`** | `TicketId` | `INT` | PK | Individual ticket record. |
| | `BookingId` | `INT` | FK | Links to the parent booking. |
| | `FlightInstanceId` | `INT` | FK | Links to the specific flight. |
| | `SeatId` | `INT` | FK, NULL | The assigned seat (if applicable). |

> **Best Practice:** The full schema details are derived from the EF Core model configurations (`EntityTypeConfiguration<T>`) within the `Infrastructure.Data.Configuration` namespace.