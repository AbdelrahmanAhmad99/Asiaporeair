using Application.DTOs.Booking;
using Application.Models;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging; 
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    // Professional implementation of the dynamic pricing service
    public class PricingService : IPricingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PricingService> _logger;
        private readonly IContextualPricingService _contextualPricingService; // Added service dependency
         
        public PricingService(IUnitOfWork unitOfWork, ILogger<PricingService> logger, IContextualPricingService contextualPricingService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _contextualPricingService = contextualPricingService;
        }

        // Calculates the base price for one passenger on one flight
        public async Task<ServiceResult<decimal>> CalculateBasePriceAsync(int flightInstanceId, string fareBasisCode)
        {
            try
            {
                // 1. Get Flight, Route, and Aircraft data
                var flightInstance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(flightInstanceId);
                if (flightInstance == null)
                {
                    _logger.LogWarning("CalculateBasePrice failed: Flight instance {FlightInstanceId} not found.", flightInstanceId);
                    return ServiceResult<decimal>.Failure("Flight instance not found.");
                }

                // 2. Get Fare Rule data
                var fareCode = await _unitOfWork.FareBasisCodes.GetByCodeAsync(fareBasisCode);
                if (fareCode == null)
                {
                    _logger.LogWarning("CalculateBasePrice failed: Fare basis code {FareCode} not found.", fareBasisCode);
                    return ServiceResult<decimal>.Failure("Fare basis code not found.");
                }
                 
                // 3. Get Contextual Factors
                var daysUntilDeparture = (flightInstance.ScheduledDeparture - DateTime.UtcNow).TotalDays;
                var contextDto = new Application.DTOs.ContextualPricingAttribute.PricingContextDto
                {
                    DaysToDeparture = (int)daysUntilDeparture,
                    LengthOfStayDays = 0 // Base price doesn't know return date, 0 for one-way
                };
                // Call the service to find matching rules
                var contextAttributesResult = await _contextualPricingService.FindBestMatchingAttributeSetsAsync(contextDto);

                decimal basePrice;

                // 4. Determine Base Price (Contextual vs. Fallback)
                // Try to get price from Contextual Rules first
                if (contextAttributesResult.IsSuccess && contextAttributesResult.Data.Any(a => a.WillingnessToPay.HasValue))
                {
                    // Use the highest "WillingnessToPay" found from matching rules
                    // This assumes WillingnessToPay is the absolute base fare for this context
                    basePrice = contextAttributesResult.Data
                        .Where(a => a.WillingnessToPay.HasValue)
                        .Max(a => a.WillingnessToPay.Value);
                    _logger.LogDebug("Base Price (Contextual): {Price}", basePrice);
                }
                else
                {
                    // Fallback to Base Price (e.g., based on distance)
                    var distance = flightInstance.Schedule?.Route?.DistanceKm ?? 1000;
                    basePrice = (decimal)distance * 0.15m; // 15 cents per km
                    _logger.LogDebug("Base Price (Fallback - Distance): {Price}", basePrice);
                }
 
                // 5. Apply Fare Class Multiplier (Modification)
                decimal fareClassMultiplier = 1.0m;
                string upperFareBasisCode = fareBasisCode.ToUpperInvariant(); // Ensure case-insensitivity

                //  Multiplier Logic based on the new extended Fare Basis Codes:

                // 1. First Class (Highest Multiplier)
                if (upperFareBasisCode.Contains("FLEX") || upperFareBasisCode.Contains("FRESTR") || upperFareBasisCode.Contains("XGOV"))
                {
                    // Codes starting with F (FLEX, FRESTR) or X (XGOV) are First Class
                    fareClassMultiplier = 4.0m;
                }
                // 2. Business Class
                else if (upperFareBasisCode.Contains("JFLX") || upperFareBasisCode.Contains("BRESTR") || upperFareBasisCode.Contains("DFLEX") || upperFareBasisCode.Contains("ZRESTR"))
                {
                    // Codes starting with J or B or D or Z are Business Class
                    fareClassMultiplier = 2.5m;
                }
                // 3. Premium Economy Class
                else if (upperFareBasisCode.Contains("PFLX") || upperFareBasisCode.Contains("PRESTR") || upperFareBasisCode.Contains("WPREM") || upperFareBasisCode.Contains("NPROM"))
                {
                    // Codes starting with P or W or N are Premium Economy
                    fareClassMultiplier = 1.5m;
                }
                // 4. Economy Class (Standard Multiplier and Variations)
                else if (upperFareBasisCode.Contains("YFLX") || upperFareBasisCode.Contains("MRESTR") || upperFareBasisCode.Contains("VPROM"))
                {
                    // Y, M, V and other Economy specific codes
                    fareClassMultiplier = 1.2m; // Standard Economy/Flexible Multiplier
                }
                else
                {
                    // Default or ultra-restricted/deep discount (Lowest Multiplier)
                    fareClassMultiplier = 1.0m;
                }
                 

                basePrice *= fareClassMultiplier;
                _logger.LogDebug("Price after Fare Class ({FareCode}): {Price}", fareBasisCode, basePrice);

                 
                // 6. Contextual Factor: Occupancy (Load Factor)
                decimal occupancyFactor = 1.0m;
                var aircraftType = flightInstance.Aircraft?.AircraftType;
                if (aircraftType?.MaxSeats.HasValue == true && aircraftType.MaxSeats > 0)
                {
                    int totalSeats = aircraftType.MaxSeats.Value;
                    int bookedSeats = await _unitOfWork.BookingPassengers.GetPassengerCountForBookingAsync(flightInstanceId); // Assumes this method exists
                    double occupancy = (double)bookedSeats / totalSeats;

                    if (occupancy > 0.9) occupancyFactor = 2.0m; // > 90% full
                    else if (occupancy > 0.75) occupancyFactor = 1.4m; // > 75% full

                    basePrice *= occupancyFactor;
                    _logger.LogDebug("Price after Occupancy ({Booked}/{Total}): {Price}", bookedSeats, totalSeats, basePrice);
                }

                // 7. Log this quoted price for analytics
                // We use a simplified context ID (or 0 if none found)
                var contextId = contextAttributesResult.Data?.FirstOrDefault()?.AttributeId ?? 0;
                await LogPriceOfferAsync(flightInstanceId, fareBasisCode, basePrice, contextId);

                return ServiceResult<decimal>.Success(Math.Round(basePrice, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in CalculateBasePriceAsync for {FlightInstanceId}, {FareCode}.", flightInstanceId, fareBasisCode);
                return ServiceResult<decimal>.Failure("An internal error occurred while calculating the price.");
            }
        }

        // Calculates the final total price for an entire booking.
        public async Task<ServiceResult<decimal>> CalculateBookingPriceAsync(CreateBookingDto bookingDto)
        {
            var passengersCount = bookingDto.Passengers.Count;
            if (passengersCount == 0)
            {
                _logger.LogWarning("CalculateBookingPrice failed: No passengers in DTO for flight {FlightInstanceId}.", bookingDto.FlightInstanceId);
                return ServiceResult<decimal>.Failure("Cannot calculate price for zero passengers.");
            }

            // 1. Get Base Flight Price
            var basePriceResult = await CalculateBasePriceAsync(bookingDto.FlightInstanceId, bookingDto.FareBasisCode);
            if (!basePriceResult.IsSuccess)
            {
                _logger.LogWarning("CalculateBookingPrice failed: Could not get base price for flight {FlightInstanceId}.", bookingDto.FlightInstanceId);
                return ServiceResult<decimal>.Failure(basePriceResult.Errors);
            }

            decimal totalFlightPrice = basePriceResult.Data * passengersCount;
            decimal totalAncillaryPrice = 0;

            // 2. Calculate Ancillary Product Costs (if they exist on the DTO)
            if (bookingDto.AncillaryPurchases != null && bookingDto.AncillaryPurchases.Any())
            {
                foreach (var purchase in bookingDto.AncillaryPurchases)
                {
                    var product = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(purchase.ProductId);
                    if (product != null)
                    {
                        totalAncillaryPrice += (product.BaseCost ?? 0) * purchase.Quantity;
                    }
                    else
                    {
                        _logger.LogWarning("CalculateBookingPrice: Ancillary product {ProductId} not found, skipping.", purchase.ProductId);
                    }
                }
            }

            decimal finalTotalPrice = totalFlightPrice + totalAncillaryPrice;
            _logger.LogInformation("Calculated total booking price: {TotalPrice} (Flights: {FlightPrice}, Ancillaries: {AncillaryPrice})", finalTotalPrice, totalFlightPrice, totalAncillaryPrice);

            return ServiceResult<decimal>.Success(finalTotalPrice);
        }

        // Logs a price quote to the database for analysis.
        public async Task<ServiceResult> LogPriceOfferAsync(int flightInstanceId, string fareCode, decimal quotedPrice, int contextId)
        {
            try
            {
                var logEntry = new PriceOfferLog
                {
                    ProductId = null, // This is for a flight fare, not an ancillary
                    OfferPriceQuote = quotedPrice,
                    Timestamp = DateTime.UtcNow,
                    ContextAttributesId = contextId,
                    FareId = fareCode,
                    AncillaryId = null,
                    IsDeleted = false
                };

                await _unitOfWork.PriceOfferLogs.AddAsync(logEntry);
                await _unitOfWork.SaveChangesAsync(); // Commit the log

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                // Do not fail the main operation (like search) if logging fails
                _logger.LogWarning(ex, "Failed to log price offer for FlightInstance {FlightInstanceId}, Fare {FareCode}.", flightInstanceId, fareCode);
                return ServiceResult.Failure("Failed to log price offer."); // ServiceResult.Success() is also fine if logging is non-critical
            }
        }
         

        // Calculates the price for selecting a *specific seat* (e.g., exit row)
        public async Task<ServiceResult<decimal?>> CalculateSeatPriceAsync(string seatId, int flightInstanceId)
        {
            _logger.LogDebug("Calculating specific seat price for SeatId {SeatId} on Flight {FlightId}", seatId, flightInstanceId);
            try
            {
                // 1. Get Seat Details 
                var seat = await _unitOfWork.Seats.GetWithCabinClassAsync(seatId);
                if (seat == null)
                {
                    return ServiceResult<decimal?>.Failure("Seat not found.");
                }

                // 2. --- Seat Pricing Logic (Example) ---
                // The default price is 0 (or null)
                decimal? price = null;

                // Rule 1: Business and First Class seats are free to choose
                if (seat.CabinClass.Name.Contains("Business") || seat.CabinClass.Name.Contains("First"))
                {
                    price = null; // Free
                }
                // Rule 2: Emergency exit (exit row) seats have an additional cost
                else if (seat.IsExitRow == true) // Using the field from the database
                {
                    price = 120.00m; // Fixed price for emergency exit
                }
                // Rule 3: "Extra Space" Seats (we assume they are in "Premium Economy")
                else if (seat.CabinClass.Name.Contains("Premium Economy"))
                {
                    price = 50.00m;
                }
                // Rule 4: The front seats in "Economy"
                else if (seat.CabinClass.Name.Contains("Economy") && int.TryParse(seat.SeatNumber.Trim('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'K'), out int row) && row < 30)
                {
                    price = 30.00m; // Price of front seats
                }



                // 3. --- Activate contextual pricing (implement the TODO) --- 
                // Only apply multipliers if the seat has a base price (i.e., not free)
                
                if (price.HasValue && price.Value > 0)
                {
                    _logger.LogDebug("Applying contextual multipliers to base seat price of {Price}", price.Value);

                    // 3a. Bringing in the context of the journey
                    var flightInstance = await _unitOfWork.FlightInstances.GetActiveByIdAsync(flightInstanceId);
                    if (flightInstance == null)
                    {
                        _logger.LogWarning("Could not find FlightInstance {FlightId} for seat price context.", flightInstanceId);
                        // Continue at the base price if we don't find the flight
                    }
                    else
                    {
                        // 3b. Applying the "Time Remaining" multiplier
                        var daysUntilDeparture = (flightInstance.ScheduledDeparture - DateTime.UtcNow).TotalDays;
                        var contextDto = new Application.DTOs.ContextualPricingAttribute.PricingContextDto
                        {
                            DaysToDeparture = (int)daysUntilDeparture,
                            LengthOfStayDays = 0 // The price of the seat does not depend on the length of stay
                        };
                        // Invoke the contextual service
                        var contextAttributesResult = await _contextualPricingService.FindBestMatchingAttributeSetsAsync(contextDto);

                        // Simple example: If the flight is nearby, increase the price.
                        // (You can make this more complex by fetching a "price multiplier" from the database.)
                        if (contextAttributesResult.IsSuccess && daysUntilDeparture < 7)
                        {
                            price *= 1.5m; // 50% increase in seats in the last week
                            _logger.LogDebug("Price after Time Factor (<7 days): {Price}", price.Value);
                        }

                        // 3c. Applying the "Occupancy" multiplier
                        var (totalCapacity, bookedSeats) = await _unitOfWork.FlightInstances.GetSeatCountsAsync(flightInstanceId);
                        if (totalCapacity > 0)
                        {
                            double occupancy = (double)bookedSeats / totalCapacity;
                            if (occupancy > 0.9) // > 90% full
                            {
                                price *= 2.0m; // Double the price
                                _logger.LogDebug("Price after Occupancy (>90%): {Price}", price.Value);
                            }
                            else if (occupancy > 0.75) // > 75% full
                            {
                                price *= 1.4m; // 40% increase
                                _logger.LogDebug("Price after Occupancy (>75%): {Price}", price.Value);
                            }
                        }
                    }
                }
                
                if (price.HasValue)
                {
                    _logger.LogInformation("Calculated price for SeatId {SeatId} is {Price}", seatId, price.Value);
                }

                return ServiceResult<decimal?>.Success(price);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price for SeatId {SeatId}", seatId);
                return ServiceResult<decimal?>.Failure(ex.Message);
            }
        }
      

    }
}
 