# Clean Architecture Deep Dive

The **Asiaporeair** project is built upon the principles of **Clean Architecture** (inspired by Robert C. Martin, "Uncle Bob"). This architectural style ensures maximum **separation of concerns**, **testability**, and **independence** from external frameworks and databases.

## 1. Core Principles

The primary principle is the **Dependency Rule**: Code dependencies must point inwards. Inner layers should never know about outer layers.

* **Independent of Frameworks:** The core business logic (Domain and Application) does not depend on ASP.NET Core, Entity Framework, or external UI libraries.
* **Independent of Database:** The Application layer defines contracts (Interfaces/Repositories), and the Infrastructure layer implements them. This makes the system independent of the specific database (SQL Server, Postgre, etc.).
* **Testable:** Business logic can be unit-tested without needing to spin up a web server, database, or external service.




## 2. Layer Functions

| Layer | Responsibility | Dependencies | Core Components |
| :--- | :--- | :--- | :--- |
| **Domain (The Core)** | Defines **Business Rules** and **Entities**. It's the heart of the system. | None (Pure C#) | Entities (`Aircraft`, `Booking`), Value Objects, Enums, Interfaces for Repositories (`IBookingRepository`). |
| **Application** | Defines **Use Cases** (business workflows). Coordinates data flow between layers. | Domain | Services (`IBookingService`), DTOs, Business Rules, Mappers (AutoMapper Profiles). |
| **Infrastructure** | Implements contracts from Application and Domain. **Handles external details**. | Application, Domain, External Libraries (EF Core, MailKit, Stripe) | `ApplicationDbContext`, Repository Implementations (`BookingRepository`), External Services (`EmailService`, `PaymentsService`). |
| **Presentation (The API)** | **Entry Point** of the application. Handles HTTP requests/responses, authorization, and configuration. | Infrastructure, Application, Domain | Controllers (`AirportsController`), Extensions (`SwaggerServicesExtension`), `Program.cs`, Middleware. |

## 3. The Flow of Control (Request Lifecycle)

1.  **Request** (Presentation): An HTTP request hits a Controller (e.g., `AirportsController`).
2.  **Validation** (Presentation/Application): The Controller validates the request DTO.
3.  **Use Case Execution** (Application): The Controller calls a method on an Application Service (`IAirportService.CreateAirportAsync`).
4.  **Data Access** (Application -> Infrastructure): The Service calls a contract (e.g., `IUnitOfWork.Airports.AddAsync`). The Infrastructure layer's implementation (`AirportRepository`) uses **Entity Framework Core** to interact with the database.
5.  **Response** (Application -> Presentation): The result (DTO or `ServiceResult`) is returned to the Controller.
6.  **Response** (Presentation): The Controller formats the response (JSON) and returns the appropriate HTTP status code.
 
-----
 