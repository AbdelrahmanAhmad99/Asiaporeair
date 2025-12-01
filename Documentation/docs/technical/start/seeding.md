# Data Seeding Strategy 

The **Asiaporeair** project is designed with a robust and decoupled data seeding strategy to ensure the development environment is complete and realistic upon the first run.

## 1. Seeding Purpose 

This process is essential for:

* **Creating Core Users:** Such as the default **SuperAdmin** user for initial login.
* **Creating User Roles:** Ensuring standard roles (`Admin`, `Supervisor`, `User`) exist in the `AspNetRoles` table.
* **Populating Reference Data:** Inserting static and vital data for system operation, such as: Global Airport Codes (Airports), Aircraft Types,Countries lists and Cabin Classes ,etc .

## 2. Execution Mechanism 

The seeding process runs automatically upon application startup, immediately after database migrations are applied, and is organized as follows:

1.  **Location:** The core seeding logic is defined in the extension file **`DataSeedExtension.cs`** (in the `Presentation` layer).
2.  **Central Orchestrator:** The `AsiaporeairDataSeed` service acts as the central orchestrator. This service is responsible for calling individual Seeder services in the correct sequence to ensure data dependency integrity.
3.  **Individual Seeder Services:** Each type of reference data is populated by a dedicated `Seeder` service (e.g., `AncillaryProductSeeder`, `AirportSeeder`, `CountrySeeder`).
    * These services rely on reading structured raw data from **JSON** files to ensure easy data updates and realism.

## 3. How It Runs 

Once the application is run via **Visual Studio** after applying migrations (as detailed in `setup.md`):

* The extension method **`app.SeedDatabaseAsync()`** is called from the `Program.cs` file.
* The orchestrator executes the seeding process sequentially, starting with core data (Roles), then reference data (Countries, Airports), and finally more complex data.
* If data already exists in the table, most **Seeders** skip the addition process to avoid duplication.

> **Verification of Success:** You can monitor the application logs (Console Logs) during startup. The message: **`--- Asiaporeair Database Seeding Completed Successfully! ---`** will appear upon successful completion.