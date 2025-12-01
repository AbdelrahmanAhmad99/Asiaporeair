# EF Core Migrations Management

**Asiaporeair** uses Entity Framework Core's Code-First Migrations to manage the database schema. This approach allows developers to define the schema using C# classes (Entities) and propagate changes to the SQL Server database in a controlled, versioned manner.

## 1. Migration Philosophy

* **Code-First:** The `Domain` and `Infrastructure` projects define the schema, not the database itself.
* **Version Control:** Every schema change is captured in a dedicated Migration file, ensuring database evolution is tracked and reversible.
* **Automated Deployment:** Migrations are applied automatically upon application startup in the Development environment (via `Program.cs`) and are part of the CI/CD pipeline for Production.

## 2. Standard Migration Workflow

### 2.1. Creating a New Migration

Whenever an Entity is modified (new property, new entity, relationship change), a new migration file must be generated:

```bash
# Ensure you are in the Solution Directory (.sln folder)
# 'Infrastructure' is the project containing the DbContext and Migrations folder.
# 'Presentation' is the startup project used to resolve dependencies.

dotnet ef migrations add [DescriptiveNameOfChange] \
    --project Infrastructure \
    --startup-project Presentation
	
```