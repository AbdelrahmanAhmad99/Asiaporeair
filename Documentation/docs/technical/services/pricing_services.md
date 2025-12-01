# Pricing & Ancillary Services

## 1. Overview

This document provides a high-level business logic reference for the core services responsible for dynamic pricing, fare management, and the sale of optional ancillary products within the **Asiaporeair** ecosystem.

These services reside within the `Application.Services` layer and enforce core revenue management rules, ensuring prices are dynamic, auditable, and contextually relevant. They utilize the Unit of Work pattern (`IUnitOfWork`) to guarantee transactional integrity during pricing and sales operations.

* **Pricing Service (`IPricingService`)**
* **Contextual Pricing Service (`IContextualPricingService`)**
* **Fare Basis Code Service (`IFareBasisCodeService`)**
* **Ancillary Product Service (`IAncillaryProductService`)**
* **Price Offer Log Service (`IPriceOfferLogService`)**

---

## 2. Pricing Service (`IPricingService`)

The Pricing Service is the central component for calculating the final price of flight components (fares and seats) by integrating base costs with dynamic, contextual adjustments. It acts as an **Anti-Corruption Layer (ACL)**, protecting the core booking process from complex and volatile yield management logic.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`CalculateBasePriceAsync`** | **Core Base Fare Calculation.** Calculates the dynamic, final fare for a single passenger on a specific flight instance, based on the selected `FareBasisCode`. |
| | **1. Base Price Retrieval:** Fetches the initial fare amount and details associated with the provided `FareBasisCode`. |
| | **2. Contextual Adjustment Integration:** Calls `IContextualPricingService` to retrieve all applicable dynamic pricing rules and multipliers (e.g., seasonal adjustments, day-of-week factors). |
| | **3. Final Calculation:** Applies the contextual multipliers and adjustments to the base price to determine the final, quoted fare. |
| **`CalculateSeatPriceAsync`** | **Ancillary Seat Pricing Logic (Yield Management).** Calculates the dynamic price for a specific, pre-assigned seat selection. |
| | **1. Seat Base Cost:** Retrieves the initial cost associated with the seat's `Zone` and `Class` (e.g., Exit Row, Premium, Standard). |
| | **2. Seat Premium Multiplier:** Applies a fixed premium (e.g., 1.5x) if the seat is classified as a premium option (e.g., Exit Row, extra legroom). |
| | **3. Occupancy-Based Dynamic Pricing (Yield Factor):** Implements a critical Yield Management rule by adjusting the price based on the flight's current load factor (occupancy). **Policy:** If occupancy > 75%, apply **1.4x** multiplier; if occupancy > 90%, apply **2.0x** multiplier. |

---

## 3. Contextual Pricing Service (`IContextualPricingService`)

This service manages the data and logic for dynamic pricing factors. It allows revenue management teams to configure market-driven price adjustments (like promotional multipliers or demand factors) without deploying code.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`GetAttributeSetByIdAsync(int attributeId)`** | Retrieves a specific, active rule set (e.g., "Weekend Demand Surcharge") for administrative review. |
| **`CreateAttributeSetAsync`** | **Admin/Staff only.** Creates a new contextual pricing rule set. **Authorization:** Restricted to high-level pricing management roles. **Validation:** Must ensure the combination of context parameters (e.g., time until departure range, length of stay) is valid and does not conflict with existing rules. |
| **`UpdateAttributeSetAsync`** | **Admin/Staff only.** Modifies an existing rule set. Requires authorization. |
| **`GetMatchingAttributesAsync`** | **Rule Retrieval Engine.** The core function used by `IPricingService` to retrieve all applicable dynamic pricing rules based on the current search/booking context. |
| | **1. Days to Departure Match:** Queries for rules that match the remaining time until the flight's departure date. |
| | **2. Length of Stay Match:** Queries for rules that match the passenger's length of stay (for round-trip bookings), prioritizing the closest upper-bound match. |
| | **3. Aggregation:** Returns a distinct collection of all active rules whose criteria are met by the provided context. |

---

## 4. Fare Basis Code Service (`IFareBasisCodeService`)

This service manages the official fare classifications and their associated statutory and commercial rules. This is the foundation for all flight pricing.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`GetFareByCodeAsync`** | Retrieves the full definition of an active fare basis code by its unique string code (e.g., 'Y1', 'KEEPN01'). |
| | **Function:** Provides the **Description** (e.g., "Full Economy, Refundable"), **Rules** (e.g., "Change fee applies, 100% refund"), and the base price/multiplier for the `PricingService`. |
| **`CreateFareAsync`** | **Admin/Staff only.** Creates a new fare basis code definition. **Policy:** The code must be unique and defined with comprehensive, compliant rules text. |
| **`UpdateFareAsync`** | **Admin/Staff only.** Modifies the description, rules, or core base price/multiplier of an existing fare basis code. **Audit:** Changes to core fare rules are logged for regulatory compliance. |
| **`DeleteFareAsync`** | **Admin/Staff only.** Performs a **Soft-Delete** on a fare basis code. **Business Rule:** The fare is flagged as inactive (`IsDeleted = true`) to prevent use in new bookings, but the record is preserved to maintain integrity for historical bookings. |
| **`ReactivateFareAsync`** | **Admin/Staff only.** Reactivates a previously soft-deleted fare code, making it available again for use in pricing. |
| **`GetAllFaresIncludingDeletedAsync`** | **Reporting/Management.** Retrieves all fare codes, including inactive (soft-deleted) codes, for a complete regulatory and business overview. |

---

## 5. Ancillary Product Service (`IAncillaryProductService`)

This service manages the official catalog and sales transactions for all optional extra products and services offered by Asiaporeair (non-fare items).

| Method | Business Logic and Rules |
| :--- | :--- |
| **`GetProductByIdAsync`** | Retrieves a single active ancillary product (e.g., checked bag allowance, meal option, lounge access). |
| **`GetAllProductsAsync`** | Retrieves the current list of all active ancillary products available for sale to customers. |
| **`CreateProductAsync`** | **Admin/Staff only.** Creates a new ancillary product definition. **Validation:** Must include a unique product name, price, and a defined `AncillaryType` (e.g., Baggage, Meal, Seat). |
| **`UpdateProductAsync`** | **Admin/Staff only.** Modifies the details or price of an existing ancillary product. |
| **`DeleteProductAsync`** | **Admin/Staff only.** Soft-deletes an ancillary product, making it unavailable for new sales while preserving existing sales records. |
| **`AddAncillaryToBookingAsync`** | **Ancillary Sales Transaction Flow.** Processes the sale and attachment of ancillary items to an existing booking. |
| | **1. Authorization:** Confirms the user has the authority to modify the booking. Requires a successful check via `AuthorizeBookingAccessAsync` (owner or admin/supervisor). |
| | **2. Validation:** Ensures the selected ancillary products exist, are active, and adhere to per-passenger/per-flight business rules (e.g., one hot meal per segment). |
| | **3. Transaction:** Creates `AncillarySale` records, calculates the subtotal, and atomically updates the overall `Booking.TotalPrice` and saves the changes within the Unit of Work. |

---

## 6. Price Offer Log Service (`IPriceOfferLogService`)

The Price Offer Log Service is an essential component for Revenue Management and compliance auditing, providing a historical record of all dynamic price quotes presented to customers.

| Method | Business Logic and Rules |
| :--- | :--- |
| **`LogPriceOfferAsync`** | **Audit/Quote Capture.** Records a specific price offer made to a customer (or internal process) before the final payment. |
| | **Validation:** Enforces a strong schema where the log entry must be linked to *exactly one* source (either a `FareBasisCode` or an `AncillaryProduct`). |
| | **Purpose:** Used to analyze price elasticity, track the effectiveness of contextual pricing rules, and support audit trails for potential pricing disputes. |
| **`GetLogByIdAsync`** | Retrieves a single price offer log entry for detailed review. |
| **`SearchLogsAsync`** | **Reporting/Analytics.** Supports querying the log by criteria such as date range, price, linked fare code, or flight instance ID. |
| **`ReactivateLogAsync`** | **Management/Recovery.** Allows an authorized user to reverse a soft-delete on a log entry, ensuring the completeness of the audit trail for auditing purposes. |