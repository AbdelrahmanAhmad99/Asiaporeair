# Application Configuration  

All application settings rely on the built-in ASP.NET Core configuration system, primarily stored in the `appsettings.json` (General/Production mode) and `appsettings.Development.json` (Development mode) files.

## 1. Database Configuration 

The main connection string is located under the `ConnectionStrings` key. It must be updated to link the project to your local SQL Server instance:

**File: `appsettings.Development.json`**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AsiaporeairDB; Integrated Security = SSPI ;MultipleActiveResultSets=true; TrustServerCertificate = True;"
  }
}
```

> **Important:** If you are using a different database name or authentication method (such as SQL Login), update the string accordingly.

## 2\. Security Settings 

### 2.1. JWT Configuration  

JWT signature and authentication settings must be defined to enable the `AuthService` interface to issue valid tokens.

**File: `appsettings.json`**

```json
{
  "JwtSettings": {
    "Secret": "A_VERY_LONG_AND_SECURE_SECRET_KEY_FOR_JWT_SIGNATURE_256_BITS_MIN",
    "Issuer": "AsiaporeairIssuer",
    "Audience": "AsiaporeairUsers",
    "DurationInDays": 7
  }
}
```

> **Warning:** The `Secret` key must be long and complex. Use a unique key for the development environment and do not use it for production.

## 3\. External Services Configuration  

### 3.1. Stripe Configuration (Payments) 

We use **Stripe** for payment processing. You will need your **Test Keys** from the Stripe Dashboard:

**File: `appsettings.Development.json`**

```json
{
  "StripeSettings": {
    "SecretKey": "sk_test_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "PublishableKey": "pk_test_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
	"WebhookSecret": "whsec_test_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
	"Currency": "usd",
    "SuccessUrl": "http://localhost:4200/payments/success",
    "CancelUrl": "http://localhost:4200/payments/cancel"
  }
}
```


### 3.2. Stripe CLI Setup 

For local development and testing of asynchronous events (like webhooks for successful payments), we use the **Stripe CLI** to forward events securely to the application.

#### Set up a local listener

Run the following steps using the Stripe CLI:

1.  **Download the Stripe CLI and log in with your Stripe account**
    ```bash
    stripe login
    ```
2.  **Forward events to your destination**
    ```bash
    stripe listen --forward-to https://localhost:5001/api/v1/webhook/stripe
    ```
    > **Note:** The `https://localhost:5150/api/v1/webhook/stripe` endpoint must match the actual Webhook listener route in the Presentation layer. The command above will output a local `whsec_...` secret key that must be copied to the `WebhookSecret` field in the `appsettings.Development.json` file for the application to validate webhook signatures.
3.  **Trigger events with the CLI (for testing)**
    ```bash
    stripe trigger payment_intent.succeeded --add "payment_intent:metadata[BookingId]=..."
    ```
	```bash
    stripe trigger payment_intent.payment_failed --add "payment_intent:metadata[BookingId]=..."
    ```


### 3.3. Email Service Configuration (SMTP)  

The email service (via MailKit) is used for sending credentials and booking confirmations.

**File: `appsettings.json`**

```json
{
  "EmailSettings": {
     "Email": "Asiapore@Asiaporeair.com", 
     "DisplayName": "Asiaporeair Service",
     "Password": "Asiaporeair-app-password",  
     "Host": "smtp.gmail.com", 
     "Port": 587,
     "FrontendBaseUrl": "http://localhost:4200" (Angular/React) 
  }
}
```

> **Tip:** It is recommended to use services like **SendGrid** or **Mailtrap** for secure testing instead of your personal email.

 
