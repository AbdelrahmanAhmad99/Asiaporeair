# Entity-Relationship Diagram (ERD)

This document provides the high-level Entity-Relationship Diagram (ERD) for the **Asiaporeair** database schema. It is the single source of truth for all relationships, primary keys, and foreign keys across the entire domain model.

## 1. ERD Visualization
  
The diagram below represents the complete schema topology.

!!! tip "Interactive View"
    Click the diagram below to open it in **Full-Screen Mode**. You can **Zoom** and **Pan** to inspect column details and relationships.

<figure>
  <a class="glightbox" href="/Asiaporeair/technical/Images/Asiapore Airline Database Schema.svg" data-type="image" data-width="100%" data-height="auto" data-desc-position="bottom">
    <img src="/Asiaporeair/technical/Images/Asiapore Airline Database Schema.svg" alt="Asiaporeair Full Schema Architecture" width="100%">
  </a>
  <figcaption>Figure 1: Complete Database Topology (Click to Expand)</figcaption>
</figure>

> **Note:**  abstract "Advanced Schema Navigation"
    For a **professional-grade experience** that includes relationship filtering, metadata inspection, and deep search capabilities, we recommend using our hosted documentation platform.This view offers superior control compared to the static diagram above.
   [**View Live Interactive Schema on dbdocs.io** ](https://dbdocs.io/abdelrahmanahmed606ky/Asiapore-Airline-Database-Schema?view=relationships) 

---

<iframe 
  src="https://dbdocs.io/abdelrahmanahmed606ky/Asiapore-Airline-Database-Schema?view=relationships" 
  width="100%" 
  height="600px" 
  style="border:none; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);">
</iframe>

 ---
## 2. Key Relationship Patterns

Our design focuses on clear and explicit relationships to support data integrity and efficient querying.

* **One-to-Many (1:N):**
    * One **`FlightSchedule`** has many **`FlightInstance`** (a recurring schedule creates many concrete flight instances).
    * One **`User`** can have many **`Booking`** records.
	* Each **`AircraftType`** can have many **`Aircraft`** (its physical seating layout).
* **One-to-One (1:1):**
    * Each specialized user role (`Admin`, `Pilot`, `Supervisor`) has a 1:1 relationship with the base **`AppUser`** entity via shared keys.
* **Many-to-Many (M:N) via Join Table:**
    * The relationship between a **`FlightInstance`** and a **`CrewMember`** is resolved using the **`FlightCrew`** junction table, allowing a crew member to be on multiple flights and a flight to have multiple crew members.

## 3. Naming and Integrity Conventions

We enforce standard conventions across the persistence layer:

* **Naming:** All table and column names use **snake\_case** in the database for consistency with SQL standards (e.g., `flight_instance`, `ticket_code`).
* **Primary Keys (PK):** Defined as integer (`INT`) identity columns, named `[EntityName]Id` (e.g., `BookingId`). Exception: Composite keys and natural keys (e.g., `Airport.IataCode`).
* **Foreign Keys (FK):** Explicitly named to indicate the relationship: `[ReferencedEntityName]Id` (e.g., `AircraftTypeId` in the `Aircraft` table).
* **Soft Deletion:** Most business entities include a `IsDeleted` (BIT) flag, allowing us to logically delete records without losing historical data, critical for auditing and reporting.