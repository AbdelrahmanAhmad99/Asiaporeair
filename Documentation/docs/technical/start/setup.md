# Quick Start: Setup and Prerequisites (Manual Local Setup)

This guide walks you through the step-by-step process of setting up the local development environment for the **Asiaporeair** project and running it directly via your Integrated Development Environment (IDE).

## 1. Prerequisites 

Before you begin, ensure the following components are installed on your machine:

| Component | Recommended Version | Notes |
| :--- | :--- | :--- |
| **.NET SDK** | **.NET 9** (or later) | Required to build and run ASP.NET Core applications. |
| **SQL Server** | **SQL Server 2019 / 2022** or **LocalDB** | The main relational database for the project. |
| **IDE** | **Visual Studio 2022** or **Visual Studio Code** | Preferred IDE (with .NET tools installed). |
| **Git** | Latest Version | To clone the source code repository. |

## 2. Initial Setup Steps 

### Step 2.1: Clone the Repository 

Use the following command to clone the project source code:

```bash
git clone [https://github.com/.../Asiaporeair]
cd Asiaporeair
```
 
### Step 2.2: Restore NuGet Packages

This crucial step ensures that all external libraries and dependencies required for the project's operation are downloaded and correctly linked to the solution. The **Asiaporeair** project relies on a modern set of enterprise-grade packages, all defined within the various project files (`.csproj`).

**Method 1: Command Line (Recommended)**

Navigate to the root directory of the solution (the folder containing the `.sln` file) and execute the standard .NET restore command:

```bash
dotnet restore
```

This command scans all project files and retrieves the required NuGet packages, ensuring consistency across environments.

**Method 2: Visual Studio IDE**

Alternatively, if you are working within **Visual Studio 2022**:

1.  Right-click on the Solution (the top-level item) in the **Solution Explorer**.
2.  Select **Restore NuGet Packages**.
3.  For troubleshooting or explicit management, you can use the **Tools** -\> **NuGet Package Manager** -\> **Package Manager Console**.

Upon completion, all necessary dependencies will be available, including the key packages utilized for the **Technology Stack**:

  * **Data Persistence:** `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, and EF Core Tools (`.Design`, `.Tools`) for migrations.
  * **Security & APIs:** `Microsoft.AspNetCore.Authentication.JwtBearer` (for JWT tokens) and `Swashbuckle.AspNetCore` (for Swagger/OpenAPI documentation).
  * **Utilities:** `AutoMapper` (for efficient **DTO** mapping).
  * **External Services:** `Stripe.net` (for payment integration), and `MailKit` (for email/SMTP services).

The system is now ready to interact with the database.

### Step 2.3: Database Setup 

We assume you have a local SQL Server instance ready. You must create an empty database named **AsiaporeairDB** (or any name you prefer, updating the connection string later).

> **Configuration Alert :** Before proceeding, you **must** update the database connection string in `appsettings.Development.json` to point to your newly created SQL Server instance. Refer to the **Application Configuration** guide for details: [database-configuration](/Asiaporeair/technical/start/config/).
### Step 2.4: Apply EF Core Migrations 

The Code First Migrations technique is used to create the database schema. Ensure you are inside the folder containing the Solution File, then apply the migrations:

```bash
# Change the infrastructure folder path to your infrastructure project name
dotnet ef database update --project Infrastructure --startup-project Presentation
```

> **Note:** This command will create all tables and relationships in the database specified in your connection string.

## 3\. Running the Project

After successfully applying the migrations, you can run the project:

1.  Open the solution in **Visual Studio 2022**.
2.  Ensure the **Presentation** project is set as the **Startup Project**.
3.  Press **F5** (or the Run button) to start the **ASP.NET Core Web API** interface.

> The **Swagger UI** interface will automatically open in your browser (usually at `https://localhost:5001/swagger`), allowing you to test all endpoints.

  