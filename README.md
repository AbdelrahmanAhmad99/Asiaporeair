# Asiaporeair Documentation

[![Documentation](https://img.shields.io/badge/Documentation%20Site-Technical%20Docs-1976D2?style=for-the-badge&logo=markdown)](http://127.0.0.1:8000/Asiaporeair/)
[![GitHub repo](https://img.shields.io/badge/GitHub-Source%20Code-181717?style=for-the-badge&logo=github)](https://github.com/AbdelrahmanAhmad99/Asiaporeair.git)
[![Live Schema](https://img.shields.io/badge/Database%20Schema-dbdocs.io%20(Live)-CC2927?style=for-the-badge&logo=postgresql)](https://dbdocs.io/abdelrahmanahmed606ky/Asiapore-Airline-Database-Schema?view=relationships)
[![API Reference](https://img.shields.io/badge/API%20Reference-Apidog%20Hosted%20Docs-FF9800?style=for-the-badge&logo=apidog)](https://share.apidog.com/762445f2-1d68-4894-b68b-6f4479003291)
[![Booking Lifecycle](https://img.shields.io/badge/Major%20Use%20Case-Booking%20Lifecycle%20Doc-8E24AA?style=for-the-badge&logo=processwire)](http://127.0.0.1:8000/Asiaporeair/technical/api/booking_sequence/)
[![Service Map](https://img.shields.io/badge/Service%20Map-Dependency%20Flow%20(UML)-00897B?style=for-the-badge&logo=dependabot)](http://127.0.0.1:8000/Asiaporeair/technical/services/dependency_map/)
## I. Project Overview  

**Asiaporeair** is a simple project supports both a high-traffic **Customer Booking Engine System (CBES)** and a secure **Airport Operations Management System (AOMS)** interface. Developed as a training project, simulates the complex operational environment of a major international airline, inspired by some Asia Airlines standards.
**Core Vision Asiaporeair** is a large-scale, backend system designed to manage a major Asian airline's entire operational and commercial ecosystem.
It is built upon a **Clean Architecture** principle to ensure **high scalability**, **maintainability**, and **domain independence**, separating core business logic from external dependencies (DB, UI, frameworks). 
 
  
 
## II. Getting Started for Developers 
Get the **Asiaporeair** backend running locally in three quick steps.

---

### Local Setup & Prerequisites (Recommended)
Configure your environment (IDE, .NET SDK, SQL Server) and run the application directly for active development. **[Setup & Prerequisites](technical/start/setup.md)**
 
### Application Configuration
Update mandatory settings like the **Database Connection String** and API secrets before the first run and deployment.  **[Configuration Guide](technical/start/config.md)**
 
### Data Seeding
Use the automated large-scale seeding strategy to populate the database with all necessary foundational and test data.   **[Data Seeding Strategy](technical/start/seeding.md)**
 
---

## III. Documentation Roadmap
This documentation is a living asset. Use the sidebar navigation to explore deeper into the project's inner workings:

<div class="grid cards" markdown>


-   :material-sitemap:{ .lg .middle } __System Architecture__

    ---

    **Deep Dive into Design Principles**
      <small> Understand the foundational structure of Asiaporeair, from the **Clean Architecture** layers to the core domain model and authorization flow. </small>
    <br>
    [:material-view-list-outline: Clean Architecture Deep Dive](technical/arch/clean_arch.md)
	<br>
    [:material-account-lock-outline: User Roles & Authorization Flow](technical/arch/auth_flow.md)
    {: .card-grid-item .card-grid-item-2 }
 
-   :material-database-sync:{ .lg .middle } __Data Persistence__

    ---

    **Database Integrity and Schema Reference**
    <small> Explore the Entity-Relationship Diagram (**ERD**), the comprehensive schema reference, and the EF Core **migrations strategy** used for data integrity.</small>
    <br>
    [:material-database-marker: Entity-Relationship Diagram (ERD)](technical/database/erd.md)
	 <br>
    [:material-table-search: Data Seeding Strategy](technical/database/seeding.md)
    {: .card-grid-item .card-grid-item-2 }
 
-   :material-briefcase-outline:{ .lg .middle } __Business Services__

    ---

    **Core Domain Logic and Service Contracts**
    <small> Detailed documentation of all primary business interfaces, including booking, ticketing, flight scheduling, and pricing services.</small>
    <br>
    [:material-map-marker-path: Service Dependency Map](technical/services/dependency_map.md)
	 <br>
    [:material-calendar-clock: Flight Scheduling & Operations](technical/services/flight_ops.md)
    {: .card-grid-item .card-grid-item-2 }

-   :material-cloud-sync-outline:{ .lg .middle } __API Reference & Guides__

    ---

    **Endpoints, Use Cases, and Integration**
    <small> Reference for all REST endpoints (**Public** and **Admin**), the full **Booking Lifecycle**, and **Postman collection setup** for testing. </small>
    <br>
    [:material-api: Public API (Customer Facing)](technical/api/public_controllers.md)
	 <br>
    [:material-lock-check: Admin API (Management)](technical/api/admin_controllers.md)
	 <br>
    [:material-timeline-text: Booking Lifecycle Sequence](technical/api/booking_sequence.md)
    {: .card-grid-item .card-grid-item-2 }

</div>

  
  
--- 
 
## IV. Key Features & Core System Capabilities
 
The Asiaporeair system is structured around independent, high-cohesion business services (`IServices`) to provide robust functionality across two main operational domains and one foundational domain:

### A. Commercial & Customer Facing (Public API)  

This domain represents the primary revenue engine, offering a secure and seamless digital experience via the Public API endpoints.

* :material-account-star-outline: **Secure Identity Management**
    
	<small> **Value: Security and High-Performance Authorization** | The system employs a rigorous security model using **JWT Bearer Tokens** for stateless authorization, supporting Multi-Factor Authentication (MFA) and profile updates (`IAuthService`). </small>

* :material-ticket-confirmation-outline: **Comprehensive Booking Engine**
    
	<small> **Value: Maximizing Conversion & Operational Flow** | This engine manages the entire multi-passenger booking lifecycle, including dynamic seat assignment (`ISeatService`), advanced flight search, and final ticket issuance (`ITicketService`). </small>

* :material-credit-card-scan-outline: **Secure Payments & Transaction Integrity**
    
	<small> **Value: Compliance & Financial Reliability** | Ensures secure and compliant transactions via direct **Stripe API** integration (`IPaymentsService`), supporting robust transaction logging and automated refunds. </small>

* :material-bag-checked: **Revenue Generation (Ancillary)**
    
	<small> **Value: Strategic Upselling and Dynamic Pricing** | Facilitates the dynamic display and sale of ancillary products (baggage, meals, lounge access) (`IAncillaryService`), supported by real-time fare rule retrieval and contextual pricing calculation. </small>

* :material-medal-outline: **Loyalty & Retention**
    
	<small> **Value: Enhancing Customer Lifetime Value (CLV)** | Includes core services (`IFrequentFlyerService`) for managing Frequent Flyer profiles, point accumulation, and redemption processes to enhance long-term customer loyalty. </small>

* :material-email-outline: **Critical Communication & Notifications**
    
	<small> **Value: User Trust and Service Delivery** | Utilizes secure libraries (MailKit/MimeKit) to manage all transactional communications, including booking confirmations, ticket delivery, and password resets, ensuring reliable service delivery (`IEmailService`). </small>

* :material-file-lock-outline: **Secure Asset Storage**
    
	<small> **Value: Data Segregation and Security** | Implements secure file system services (`IFileService`) to manage and serve non-database assets, such as user profile pictures and document uploads, with segregated access control. </small>
 
***

### B. Airline Operations & Management (Admin API) 

Accessed via dedicated, secure Admin API endpoints, this module empowers internal staff, crew, and administrators to maintain operational efficiency and strategic control.

* :material-calendar-clock: **Real-time Flight Operations**
    
	<small> **Value: Operational Agility and Asset Control** | Enables precise management of all flight instances, including scheduling (`IFlightSchedulingService`), status updates, delays, aircraft changes, and terminal/gate assignment (`IFlightOperationsService`). </small>

* :material-human-male-board: **HR and Crew Resource Optimization**
    
	<small> **Value: Compliance and Resource Utilization** | Manages employee profiles, tracks pilot/attendant certifications, ensures compliance with flight hour limits, and utilizes optimized crew scheduling (`ICrewSchedulingService`). </small>

* :material-cog-outline: **Administrative Control & Asset Management**
    
	<small> **Value: Centralized Control and Configuration** | Provides SuperAdmins with high-level access for user role management and centralized configuration of core assets, including Airports, Routes, and Aircraft Configurations (`IAircraftService`). </small>
 
* :material-airplane-edit: **Fleet & Aircraft Inventory Management**
    
	<small> **Value: Inventory Control and Maintenance Planning** | Manages the internal fleet inventory (Aircraft entities), detailed seat map configurations, and defines aircraft models and suitability checks via (`IAircraftManagementService` and `IAircraftTypeService`). </small>

* :material-map-marker-path: **Network & Route Definition**
    
	<small> **Value: Foundational Data Integrity** | Manages core reference data for all operational regions, defining flight routes (origin-destination pairs) and assigning specific operating airlines to each route (`IRouteManagementService`, `IAirportService`). </small>

* :material-chart-line: **Strategic Revenue Management**
    
	<small> **Value: Dynamic Pricing and BI Data Feed** | Implements and manages **Contextual Pricing Rules** (`IContextualPricingService`) based on demand and capacity, coupled with comprehensive price offer logging for business intelligence (BI) analysis. </small>

* :material-barcode: **Ground Operations & Check-in**
    
	<small> **Value: Streamlining Airport Experience** | Streamlines the airport experience by managing airport gates, terminals, and handling the Check-in process and digital boarding pass issuance (`IBoardingPassService`). </small>

* :material-monitor-dashboard: **Business Intelligence (BI) & Monitoring**
    
	<small> **Value: Data-Driven Decision Making** | Generates critical operational reports (Load Factor Analysis, Sales Summary) (`IReportingService`) and retrieves real-time, aggregated metrics for the centralized management dashboard (`DashboardController.cs`). </small>

***

### C. Data Foundation & Infrastructure 

This layer ensures a robust, performance-tested data environment, critical for enterprise-level operational scale and integrity.

* :material-database-marker: **Schema Integrity and Design**
    
	<small> **Goal: Guaranteed Data Consistency and Optimized Query Performance** | Comprehensive SQL Schema (`AsiaporeairDb`) detailing all entities, optimized indices, and Foreign Key constraints for robust referential integrity. </small>
 
* :material-database-export: **High-Volume Data Seeding**
    
	<small> **Goal: Performance Benchmarking and Reliable Production Scale Testing** | A professional, dependency-ordered seeding strategy (using EF Core + JSON) to automatically populate all tables with necessary foundational and test data prior to deployment. </small>
 
----
 
## V. Technology Stack - Asiaporeair Enterprise Solution 
<small> This section outlines the complete **Technology Stack** adopted for building the integrated Asiaporeair solutions, focusing on efficiency, security, and enterprise-grade **Scalability**. </small>

<div class="grid cards" markdown>

- :material-cube-outline:{ .lg .middle } __Software Architecture__

    ---

    * **Clean Architecture**
         <small> Implemented for strict **Separation of Concerns**, testability, and isolated Domain Layer, enhancing **maintainability**. </small>
    * **Repository Pattern & UoW**
         <small> Patterns used to abstract data access, enhancing **modularity** and **Testability** by decoupling the application from the persistence framework. </small>
    {: .card-grid-item .card-grid-item-2 }

- :material-language-csharp:{ .lg .middle } __Core Backend Framework__

    ---

    * **.NET 9 (C#)**
         <small> High-performance, cross-platform framework for robust enterprise development, utilizing the power of **C#** and modern runtime features. </small>
    * **ASP.NET Core Web API**
         <small> Foundational layer for scalable, efficient **RESTful API** service development, supporting dependency injection (DI) and asynchronous operations. </small>
    {: .card-grid-item .card-grid-item-2 }

- :material-database-sync:{ .lg .middle } __Data Management and Persistence__

    ---

    * **SQL Server**
         <small> Enterprise-grade **RDBMS** for reliability, **transactional integrity**, and robust data security across all operational domains. </small>
    * **Entity Framework Core (EF Core)**
         <small> Standard **ORM** for simplified data access, supporting **Code First Migrations** for consistent database schema evolution. </small>
    * **ASP.NET Core Identity**
         <small> Integrated identity system for managing user registration, **Roles**, and secure profile data management. </small>
    * **Data Seeding Mechanism**
         <small> Custom mechanisms (`DataSeedExtension.cs`) for reliable, dependency-ordered initial population of core and reference data. </small>
    {: .card-grid-item .card-grid-item-2 }

- :material-security:{ .lg .middle } __Security and Authorization__

    ---

    * **JWT Bearer Tokens**
         <small> Industry-standard, **stateless** authentication method for securing all API endpoints and managing high-volume traffic efficiently. </small>
    * **Role- and Claim-Based Auth**
         <small> Precise control over resource access based on assigned **Roles and Claims** (`[Authorize(Roles = "...")`) for granular administrative control. </small>
    {: .card-grid-item .card-grid-item-2 }

- :material-tools:{ .lg .middle } __Architecture and Development Utilities__

    ---

    * **AutoMapper**
         <small> Convention-based library for efficient **Object Mapping** between Entity models and **DTOs**, simplifying the application layer code. </small>
    * **Swagger (OpenAPI)**
         <small> Automatic generation of interactive, standardized **API documentation** for testing and discovery, using Swashbuckle. </small>
    * **Structured Logging**
         <small> Utilization of `Microsoft.Extensions.Logging` for deep diagnostics, error tracking, and real-time operational monitoring. </small>
    * **Custom API Response Handlers**
         <small> Unified response structures (e.g., `ApiResponse`) ensuring consistency and predictability in all success and error messages. </small>
    {: .card-grid-item .card-grid-item-2 }

- :material-link-variant:{ .lg .middle } __External System Integration__

    ---

    * **Stripe API Integration**
         <small> Direct integration with **Stripe** for secure, compliant payment processing and automated refund management. </small>
    * **MailKit / MimeKit**
         <small> Modern libraries for secure creation and transmission of critical emails (confirmations, password resets) via **SMTP** with **StartTls**. </small>
    * **Local File Storage**
         <small> Use of local file system services (`IWebHostEnvironment`) to securely manage and serve uploaded files (e.g., profile pictures) (`IFileService`). </small>
    {: .card-grid-item .card-grid-item-2 }

</div>
 
---