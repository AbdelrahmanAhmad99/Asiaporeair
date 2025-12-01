using Application.DTOs.Flight;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Application.Models;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    //  service for customer-facing flight search, availability, and details.
    public class FlightService : IFlightService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPricingService _pricingService;
        private readonly IFlightOperationsService _flightOperationsService;  
        private readonly ILogger<FlightService> _logger;
        private readonly IMapper _mapper;

        public FlightService(
            IUnitOfWork unitOfWork,
            IPricingService pricingService,
            IFlightOperationsService flightOperationsService,  
            ILogger<FlightService> logger,  
            IMapper mapper)  
        {
            _unitOfWork = unitOfWork;
            _pricingService = pricingService;
            _flightOperationsService = flightOperationsService;
            _logger = logger;
            _mapper = mapper;
        }
         
        // Performs a comprehensive search for flights (One-Way, Round-Trip, Multi-City).
        public async Task<ServiceResult<IEnumerable<FlightItineraryDto>>> SearchFlightsAsync(FlightSearchRequestDto searchRequest)
        {
            _logger.LogInformation("Starting flight search for {SearchType} from {Origin} to {Destination}.",
                searchRequest.SearchType,
                searchRequest.Segments.First().OriginIataCode,
                searchRequest.Segments.First().DestinationIataCode);

            if (searchRequest.Passengers.TotalPassengers == 0)
                return ServiceResult<IEnumerable<FlightItineraryDto>>.Failure("Search request must include at least one adult or child.");

            try
            {
                // For this example, we'll implement One-Way. RoundTrip/MultiCity would call this method iteratively.
                if (searchRequest.SearchType != "OneWay")
                {
                    _logger.LogWarning("Search type {SearchType} not yet fully implemented, defaulting to OneWay logic.", searchRequest.SearchType);
                    // In a real app, RoundTrip would search outbound, then inbound, and combine them.
                    // MultiCity would iterate searchRequest.Segments and find options for each.
                }

                var firstSegment = searchRequest.Segments.First();

                // 1. Build search filter for flight instances
                var (instances, error) = await FindFlightInstancesAsync(firstSegment.OriginIataCode, firstSegment.DestinationIataCode, firstSegment.DepartureDate);
                if (error != null)
                    return ServiceResult<IEnumerable<FlightItineraryDto>>.Failure(error);

                var itineraries = new List<FlightItineraryDto>();

                // 2. Iterate through found flight instances
                foreach (var instance in instances)
                {
                    // 3. Check seat availability
                    var availabilityResult = await CheckSeatAvailabilityAsync(instance, searchRequest.Passengers.TotalPassengers);
                    if (!availabilityResult.IsAvailable)
                    {
                        _logger.LogWarning("Skipping FlightInstance {InstanceId}: {Reason}", instance.InstanceId, availabilityResult.Reason);
                        continue;
                    }

                    // 4. Get fare options for this flight
                    var fareOptionsResult = await GetFareOptionsForFlightInternalAsync(instance.InstanceId, searchRequest.Passengers.TotalPassengers, searchRequest.CabinClassPreference);
                    if (!fareOptionsResult.IsSuccess || !fareOptionsResult.Data.Any())
                    {
                        _logger.LogWarning("Skipping FlightInstance {InstanceId}: No valid fare options found.", instance.InstanceId);
                        continue;
                    }

                    // 5. Build the itinerary DTO
                    var segmentDto = _mapper.Map<FlightSegmentDto>(instance);

                    var itinerary = new FlightItineraryDto
                    {
                        ItineraryId = Guid.NewGuid().ToString(),
                        OutboundSegments = new List<FlightSegmentDto> { segmentDto },
                        FareOptions = fareOptionsResult.Data.ToList(),
                        TotalDurationMinutes = segmentDto.DurationMinutes,
                        NumberOfStops = 0 // This logic would be more complex for connecting flights
                    };

                    itineraries.Add(itinerary);
                }

                if (!itineraries.Any())
                {
                    _logger.LogInformation("No available flights found for {Origin}-{Destination} on {Date}.",
                        firstSegment.OriginIataCode, firstSegment.DestinationIataCode, firstSegment.DepartureDate.Date);
                    return ServiceResult<IEnumerable<FlightItineraryDto>>.Failure("No flights found matching your criteria.");
                }

                _logger.LogInformation("Successfully found {Count} flight itineraries.", itineraries.Count);
                return ServiceResult<IEnumerable<FlightItineraryDto>>.Success(itineraries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during SearchFlightsAsync for {Origin}-{Destination}.",
                    searchRequest.Segments.First().OriginIataCode, searchRequest.Segments.First().DestinationIataCode);
                return ServiceResult<IEnumerable<FlightItineraryDto>>.Failure("An internal error occurred while searching for flights.");
            }
        }

        // Retrieves detailed information for a specific flight instance.
        public async Task<ServiceResult<FlightDetailsDto>> GetFlightDetailsAsync(int flightInstanceId)
        {
            _logger.LogInformation("Retrieving flight details for InstanceId {InstanceId}.", flightInstanceId);
            try
            {
                var flight = await _unitOfWork.FlightInstances.GetWithDetailsAsync(flightInstanceId);
                if (flight == null)
                {
                    _logger.LogWarning("Flight details not found for InstanceId {InstanceId}.", flightInstanceId);
                    return ServiceResult<FlightDetailsDto>.Failure("Flight details not found.");
                }

                var detailsDto = _mapper.Map<FlightDetailsDto>(flight);

                // Get Cabin Class details and availability
                var aircraftConfigs = await _unitOfWork.AircraftConfigs.GetByAircraftAsync(flight.AircraftId);
                var activeConfig = aircraftConfigs.FirstOrDefault(c => !c.IsDeleted);

                if (activeConfig != null)
                {
                    var cabinClasses = await _unitOfWork.CabinClasses.GetByConfigurationAsync(activeConfig.ConfigId);
                    foreach (var cabin in cabinClasses)
                    {
                        var totalSeats = await _unitOfWork.Seats.CountAsync(s => s.CabinClassId == cabin.CabinClassId && !s.IsDeleted);
                        var bookedSeats = await _unitOfWork.BookingPassengers.GetPassengerCountForCabinAsync(flightInstanceId, cabin.CabinClassId);

                        detailsDto.CabinClasses.Add(new CabinClassAvailabilityDto
                        {
                            CabinClassId = cabin.CabinClassId,
                            Name = cabin.Name,
                            TotalSeats = totalSeats,
                            AvailableSeats = totalSeats - bookedSeats
                            // 'Layout' would likely be another property on CabinClass entity
                        });
                    }
                }

                return ServiceResult<FlightDetailsDto>.Success(detailsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for InstanceId {InstanceId}.", flightInstanceId);
                return ServiceResult<FlightDetailsDto>.Failure("An error occurred while retrieving flight details.");
            }
        }

        // Retrieves the available fare options (e.g., Lite, Standard, Flex) for a specific flight.
        public async Task<ServiceResult<IEnumerable<FlightFareOptionDto>>> GetFareOptionsForFlightAsync(int flightInstanceId, int totalPassengers)
        {
            _logger.LogInformation("Getting fare options for InstanceId {InstanceId} for {Passengers} passengers.", flightInstanceId, totalPassengers);
            var result = await GetFareOptionsForFlightInternalAsync(flightInstanceId, totalPassengers, null);

            if (!result.IsSuccess)
                return ServiceResult<IEnumerable<FlightFareOptionDto>>.Failure(result.Errors);

            return ServiceResult<IEnumerable<FlightFareOptionDto>>.Success(result.Data);
        }

        // Public-facing method to check the real-time status of a flight by its number and date.
        public async Task<ServiceResult<FlightSegmentDto>> GetFlightStatusByNumberAsync(FlightStatusRequestDto statusRequest)
        {
            
            _logger.LogInformation("Checking flight status for {FlightNumber} on {Date}.", statusRequest.FlightNumber, statusRequest.FlightDate);
            try
            {
                // 1. Find the schedule(s) for this flight number
                var schedules = await _unitOfWork.FlightSchedules.FindByFlightNumberAsync(statusRequest.FlightNumber);
                if (!schedules.Any())
                    return ServiceResult<FlightSegmentDto>.Failure($"Flight {statusRequest.FlightNumber} not found in schedule.");

                // 2. Find the *specific instance* for that date
                var date = statusRequest.FlightDate.Date;
                var nextDate = date.AddDays(1);

                Expression<Func<FlightInstance, bool>> filter = fi =>
                    fi.Schedule.FlightNo == statusRequest.FlightNumber &&
                    fi.ScheduledDeparture >= date &&
                    fi.ScheduledDeparture < nextDate &&
                    !fi.IsDeleted;

                var instances = await _unitOfWork.FlightInstances.SearchAsync(filter);
                var instance = instances.FirstOrDefault(); // Should only be one

                if (instance == null)
                {
                    _logger.LogWarning("Flight {FlightNumber} found in schedule, but no instance exists for {Date}.", statusRequest.FlightNumber, date);
                    return ServiceResult<FlightSegmentDto>.Failure($"Flight {statusRequest.FlightNumber} is not scheduled to operate on {date:yyyy-MM-dd}.");
                }

                // 3. Map the instance to the segment DTO, which includes status
                var segmentDto = _mapper.Map<FlightSegmentDto>(instance);
                 
                // The mapper already set DepartureTime to ScheduledDeparture.
                // We override it ONLY if an ActualDeparture time exists.
                if (instance.ActualDeparture.HasValue)
                    segmentDto.DepartureTime = instance.ActualDeparture.Value;

                // Also check actual arrival
                if (instance.ActualArrival.HasValue)
                    segmentDto.ArrivalTime = instance.ActualArrival.Value;

                return ServiceResult<FlightSegmentDto>.Success(segmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking status for {FlightNumber}.", statusRequest.FlightNumber);
                return ServiceResult<FlightSegmentDto>.Failure("An error occurred while checking flight status.");
            }
        }

        // Quick check for seat availability for a specific flight.
        public async Task<ServiceResult<bool>> CheckFlightAvailabilityAsync(int flightInstanceId, int passengerCount)
        {
            var instance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(flightInstanceId);
            if (instance == null)
                return ServiceResult<bool>.Failure("Flight instance not found.");

            var availability = await CheckSeatAvailabilityAsync(instance, passengerCount);

            if (!availability.IsAvailable)
                return ServiceResult<bool>.Failure(availability.Reason);

            return ServiceResult<bool>.Success(true);
        }

        // --- Private Helper Methods ---

        // Internal helper to find flight instances based on route and date.
        private async Task<(IEnumerable<FlightInstance> Instances, string? Error)> FindFlightInstancesAsync(string origin, string dest, DateTime date)
        {
            try
            {
                var searchDate = date.Date;
                var nextDate = searchDate.AddDays(10);

                // Use the repository to search for active instances
                var instances = await _unitOfWork.FlightInstances.SearchAsync(
                    f => f.ScheduledDeparture >= searchDate &&
                         f.ScheduledDeparture < nextDate && 
                         f.Schedule.Route.OriginAirport.IataCode == origin && 
                         f.Schedule.Route.DestinationAirport.IataCode == dest &&  
                         f.Status != "Cancelled" && // Don't show cancelled flights
                         !f.IsDeleted);

                return (instances.OrderBy(f => f.ScheduledDeparture), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find flight instances in repository for {Origin}-{Dest}.", origin, dest);
                return (new List<FlightInstance>(), "Database error while searching for flights.");
            }
        }

        // Internal helper to check seat availability.
        private async Task<(bool IsAvailable, string Reason)> CheckSeatAvailabilityAsync(FlightInstance instance, int passengers)
        {
            try
            {
                int? totalSeats = instance.Aircraft?.AircraftType?.MaxSeats;
                if (totalSeats == null || totalSeats == 0)
                {
                    // Fallback to aircraft config if main type has no seat count
                    var config = (await _unitOfWork.AircraftConfigs.GetByAircraftAsync(instance.AircraftId)).FirstOrDefault();
                    if (config?.TotalSeatsCount.HasValue == true)
                        totalSeats = config.TotalSeatsCount;
                    else
                        return (false, "Aircraft configuration not found or seat count is zero.");
                }

                // Get count of all passengers booked on this flight
                int bookedSeats = await _unitOfWork.BookingPassengers.GetPassengerCountForFlightAsync(instance.InstanceId);

                if ((bookedSeats + passengers) > totalSeats)
                {
                    return (false, $"Not enough seats available. Required: {passengers}, Available: {totalSeats - bookedSeats}");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking seat availability for InstanceId {InstanceId}.", instance.InstanceId);
                return (false, "Error checking seat availability.");
            }
        }
         
        // Internal helper to get all valid fare options for a flight.
        private async Task<ServiceResult<List<FlightFareOptionDto>>> GetFareOptionsForFlightInternalAsync(int instanceId, int totalPassengers, string? cabinPreference)
        {
            var fareOptions = new List<FlightFareOptionDto>();

            // 1. Fetch ALL active fare basis codes from the database (Professional approach) 
            var allActiveFareBasis = await _unitOfWork.FareBasisCodes.GetAllActiveAsync();

            if (!allActiveFareBasis.Any())
            {
                _logger.LogWarning("No active fare basis codes found in the database.");
                return ServiceResult<List<FlightFareOptionDto>>.Failure("No active fare options defined by the system.");
            }

            // 2. Filter fares based on the requested cabin preference (if provided)
            // We trim and set to null if empty to simplify filtering logic.
            var cabin = string.IsNullOrWhiteSpace(cabinPreference) ? null : cabinPreference.Trim();

            // Select relevant fares. We rely on the GetCabinClassFromCode helper to map the code to a cabin class.
            var relevantFares = allActiveFareBasis.Where(fare =>
            {
                // Use the helper to determine the cabin class of the fare code
                var fareCabinClass = GetCabinClassFromCode(fare.Code);

                // If no preference is given (cabin == null), include all fares.
                if (cabin == null) return true;

                // Match the requested cabin (case-insensitive)
                return fareCabinClass.Equals(cabin, StringComparison.OrdinalIgnoreCase);
            }).ToList();

            if (!relevantFares.Any())
            {
                _logger.LogInformation("No active fare basis codes matched the requested cabin preference: {Cabin}", cabinPreference);
                return ServiceResult<List<FlightFareOptionDto>>.Failure($"No fare options found for cabin {cabinPreference}.");
            }

            // 3. Check availability and pricing for each relevant fare
            // Fetch total and booked seat counts once using IUnitOfWork for efficiency
            var seatCountsResult = await _unitOfWork.FlightInstances.GetSeatCountsAsync(instanceId);
            int totalAvailableSeats = seatCountsResult.TotalCapacity - seatCountsResult.BookedSeats;

            foreach (var fareBasis in relevantFares)
            {
                var code = fareBasis.Code;

                // 3a. Check seat availability (incorporating simplified "Fare Bucket" logic)
                // Using the calculated value:
                int availableSeats = totalAvailableSeats;

                // Simulating Fare Buckets: Assume restricted categories have fewer dedicated seats
                // This is a necessary simulation until a true 'CabinConfiguration' table is used.
                if (code.Contains("RESTR", StringComparison.OrdinalIgnoreCase) || code.Contains("PROM", StringComparison.OrdinalIgnoreCase))
                {
                    // Max available seats for this code is 50 (example)
                    availableSeats = Math.Min(availableSeats, 50);
                }

                if (availableSeats < totalPassengers)
                {
                    _logger.LogInformation("Skipping fare code {Code}: Insufficient seats available ({Available} < {Required}).", code, availableSeats, totalPassengers);
                    continue;
                }

                // 3b. Get the dynamic price
                var priceResult = await _pricingService.CalculateBasePriceAsync(instanceId, code);
                if (!priceResult.IsSuccess)
                {
                    _logger.LogWarning("Could not calculate price for InstanceId {InstanceId} with Fare {Code}: {Error}", instanceId, code, priceResult.Errors.FirstOrDefault());
                    continue; // Skip if pricing fails
                }

                // 3c. Parse the actual rules from the database
                var rules = ParseFareRules(fareBasis.Rules);

                // 3d. Create the Data Transfer Object (DTO)
                var fareDto = new FlightFareOptionDto
                {
                    FareBasisCode = code,
                    FareName = fareBasis.Description,
                    CabinClass = GetCabinClassFromCode(code), // Helper function to accurately determine cabin class
                    PricePerAdult = priceResult.Data,
                    PricePerChild = priceResult.Data * 0.8m,
                    PricePerInfant = priceResult.Data * 0.1m,
                    AvailableSeats = availableSeats,

                    // Using the actual parsed rules
                    BaggageAllowance = rules.Baggage,
                    IsChangeable = rules.IsChangeable,
                    IsRefundable = rules.IsRefundable
                };

                fareOptions.Add(fareDto);
            }

            // Final check: If no fares were found
            if (!fareOptions.Any())
            {
                return ServiceResult<List<FlightFareOptionDto>>.Failure("No valid fare options found matching criteria.");
            }

            return ServiceResult<List<FlightFareOptionDto>>.Success(fareOptions);
        }

        // Helper function to determine cabin class based on the fare code (usually the first character)
        // This logic is crucial for filtering and remains necessary.
        private string GetCabinClassFromCode(string fareCode)
        {
            var firstChar = fareCode.Length > 0 ? fareCode.ToUpper()[0] : ' ';

            switch (firstChar)
            {
                case 'F':
                case 'A':
                case 'X':
                    return "FirstClass";
                case 'J':
                case 'C':
                case 'D':
                case 'B':
                    return "Business";
                case 'P':
                case 'R':
                case 'W':
                    return "PremiumEconomy";
                default:
                    return "Economy";
            }
        }

        // Helper function for parsing rules (still needed)
        private (bool IsChangeable, bool IsRefundable, string Baggage) ParseFareRules(string rules)
        {
            var lowerRules = rules.ToLower();
            // Simple improvement for 'Changeable' and 'Refundable' condition to avoid errors (e.g., matching "no changeable")
            var isChangeable = lowerRules.Contains("changeable") && !lowerRules.Contains("no changeable");
            var isRefundable = lowerRules.Contains("refundable") && !lowerRules.Contains("no refunds");

            // Simple example for baggage analysis
            string baggage = "23kg";
            if (lowerRules.Contains("no baggage") || lowerRules.Contains("0kg")) baggage = "0kg";
            else if (lowerRules.Contains("40kg")) baggage = "40kg";
            else if (lowerRules.Contains("30kg")) baggage = "30kg";

            return (isChangeable, isRefundable, baggage);
        }

         
        // These are kept for compatibility but should be replaced by the new SearchFlightsAsync.

        // This is the original method from the user's file, renamed for clarity.
        public async Task<ServiceResult<IEnumerable<FlightSearchResultDto>>> LegacySearchFlightsAsync(LegacyFlightSearchDto searchDto)
        {
            // Calculate total passengers to filter by available seats
            var totalPassengers = searchDto.NumberOfAdults + searchDto.NumberOfChildren;

            // Use the repository to search for flights
            var flights = await _unitOfWork.FlightInstances.SearchAsync(
                f => f.ScheduledDeparture.Date == searchDto.DepartureDate.Date &&
                     f.Schedule.Route.OriginAirportId == searchDto.OriginIataCode && // Corrected: Use ID
                     f.Schedule.Route.DestinationAirportId == searchDto.DestinationIataCode && // Corrected: Use ID
                     !f.IsDeleted); // Added check

            var availableFlights = new List<FlightSearchResultDto>();
            foreach (var flight in flights)
            {
                // Placeholder for seat availability check.
                // This would be replaced by a real check on the SeatRepository.
                var availableSeats = 100;

                if (availableSeats >= totalPassengers)
                {
                    // Corrected: Handling the ServiceResult<decimal> return type
                    var basePriceResult = await _pricingService.CalculateBasePriceAsync(flight.InstanceId, "Economy"); // Assuming "Economy"
                    if (!basePriceResult.IsSuccess)
                    {
                        // Log or handle the error appropriately
                        // For now, we'll just skip this flight if price calculation fails
                        _logger.LogWarning("LegacySearch: Price calculation failed for {InstanceId}", flight.InstanceId);
                        continue;
                    }

                    var basePrice = basePriceResult.Data;

                    // This DTO is now legacy, replaced by FlightItineraryDto
                    availableFlights.Add(new FlightSearchResultDto
                    {
                        FlightInstanceId = flight.InstanceId,
                        FlightNumber = flight.Schedule.FlightNo,
                        AirlineName = flight.Schedule.Airline.Name,
                        DepartureTime = flight.ScheduledDeparture,
                        ArrivalTime = flight.ScheduledArrival,
                        OriginAirportIata = flight.Schedule.Route.OriginAirport.IataCode,
                        DestinationAirportIata = flight.Schedule.Route.DestinationAirport.IataCode,
                        BasePrice = basePrice,
                        AvailableSeats = availableSeats
                    });
                }
            }

            if (!availableFlights.Any())
            {
                return ServiceResult<IEnumerable<FlightSearchResultDto>>.Failure("No flights found for the specified criteria.");
            }

            return ServiceResult<IEnumerable<FlightSearchResultDto>>.Success(availableFlights);
        }

        
        // Renaming original DTOs to avoid conflicts
        public class LegacyFlightSearchDto
        {
            public string OriginIataCode { get; set; } = string.Empty;
            public string DestinationIataCode { get; set; } = string.Empty;
            public DateTime DepartureDate { get; set; }
            public int NumberOfAdults { get; set; }
            public int NumberOfChildren { get; set; }
        }

        public class FlightSearchResultDto
        {
            public int FlightInstanceId { get; set; }
            public string FlightNumber { get; set; } = string.Empty;
            public string AirlineName { get; set; } = string.Empty;
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public string OriginAirportIata { get; set; } = string.Empty;
            public string DestinationAirportIata { get; set; } = string.Empty;
            public decimal BasePrice { get; set; }
            public int AvailableSeats { get; set; }
        }
    }
}
  
   