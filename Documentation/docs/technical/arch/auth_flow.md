# User Roles and Authorization Flow

The security of the **Asiaporeair** application is managed using **ASP.NET Core Identity**, enhanced with role-based authorization to strictly separate access between customers, management, and operational staff.

## 1. Defined User Roles

The system uses specific roles to control access to API endpoints and features:

| Role Name | Access Type | Primary Responsibility | Associated Entity |
| :--- | :--- | :--- | :--- |
| **SuperAdmin** | Full CRUD | System maintenance, managing other Admins/Supervisors. | `SuperAdmin` |
| **Admin** | Management CRUD | Managing flight schedules, pricing, and operational data. | `Admin` |
| **Supervisor** | Read-Only/Limited Write | Monitoring, auditing, and limited data entry (e.g., reporting). | `Supervisor` |
| **Pilot** | Read-Only (Flight Data) | Accessing specific flight manifests and crew data. | `Pilot` |
| **Attendant** | Read-Only (Flight Data) | Accessing passenger manifests and cabin information. | `Attendant` |
| **User** | Public/Customer | Booking flights, managing reservations, updating profile. | `User` (Implicitly linked to `Passenger` via `AppUser`) |

## 2. Authentication Flow (JWT)

1.  **Login:** A user sends credentials (`Email`/`Password`) to the `AuthService.LoginAsync()` endpoint.
2.  **Verification:** `UserManager` verifies the credentials.
3.  **Token Generation:** If successful, the `IJwtService` generates a **JWT (JSON Web Token)**. This token contains user claims, including their **User ID** and **Roles** (`Admin`, `User`, etc.).
4.  **Authorization Header:** The client stores the token and sends it in the `Authorization: Bearer {token}` header for all subsequent API requests.

## 3. Authorization Enforcement
Authorization is enforced declaratively at the **Presentation Layer (Controllers)** using the built-in ASP.NET Core attributes.

**Example: Admin-Only Endpoint**

```csharp
[Authorize(Roles = "Admin, SuperAdmin")]
[Route("api/v1/admin/routes")]
public class RoutesController : ControllerBase
{
    // ...
}
```

**Example: Fine-Grained Authorization**
In addition to controller attributes, **role checks are performed inside Application Services** for sensitive operations to ensure business logic security.

```csharp
// Inside IBookingService.CancelBookingAsync(userClaims, bookingId)
if (!userClaims.IsInRole("Admin") && booking.UserId != userClaims.GetUserId())
{
    return ServiceResult.Failure("Unauthorized access to booking.");
}
```
> This combination ensures that unauthorized requests are blocked early at the Controller level, while more complex business constraints are enforced robustly within the Application layer.
