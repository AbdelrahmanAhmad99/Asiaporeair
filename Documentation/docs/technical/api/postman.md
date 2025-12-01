# Postman Collection Setup

To facilitate rapid testing and development, we provide a comprehensive **Postman Collection** covering all Public and Admin API endpoints.

## 1. Prerequisites

* Download and install [Postman](https://www.postman.com/downloads/).
* Ensure the API is running locally (Default: `https://localhost:5001`).

## 2. Importing the Collection

1.  Locate the file `Asiaporeair.postman_collection.json` in the root `docs` folder.
2.  Open Postman -> **File** -> **Import** -> Upload the file.

## 3. Environment Variables

We use environment variables to manage tokens and URLs dynamically. Create a new Environment in Postman (e.g., "Asiaporeair Local") with the following variables:

| Variable | Initial Value | Current Value | Description |
| :--- | :--- | :--- | :--- |
| `baseUrl` | `https://localhost:5001` | `https://localhost:5001` | API Root URL |
| `auth_token` | (Leave Empty) | (Leave Empty) | JWT Token (Auto-filled by scripts) |
| `admin_token` | (Leave Empty) | (Leave Empty) | JWT Token for Admin Users |

## 4. Usage Workflow

### Step 1: Authenticate

1.  Open the request `Auth / Login (User)`.
2.  Send the request with valid credentials (e.g., seeded user).
3.  **Note:** The collection contains a **Test Script** that automatically saves the response `token` to the `{{auth_token}}` variable.

### Step 2: Test Protected Endpoints

1.  Open any protected request (e.g., `Booking / Create Booking`).
2.  Ensure the **Authorization** tab is set to `Inherit auth from parent` or `Bearer Token` using `{{auth_token}}`.
3.  Send the request.
 