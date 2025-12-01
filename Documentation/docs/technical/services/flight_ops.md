# Flight Operations & Scheduling Business Service

## 1. Overview and Architectural Scope

The Flight Operations and Scheduling services are the foundational layer for all time-series and inventory management within the **Asiaporeair** ecosystem. They manage the static definition of flight routes (`FlightSchedulingService`) and the dynamic, real-time status of individual flights (`FlightOperationsService` and `FlightService`).

These services ensure resource constraints are honored (e.g., aircraft availability, gate assignments) and provide the accurate, real-time data necessary for customer-facing systems and operational control centers (OCC).

| Service Interface | Responsibility Area | Key Business Function |
| :--- | :--- | :--- |
| **`IFlightSchedulingService`** | Master Data & Inventory | Defines and manages permanent routes, flight numbers, and multi-stop segments. |
| **`IFlightOperationsService`** | Real-Time Control & Dispatch | Manages dynamic updates: aircraft assignment, status changes (delay, departure), and gate control. |
| **`IFlightService`** | Customer & Search Facade | Provides availability search, pricing integration, and flight details for public consumption. |

---

## 2. Flight Scheduling Service (`IFlightSchedulingService`)

This service manages the master schedule data, defining the recurring flight patterns and legs (segments) that form the airline's network inventory.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`GetScheduleByIdAsync`** | Retrieves the full static definition of a flight schedule. This includes the route definition, associated aircraft type (e.g., Boeing 777), and the operator/airline. |
| **`CreateFlightScheduleAsync`** | Creates a new recurring flight entry (e.g., daily flight SQ123). **Pre-Conditions:** Requires valid, existing `RouteId` and `AirlineId`. The system enforces that a schedule cannot be created without a defined route and operator. |
| **`UpdateFlightScheduleAsync`** | Modifies the parameters of a static schedule (e.g., changing the operating aircraft *type* for all future instances). **Critical Constraint:** Changes must not conflict with existing, confirmed `FlightInstance` records that have already been created (i.e., historical instances are immutable). |
| **`CreateFlightLegDefAsync`** | Defines a segment of a multi-stop flight schedule (e.g., SIN to HKG as the first segment, and HKG to TPE as the second). |
| | **1. Input Validation:** Verifies both `DepartureAirportIataCode` and `ArrivalAirportIataCode` against the master `Airport` table. |
| | **2. Uniqueness Constraint:** Enforces that the `SegmentNumber` is unique for the specific parent `ScheduleId` to ensure the correct sequence is maintained (e.g., Segment 1, then Segment 2). |

---

## 3. Flight Operations Service (`IFlightOperationsService`)

This is the core operational service, responsible for managing the dynamic lifecycle and resource allocation for every specific flight instance (e.g., SQ123 on June 1st).

| Method | Business Logic and Rules |
| :--- | :--- |
| **`GetInstanceByIdAsync`** | Fetches the full, real-time details of a flight instance, including its current `Status`, assigned `Aircraft`, and operational timing. Used primarily by Ground Operations and OCC (Operations Control Center). |
| **`CreateFlightInstanceFromScheduleAsync`** | **Critical Instantiation Flow.** Creates a concrete, date-specific `FlightInstance` based on a predefined `FlightSchedule` template. |
| | **1. Date Constraint:** Must not create an instance for a date/time that has already passed. |
| | **2. Inventory Snapshot:** Inherits default capacity and route segments from the `FlightSchedule` template. |
| | **3. Initial State:** The new instance is automatically assigned the `Scheduled` status. |
| **`UpdateFlightStatusAsync`** | Updates the state of a flight instance (e.g., `Scheduled` -> `Delayed` -> `Departed` -> `Arrived`). |
| | **1. Integrity Check:** The system logs the user and timestamp of the status change and requires high-level role authorization (`Dispatch`, `OCC`) for critical transitions (e.g., setting `Departed` status). |
| | **2. Pre-Condition:** Cannot set status to `Departed` if `Gate` or `Aircraft` assignment is missing. |
| **`AssignAircraftToInstanceAsync`** | Assigns a specific physical aircraft (identified by `TailNumber`) to a flight instance. |
| | **1. Type Match:** The aircraft's `AircraftTypeId` must match the type specified in the parent `FlightSchedule`. |
| | **2. Availability Check (Anti-Conflict):** Executes an atomic check to ensure the chosen `TailNumber` is not already scheduled for another flight instance that conflicts with the new flight's `ScheduledDeparture` and `ScheduledArrival` times. If a conflict is detected, the assignment fails. |
| | **3. Status:** The aircraft must be in an `Operational` status (not `Maintenance` or `Decommissioned`). |
| **`UpdateGateAndTerminalAsync`** | Updates the physical resource allocation (Gate and Terminal) for the flight instance. |
| | **1. Mandatory Check:** Gate and terminal codes must be valid against the master `AirportFacilities` table for the specified airport. |
| | **2. Conflict Check:** Prevents double-booking the same gate at the same time for two different arrivals/departures. |
| **`UpdateEstimatedTimesAsync`** | Updates the **Estimated Time of Departure (ETD)** and **Estimated Time of Arrival (ETA)** due to operational changes (e.g., weather, maintenance). |
| | **1. Data Source:** Primarily used by OCC/Dispatch staff based on updated flight plans or ATC information. |
| | **2. Status Dependency:** This operation is invalid if the flight is already in `Departed` or `Arrived` status. |
| | **3. Passenger Notification:** Triggers the `NotificationService` to alert affected passengers of the delay/change if the new time exceeds a threshold (e.g., 30 minutes). |
| **`UpdateActualTimesAsync`** | Records the final, verified **Actual Time of Departure (ATD)** and **Actual Time of Arrival (ATA)**. |
| | **1. Post-Event Trigger:** This method is typically called by a ground system (or manually by an authorized agent) immediately upon the physical event. |
| | **2. Final Status:** Calling this method usually precedes or coincides with setting the flight status to `Departed` (for ATD) or `Arrived` (for ATA). |
| **`CancelFlightInstanceAsync`** | Formally cancels a specific flight instance. |
| | **1. Status Update:** Changes the `FlightInstance.Status` to `Cancelled`. |
| | **2. Booking Integration:** Calls the `IBookingService` to identify all associated bookings and initiates the passenger re-accommodation or refund process. |
| | **3. Notification:** Triggers a high-priority communication to all booked passengers and crew. |
| **`GenerateOperationalDashboardAsync`** | Aggregates real-time data (arrivals and departures) for a specific airport within a defined time window. |
| | **1. Filtering Rule:** Only includes flights with statuses like `Scheduled`, `Delayed`, `Boarding`, or `Departed`. Excludes historical or completed flights for clear, real-time focus. |
| | **2. Sorting:** Results are typically sorted by `EstimatedDeparture` or `ScheduledArrival` for operational clarity. |
 
---

## 4. Flight Service (`IFlightService`)

This service acts as the public-facing facade for flight information, supporting the customer search and booking process. It consumes data from both the Scheduling and Operations services.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`SearchFlightsAsync`** | Performs a comprehensive search for available flight instances based on customer criteria. |
| | **1. Core Filter:** Filters `FlightInstance` entities by exact `OriginIataCode`, `DestinationIataCode`, and `DepartureDate`. |
| | **2. Capacity Validation:** For each found flight, calculates `AvailableSeats` by subtracting the count of `ConfirmedBookings` from the `AircraftType.MaxSeats`. Only returns flights where `AvailableSeats` is greater than or equal to the requested number of passengers. |
| | **3. Pricing Integration:** Invokes the `IPricingService` to retrieve the current `BasePrice` for the itinerary. This process integrates dynamic pricing factors but remains quick to support high-volume searches. |
| | **4. Status Filter:** Only includes flights with a `Scheduled` or `Delayed` status (i.e., excludes `Cancelled` or `Departed` flights). |
| **`GetFlightDetailsAsync`** | Retrieves the public view of a single flight instance, including route map, operational status, and current time estimates. **Data Source:** Combines static data from `FlightSchedule` with real-time data from `FlightOperationsService`. |