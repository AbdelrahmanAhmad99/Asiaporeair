# Booking & Ticketing Services  
 
## 1. Overview

This document provides a high-level business logic reference for the core services responsible for the **Asiaporeair** booking lifecycle, from initial reservation and passenger management to check-in, ticketing, and loyalty program integration.

These services reside within the `Application.Services` layer and enforce core business rules, transactional integrity (via `IUnitOfWork`), and authorization checks before executing any data persistence operations.
  
* **Booking Service (`IBookingService`)**
* **Passenger Service (`IPassengerService`)**
* **Ticket Service (`ITicketService`)**
* **Boarding Pass Service (`IBoardingPassService`)**
* **Frequent Flyer Service (`IFrequentFlyerService`)**

All services rely on the **Unit of Work (`IUnitOfWork`)** pattern to ensure transactional integrity (ACID properties) during complex, multi-step operations.
 
 
## 2. Booking Service (`IBookingService`)

The Booking Service manages the complete reservation lifecycle, acting as the primary transactional entry point for customer flight purchases.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`CreateBookingAsync`** | **Core Transactional Flow.** This method executes a multi-step, atomic transaction (ACID compliant) to create a new booking in a `Pending` state. |
| | **1. Security & Authorization:** Must validate the authenticated `ClaimsPrincipal` and retrieve a corresponding internal `User` profile. |
| | **2. Flight Validity:** The target `FlightInstance` must exist, must be in the `Scheduled` status, and the `ScheduledDeparture` time must be in the future (i.e., has not already departed). |
| | **3. Capacity Check:** Verifies sufficient available seats based on the aircraft's total capacity minus existing confirmed bookings. Fails if capacity is exceeded. |
| | **4. Pricing Integration:** Calls the `IPricingService` to calculate the final total fare, incorporating base costs, taxes, and any dynamic pricing factors. The transaction fails if pricing errors occur. |
| | **5. Transaction Scope:** An explicit database transaction is used to ensure all related entities (Booking Header, Passengers, Ancillary Sales) are created or rolled back together. |
| | **6. Ancillary Snapshots:** Ancillary products (e.g., baggage, meals) are validated for existence, and their current prices are snapshotted (`PricePaid` from `BaseCost`) to prevent historical data discrepancies. |
| **`GetBookingDetailsByIdAsync`** | Retrieves the full, detailed record of a booking. **Authorization Rule:** Access is granted only if the current user is the booking owner, or holds an authorized role (`Admin`, `Supervisor`, etc.). |
| **`UpdateBookingPaymentStatusAsync`** | Updates the booking's `PaymentStatus` (e.g., to `CONFIRMED` or `FAILED`). This method is typically invoked by a secure payment gateway webhook or an authorized internal system. |
| **`CancelBookingAsync`** | Executes the soft-deletion of the `Booking` entity and all associated `BookingPassenger` junction entities. **Authorization Rule:** Restricted to the booking owner or authorized administrators. |
| **`GetPassengerManifestForFlightAsync`** | Retrieves an operational list of confirmed passengers for ground staff use. **Status Filter:** Only includes passengers linked to bookings with a `CONFIRMED` payment status. |

---

## 3. Ticket Service (`ITicketService`)

The Ticket Service is responsible for generating the official electronic document of carriage (the e-ticket) after a booking is confirmed.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`GenerateTicketsForBookingAsync`** | Generates a unique `Ticket` for every passenger on a booking. |
| | **1. Payment Confirmation:** Requires the parent booking's `PaymentStatus` to be `CONFIRMED` before generation can proceed. |
| | **2. Idempotency:** Checks for the existence of an active ticket for the specific `(BookingId, PassengerId)` combination. If found, generation is skipped to prevent duplicates. |
| | **3. Unique Code:** Generates and assigns a unique `TicketCode` (the industry standard document number/barcode) to the new ticket. |
| **`GetTicketsByBookingAsync`** | Retrieves all tickets related to a single booking ID. **Authorization Rule:** Access to the underlying booking must be confirmed via `AuthorizeBookingAccessAsync`. |
| **`UpdateTicketStatusAsync`** | Updates the operational status of a single ticket (e.g., from `Issued` to `CheckedIn` or `Boarded`). **Pre-Conditions:** Prevents specific illegal transitions, such as reverting a ticket's status once it has been marked as `Boarded`. |

---

## 4. Boarding Pass Service (`IBoardingPassService`)

This service manages the Check-In process, seat confirmation, and the final generation of the Boarding Pass document necessary for gate access.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`GenerateBoardingPassAsync`** | Executes the formal Check-In process for a passenger. |
| | **1. Authorization:** Requires the user to either own the booking or possess the operational `CheckInAgent` role. |
| | **2. Idempotency Check:** Fails if a `BoardingPass` already exists for the specified passenger and booking. |
| | **3. Seat Mandate:** Requires a non-null `SeatAssignmentId` (seat must be selected) on the `BookingPassenger` record to proceed. |
| | **4. Status Update:** Automatically calls the `ITicketService` to update the associated ticket's status to `CheckedIn`. |
| | **5. Boarding Time:** Calculates the mandatory `BoardingTime` based on the `FlightInstance.ScheduledDeparture` (default rule: 45 minutes prior). |
| **`GetBoardingPassByBookingPassengerAsync`** | Retrieves the Boarding Pass document for presentation. **Authorization Rule:** Same as `GetBookingDetailsByIdAsync`. |
| **`ScanBoardingPassAsync`** | Used for gate control (boarding validation). |
| | **1. Flight Match:** Validates the scanned Boarding Pass's internal flight ID matches the flight ID of the gate/scanning station. |
| | **2. Ticket Status Check:** Verifies the underlying ticket status is valid for boarding (`CheckedIn` or `Boarded`). |
| | **3. Final Status Update:** Upon successful validation, updates the ticket status to `Boarded`. |

---

## 5. Passenger Service (`IPassengerService`)

This service is the system of record for managing customer and non-user passenger profile data.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`AddMultiplePassengersAsync`** | Creates and persists new `Passenger` profiles and links them to a specific booking using the `BookingPassenger` entity. **Default Linkage:** New profiles are linked to the `UserId` who created the booking. |
| **`GetPassengerByIdAsync`** | Retrieves a single passenger profile. **Authorization Rule:** Requires the logged-in user to be the profile owner or an authorized administrative user (`Admin`, `SuperAdmin`). |
| **`GetMyPassengersAsync`** | Retrieves a list of all `Passenger` profiles linked to the currently authenticated user's account, facilitating quick selection during new bookings. |
| **`UpdatePassengerAsync`** | Updates personal data fields (Name, DOB, Passport Number) on an existing profile. Requires authorization (user owns profile). |
| **`DeletePassengerAsync`** | Soft-deletes a passenger profile from the system. |

---

## 6. Frequent Flyer Service (`IFrequentFlyerService`)

The Frequent Flyer Service manages the Asiaporeair loyalty program, including account management and point accrual.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`CreateAccountAsync`** | Creates a new `FrequentFlyer` record and links it to an existing `User` profile. **Authorization Rule:** Currently restricted to `Admin` or `SuperAdmin` roles. **Pre-Check:** Fails if the target user already possesses an active loyalty account. |
| **`GetMyAccountAsync`** | Retrieves the loyalty program details (Card Number, Level, Award Points) for the authenticated user. |
| **`AwardPointsForBookingAsync`** | **Critical Post-Flight/Confirmation Process.** |
| | **1. Status Check:** Confirms the booking payment status is `CONFIRMED`. |
| | **2. Linkage Check:** Verifies the booking user is linked to an active `FrequentFlyer` account. |
| | **3. Idempotency Check:** Prevents double awarding by checking the `Booking.PointsAwarded` flag. |
| | **4. Transaction:** Calculates the earned points and atomically updates the `FrequentFlyer` balance and sets the `PointsAwarded` flag on the `Booking`. |
| **`ManuallyAdjustPointsAsync`** | Allows authorized staff to add or subtract points from a flyer's balance. **Authorization Rule:** Restricted to `Admin` or `SuperAdmin`. **Policy:** The operation is logged, and adjustments resulting in a negative balance are permitted but marked with a warning. |
| **`DeleteAccountAsync`** | Soft-deletes the Frequent Flyer account and removes the `FrequentFlyerId` link from the corresponding `User` profile. |