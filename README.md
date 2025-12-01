# Asiaporeair Documentation

[![Documentation](https://img.shields.io/badge/Documentation%20Site-Technical%20Docs-1976D2?style=for-the-badge&logo=markdown)](https://abdelrahmanahmad99.github.io/Asiaporeair/)
[![GitHub repo](https://img.shields.io/badge/GitHub-Source%20Code-181717?style=for-the-badge&logo=github)](https://github.com/AbdelrahmanAhmad99/Asiaporeair.git)
[![Live Schema](https://img.shields.io/badge/Database%20Schema-dbdocs.io%20(Live)-CC2927?style=for-the-badge&logo=postgresql)](https://dbdocs.io/abdelrahmanahmed606ky/Asiapore-Airline-Database-Schema?view=relationships)
[![API Reference](https://img.shields.io/badge/API%20Reference-Apidog%20Hosted%20Docs-FF9800?style=for-the-badge&logo=apidog)](https://share.apidog.com/762445f2-1d68-4894-b68b-6f4479003291)
[![Booking Lifecycle](https://img.shields.io/badge/Major%20Use%20Case-Booking%20Lifecycle%20Doc-8E24AA?style=for-the-badge&logo=processwire)](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/api/booking_sequence/)
[![Service Map](https://img.shields.io/badge/Service%20Map-Dependency%20Flow%20(UML)-00897B?style=for-the-badge&logo=dependabot)](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/services/dependency_map/)
## I. Project Overview  

**Asiaporeair** is a simple project supports both a high-traffic **Customer Booking Engine System (CBES)** and a secure **Airport Operations Management System (AOMS)** interface. Developed as a training project, simulates the complex operational environment of a major international airline, inspired by some Asia Airlines standards.
**Core Vision Asiaporeair** is a large-scale, backend system designed to manage a major Asian airline's entire operational and commercial ecosystem.
It is built upon a **Clean Architecture** principle to ensure **high scalability**, **maintainability**, and **domain independence**, separating core business logic from external dependencies (DB, UI, frameworks). 
 
  
 
## II. Getting Started for Developers 
Get the **Asiaporeair** backend running locally in three quick steps.

---

### Local Setup & Prerequisites (Recommended)
Configure your environment (IDE, .NET SDK, SQL Server) and run the application directly for active development. **[Setup & Prerequisites](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/start/setup/)**
 
### Application Configuration
Update mandatory settings like the **Database Connection String** and API secrets before the first run and deployment.  **[Configuration Guide](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/start/config/)**
 
### Data Seeding
Use the automated large-scale seeding strategy to populate the database with all necessary foundational and test data.   **[Data Seeding Strategy](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/start/seeding/)**
 
---

## III. Documentation Roadmap
This documentation is a living asset. Use the sidebar navigation to explore deeper into the project's inner workings:

---

### System Architecture
**Deep Dive into Design Principles**
*Understand the foundational structure of Asiaporeair, from the **Clean Architecture** layers to the core domain model and authorization flow.*

* [Clean Architecture Deep Dive](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/arch/clean_arch.md)
* [User Roles & Authorization Flow](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/arch/auth_flow.md)

---

### Data Persistence
**Database Integrity and Schema Reference**
*Explore the Entity-Relationship Diagram (**ERD**), the comprehensive schema reference, and the EF Core **migrations strategy** used for data integrity.*

* [Entity-Relationship Diagram (ERD)](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/database/erd.md)
* [Data Seeding Strategy](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/database/seeding.md)

---

### Business Services
**Core Domain Logic and Service Contracts**
*Detailed documentation of all primary business interfaces, including booking, ticketing, flight scheduling, and pricing services.*

* [Service Dependency Map](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/services/dependency_map.md)
* [Flight Scheduling & Operations](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/services/flight_ops.md)

---

### API Reference & Guides
**Endpoints, Use Cases, and Integration**
*Reference for all REST endpoints (**Public** and **Admin**), the full **Booking Lifecycle**, and **Postman collection setup** for testing.*

* [Public API (Customer Facing)](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/api/public_controllers.md)
* [Admin API (Management)](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/api/admin_controllers.md)
* [Booking Lifecycle Sequence](https://abdelrahmanahmad99.github.io/Asiaporeair/technical/api/booking_sequence.md)

---