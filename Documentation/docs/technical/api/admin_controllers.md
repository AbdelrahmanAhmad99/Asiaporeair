# Admin API Reference (Management)

The Admin API provides capabilities for system configuration, flight operations, and reporting. Access is strictly restricted to `Admin`, `SuperAdmin`, and `Supervisor` roles.

--- 

## Interactive API Explorer

For secure testing and schema validation, use the internal developer portal.
 
<div style="position: relative; padding-bottom: 56.25%; height: 0; overflow: hidden; max-width: 100%; border: 1px solid #e0e0e0; border-radius: 8px;"> 
<iframe src="https://share.apidog.com/762445f2-1d68-4894-b68b-6f4479003291" style="position: absolute; top: 0; left: 0; width: 100%; height: 100%; border: 0;" allowfullscreen> 
</iframe> </div>
 
-----

## Security & Authentication

Access to the Admin API is strictly controlled via Role-Based Access Control (RBAC).

### Base URL

`https://localhost:5001/api/v1`

### Super Admin Credentials

For initial system bootstrapping or emergency overrides, utilize the root account:
 
```json
{
  "email": "superadmin@asiaporeair.com",
  "password": "SuperAdmin_123*"
}
```

### Authorization Header

All requests must include a valid JWT generated via the `/auth/login` endpoint. The token must contain claims for roles: `Admin`, `SuperAdmin` or `Supervisor`.

  * **Header:** `Authorization: Bearer <your_admin_jwt>`

### Response Codes

| Code | Description |
| :--- | :--- |
| `200` | **OK**. Operation completed successfully. |
| `201` | **Created**. Resource created (e.g., Schedule, Instance). |
| `400` | **Bad Request**. Business logic violation (e.g., overlap in schedules). |
| `401` | **Unauthorized**. Invalid or missing JWT. |
| `403` | **Forbidden**. User lacks specific clearance (e.g., Crew Scheduler trying to delete an Airport). | 

-----

## Module 1: Infrastructure Management

### Onboard New Airport

Registers a new operational node in the network. This must be done before defining routes or schedules involving this location.

  * **Endpoint:** `POST /api/v1/admin/airports`
  * **Required Role:** `SuperAdmin`, `Admin`

**Request Body (`CreateAirportDto`)**

| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `iataCode` | string(3) | Yes | Unique IATA identifier (e.g., "DXB"). |
| `icaoCode` | string(4) | Yes | Unique ICAO identifier (e.g., "OMDB"). |
| `countryIsoCode` | string(3) | Yes | ISO 3166-1 alpha-3 code (e.g., "ARE"). |
| `latitude` | decimal | Yes | Geographical coordinate. |
| `longitude` | decimal | Yes | Geographical coordinate. |

**Example Request**

```json
{
  "iataCode": "DXB",
  "icaoCode": "OMDB",
  "name": "Dubai International Airport",
  "city": "Dubai",
  "countryIsoCode": "ARE",
  "latitude": 25.2532,
  "longitude": 55.3657,
  "altitude": 62
}
```

**Response (201 Created)**

```json
{
  "statusCode": 201,
  "message": "Airport created successfully.",
  "data": {
    "iataCode": "DXB",
    "icaoCode": "OMDB",
    "name": "Dubai International Airport",
    "city": "Dubai",
    "countryIsoCode": "ARE",
    "countryName": "United Arab Emirates",
    "latitude": 25.2532,
    "longitude": 55.3657,
    "altitude": 62
  }
}
```

-----

## Module 2: Flight Planning

### Create Flight Schedule

Defines a recurring master schedule (e.g., "SQ322 flies daily"). This does not create an actual flyable flight for a specific date; it creates the *template* used by the Generator.

  * **Endpoint:** `POST /api/v1/admin/schedules`
  * **Required Role:** `Admin`, `NetworkPlanner`

**Request Body (`CreateFlightScheduleDto`)**

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `flightNo` | string | The commercial flight number (e.g., "SQ322"). |
| `routeId` | integer | ID of the associated Route entity. |
| `daysOfWeek` | byte | Bitmask representing operating days (1=Mon ... 64=Sun, 127=Daily). |
| `departureTimeScheduled` | string($date-time) | The base time for departure (Date part is ignored, Time part is used). |

**Example Request**

```json
{
  "flightNo": "SQ322",
  "routeId": 105,
  "airlineIataCode": "SQ",
  "aircraftTypeId": 1,
  "departureTimeScheduled": "2026-11-28T23:30:00Z",
  "arrivalTimeScheduled": "2026-11-29T05:55:00Z",
  "daysOfWeek": 127
}
```

**Response (201 Created)**

```json
{
  "statusCode": 201,
  "message": "Flight schedule created successfully.",
  "data": {
    "scheduleId": 86,
    "flightNo": "SQ322",
    "routeId": 15,
    "routeName": "DXB - IST",
    "airlineIataCode": "SQ",
    "aircraftTypeModel": "A350-900",
    "departureTimeScheduled": "2025-11-28T23:30:00Z",
    "arrivalTimeScheduled": "2025-11-29T05:55:00Z",
    "daysOfWeek": 127
  }
}
```

-----

## Module 3: Flight Operations (Dispatch)

### Generate Flight Instances

This is a **batch operation**. It takes the Master Schedules defined above and "instantiates" them for a specific date range. It automatically assigns aircraft based on type availability.

  * **Endpoint:** `POST /api/v1/admin/operations/generate-from-schedules`
  * **Required Role:** `Admin`, `Dispatcher`

**Request Body (`GenerateInstancesRequestDto`)**

```json
{
  "startDate": "2026-03-01T00:00:00Z",
  "endDate": "2026-03-01T23:59:59Z",
  "airlineIataCode": "SQ"  
}
```

**Response (200 OK)**

Returns a report of the generation process, including warnings for conflicts or instances where an aircraft could not be automatically assigned (marked as TBA).

```json
{
  "statusCode": 200,
  "message": "Flight instance generation completed.",
  "data": {
    "instancesCreated": 24,
    "warnings": [
      "Instance SQ322 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ24 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ912 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ470 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ974 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ888 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ208 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ778 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ296 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ779 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ975 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ913 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ120 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ471 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ553 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ422 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ121 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ880 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ423 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ611 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ295 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ321 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ23 on 3/1/2026 created without an assigned aircraft (TBA).",
      "Instance SQ322 on 3/1/2026 created without an assigned aircraft (TBA).", 
    ],
    "failures": []
  }
}
```

-----
 
## Crew & Resource Management 

Manages staff profiles, qualifications, and the assignment process.
 
### Assign Crew

Assigns flight deck and cabin crew to a specific Flight Instance.

  * **Endpoint:** `POST /api/v1/admin/crew/assignments`
  * **Required Role:** `CrewScheduler`, `Admin`

**Request Body (`AssignCrewRequestDto`)**

| Parameter | Type | Valid Values |
| :--- | :--- | :--- |
| `flightInstanceId` | int | The ID of the specific date/flight. |
| `role` | string | `Captain`, `First Officer`, `Purser`, `Cabin Crew`. |

**Example Request**

```json
{
  "flightInstanceId": 101,
  "assignments": [
    {
      "crewMemberEmployeeId": 11,
      "role": "Captain"
    },
    {
      "crewMemberEmployeeId": 16,
      "role": "First Officer"
    },
    {
      "crewMemberEmployeeId": 28,
      "role": "Lead Attendant"
    }
  ]
}
```

**Response (200 OK)**

```json
{
  "statusCode": 200,
  "message": "Crew assigned successfully."
}
```

-----

## Module 4: Customer Support (Ticket Management)

### Force Status Change

Allows high-level overrides of ticket statuses. Used when a passenger is denied boarding, offloaded, or when the system state must be manually corrected.

  * **Endpoint:** `PUT /api/v1/Ticket/admin/status/{ticketId}`
  * **Required Role:** `SupportLead`, `Supervisor`

**Request Body (`UpdateTicketStatusDto`)**

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `newStatus` | string | `Issued`, `CheckedIn`, `Boarded`, `Used`, `Cancelled`. |
| `reason` | string | Audit log reason for the manual override. |

**Example Request**

```json
{
  "newStatus": "Cancelled",
  "reason": "Passenger no-show at gate; baggage offloaded."
}
```

**Response (200 OK)**

```json
{ 
  "message": "Ticket status updated successfully." 
}
```