using Application.DTOs.Reporting;
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    // Service implementation for generating reports by aggregating data.
    public class ReportingService : IReportingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportingService> _logger;
        // No IMapper injected, as DTOs are custom-built from aggregates.

        public ReportingService(IUnitOfWork unitOfWork, ILogger<ReportingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // Generates a comprehensive sales and revenue summary.
        public async Task<ServiceResult<SalesSummaryReportDto>> GetSalesSummaryReportAsync(ReportRequestDto request)
        {
            _logger.LogInformation("Generating Sales Summary Report from {StartDate} to {EndDate}.", request.StartDate, request.EndDate);
            try
            {
                // 1. Get all bookings in range
                var allBookings = await _unitOfWork.Bookings.GetByDateRangeAsync(request.StartDate, request.EndDate);
                request.AirlineIataCode = request.AirlineIataCode?.ToUpper();

                // 2. Filter by airline if specified
                if (!string.IsNullOrWhiteSpace(request.AirlineIataCode))
                {
                    allBookings = allBookings.Where(b => b.FlightInstance.Schedule.AirlineId == request.AirlineIataCode);
                }

                var confirmedBookings = allBookings.Where(b => b.PaymentStatus == "Confirmed").ToList();
                var cancelledBookings = allBookings.Where(b => b.PaymentStatus == "Cancelled").ToList();

                // 3. Get ancillary sales for confirmed bookings
                var confirmedBookingIds = confirmedBookings.Select(b => b.BookingId).ToHashSet();
                var ancillarySales = (await _unitOfWork.AncillarySales.GetAllActiveAsync()) // Optimize: GetByBookingIds
                                     .Where(s => confirmedBookingIds.Contains(s.BookingId))
                                     .ToList();

                var passengers = (await _unitOfWork.BookingPassengers.GetAllActiveAsync()) // Optimize: GetByBookingIds
                                   .Where(bp => confirmedBookingIds.Contains(bp.BookingId))
                                   .ToList();

                // 4. Aggregate data
                var report = new SalesSummaryReportDto
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    AirlineIataCode = request.AirlineIataCode,
                    TotalBookingsConfirmed = confirmedBookings.Count,
                    TotalBookingsCancelled = cancelledBookings.Count,
                    TotalPassengers = passengers.Count,
                    TotalBookingRevenue = confirmedBookings.Sum(b => b.PriceTotal ?? 0),
                    TotalAncillaryRevenue = ancillarySales.Sum(s => s.PricePaid ?? 0),

                    BookingsByFareCode = confirmedBookings
                        .GroupBy(b => b.FareBasisCodeId)
                        .ToDictionary(g => g.Key, g => g.Count()),

                    RevenueByFareCode = confirmedBookings
                        .GroupBy(b => b.FareBasisCodeId)
                        .ToDictionary(g => g.Key, g => g.Sum(b => b.PriceTotal ?? 0)),

                    TopRoutesByBookings = confirmedBookings
                        .GroupBy(b => $"{b.FlightInstance.Schedule.Route.OriginAirportId}-{b.FlightInstance.Schedule.Route.DestinationAirportId}")
                        .OrderByDescending(g => g.Count())
                        .Take(10) // Top 10
                        .ToDictionary(g => g.Key, g => g.Count()),

                    TopRoutesByRevenue = confirmedBookings
                        .GroupBy(b => $"{b.FlightInstance.Schedule.Route.OriginAirportId}-{b.FlightInstance.Schedule.Route.DestinationAirportId}")
                        .OrderByDescending(g => g.Sum(b => b.PriceTotal ?? 0))
                        .Take(10)
                        .ToDictionary(g => g.Key, g => g.Sum(b => b.PriceTotal ?? 0)),

                    TopAncillariesByQuantity = ancillarySales
                        .GroupBy(s => s.Product.Name) // Assumes Product is included in GetByBookingAsync
                        .OrderByDescending(g => g.Sum(s => s.Quantity ?? 0))
                        .Take(10)
                        .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity ?? 0))
                };

                report.AverageRevenuePerBooking = report.TotalBookingsConfirmed > 0 ? (report.TotalRevenue / report.TotalBookingsConfirmed) : 0;

                return ServiceResult<SalesSummaryReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Sales Summary Report.");
                return ServiceResult<SalesSummaryReportDto>.Failure("An internal error occurred while generating the sales report.");
            }
        }

        // Generates an operational report on flight performance (delays, cancellations).
        public async Task<ServiceResult<FlightPerformanceReportDto>> GetFlightPerformanceReportAsync(ReportRequestDto request)
        {
            _logger.LogInformation("Generating Flight Performance Report for {Airport} from {StartDate} to {EndDate}.", request.AirportIataCode ?? "All", request.StartDate, request.EndDate);
            try
            {
                var instances = (await _unitOfWork.FlightInstances.GetByScheduledDateRangeAsync(request.StartDate, request.EndDate)).ToList();
                request.AirportIataCode = request.AirportIataCode?.ToUpper();
                request.AirlineIataCode = request.AirlineIataCode?.ToUpper();

                var totalScheduled = instances.Count;
                if (totalScheduled == 0) return ServiceResult<FlightPerformanceReportDto>.Failure("No flights found for the specified date range.");

                int totalCancelled = instances.Count(i => i.Status == "Cancelled");
                int totalOperated = totalScheduled - totalCancelled;

                // Define delay (e.g., > 15 minutes)
                const int delayThresholdMinutes = 15;

                // Filter by airport if provided (affects departure/arrival counts)
                var departureInstances = instances;
                var arrivalInstances = instances;
                if (!string.IsNullOrWhiteSpace(request.AirportIataCode))
                {
                    departureInstances = instances.Where(i => i.Schedule.Route.OriginAirportId == request.AirportIataCode).ToList();
                    arrivalInstances = instances.Where(i => i.Schedule.Route.DestinationAirportId == request.AirportIataCode).ToList();
                }

                // Departure stats
                var relevantDepartures = departureInstances.Where(i => i.Status != "Cancelled").ToList();
                var delayedDepartures = relevantDepartures
                    .Where(i => i.ActualDeparture.HasValue && i.ActualDeparture.Value > i.ScheduledDeparture.AddMinutes(delayThresholdMinutes))
                    .ToList();
                double avgDepartureDelay = delayedDepartures.Any()
                    ? delayedDepartures.Average(i => (i.ActualDeparture.Value - i.ScheduledDeparture).TotalMinutes)
                    : 0;

                // Arrival stats
                var relevantArrivals = arrivalInstances.Where(i => i.Status != "Cancelled").ToList();
                var delayedArrivals = relevantArrivals
                    .Where(i => i.ActualArrival.HasValue && i.ActualArrival.Value > i.ScheduledArrival.AddMinutes(delayThresholdMinutes))
                    .ToList();
                double avgArrivalDelay = delayedArrivals.Any()
                    ? delayedArrivals.Average(i => (i.ActualArrival.Value - i.ScheduledArrival).TotalMinutes)
                    : 0;

                var report = new FlightPerformanceReportDto
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    AirportIataCode = request.AirportIataCode,
                    TotalFlightsScheduled = instances.Count, // Total in system for range
                    TotalFlightsOperated = totalOperated,
                    FlightsCancelled = totalCancelled,
                    TotalDepartures = departureInstances.Count, // Total departures from airport
                    TotalArrivals = arrivalInstances.Count,   // Total arrivals to airport
                    DeparturesDelayed = delayedDepartures.Count,
                    ArrivalsDelayed = delayedArrivals.Count,
                    OnTimeDeparturePercentage = relevantDepartures.Any() ? (double)(relevantDepartures.Count - delayedDepartures.Count) / relevantDepartures.Count * 100 : 100,
                    OnTimeArrivalPercentage = relevantArrivals.Any() ? (double)(relevantArrivals.Count - delayedArrivals.Count) / relevantArrivals.Count * 100 : 100,
                    AverageDepartureDelayMinutes = avgDepartureDelay,
                    AverageArrivalDelayMinutes = avgArrivalDelay
                };

                return ServiceResult<FlightPerformanceReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Flight Performance Report.");
                return ServiceResult<FlightPerformanceReportDto>.Failure("An internal error occurred while generating the performance report.");
            }
        }

        // Generates a report on flight occupancy and load factors.
        public async Task<ServiceResult<LoadFactorReportDto>> GetLoadFactorReportAsync(ReportRequestDto request)
        {
            _logger.LogInformation("Generating Load Factor Report from {StartDate} to {EndDate}.", request.StartDate, request.EndDate);
            try
            {
                // 1. Get operated flights in range
                var instances = (await _unitOfWork.FlightInstances.GetByScheduledDateRangeAsync(request.StartDate, request.EndDate))
                                .Where(i => i.Status != "Cancelled")
                                .ToList();

                if (!instances.Any()) return ServiceResult<LoadFactorReportDto>.Failure("No operated flights found for the specified date range.");

                int totalCapacity = 0;
                int totalPassengers = 0;
                var routeData = new Dictionary<string, (int Flights, int Capacity, int Passengers)>();

                // 2. Iterate each flight to get capacity and passenger count
                foreach (var instance in instances)
                {
                    // Get capacity from AircraftType (requires includes in repo method)
                    int capacity = instance.Schedule.AircraftType?.MaxSeats ?? 0;
                    if (capacity == 0)
                    {
                        _logger.LogWarning("FlightInstance {InstanceId} has AircraftType {Model} with 0 MaxSeats. Skipping for load factor.", instance.InstanceId, instance.Schedule.AircraftType?.Model);
                        continue;
                    }

                    // Get confirmed passenger count
                    int passengers = await _unitOfWork.BookingPassengers.GetPassengerCountForFlightAsync(instance.InstanceId); // Assumes this only counts confirmed

                    totalCapacity += capacity;
                    totalPassengers += passengers;

                    // Aggregate by route
                    string routeName = $"{instance.Schedule.Route.OriginAirportId}-{instance.Schedule.Route.DestinationAirportId}";
                    if (!routeData.ContainsKey(routeName)) routeData[routeName] = (0, 0, 0);
                    routeData[routeName] = (
                        routeData[routeName].Flights + 1,
                        routeData[routeName].Capacity + capacity,
                        routeData[routeName].Passengers + passengers
                    );
                }

                if (totalCapacity == 0) return ServiceResult<LoadFactorReportDto>.Failure("No flights with seat capacity found.");

                // 3. Map route data to DTO
                var routeLoadFactors = routeData.Select(kvp => new RouteLoadFactorDto
                {
                    RouteName = kvp.Key,
                    FlightsOnRoute = kvp.Value.Flights,
                    TotalCapacity = kvp.Value.Capacity,
                    TotalPassengers = kvp.Value.Passengers,
                    LoadFactorPercent = kvp.Value.Capacity > 0 ? (double)kvp.Value.Passengers / kvp.Value.Capacity * 100 : 0
                }).ToList();

                // 4. Build final report
                var report = new LoadFactorReportDto
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    TotalFlightsOperated = instances.Count,
                    TotalCapacityOffered = totalCapacity,
                    TotalPassengersConfirmed = totalPassengers,
                    AverageLoadFactorPercent = (double)totalPassengers / totalCapacity * 100,
                    TopRoutesByLoadFactor = routeLoadFactors.OrderByDescending(r => r.LoadFactorPercent).Take(10).ToList(),
                    BottomRoutesByLoadFactor = routeLoadFactors.OrderBy(r => r.LoadFactorPercent).Take(10).ToList()
                };

                return ServiceResult<LoadFactorReportDto>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Load Factor Report.");
                return ServiceResult<LoadFactorReportDto>.Failure("An internal error occurred while generating the load factor report.");
            }
        }

        // Retrieves the full passenger manifest for a single flight instance.
        public async Task<ServiceResult<PassengerManifestDto>> GetPassengerManifestAsync(int flightInstanceId)
        {
            _logger.LogInformation("Generating Passenger Manifest for FlightInstanceId {FlightId}.", flightInstanceId);
            try
            {
                var instance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(flightInstanceId);
                if (instance == null) return ServiceResult<PassengerManifestDto>.Failure("Flight instance not found.");

                // Get confirmed bookings with full passenger, seat, and ticket details
                var bookings = await _unitOfWork.Bookings.GetByFlightInstanceAsync(flightInstanceId);
                var confirmedBookings = bookings.Where(b => b.PaymentStatus == "Confirmed");
                var tickets = (await _unitOfWork.Tickets.GetByFlightInstanceAsync(flightInstanceId))
                              .ToDictionary(t => t.PassengerId, t => t);

                var passengerList = new List<ManifestPassengerDto>();
                foreach (var bp in confirmedBookings.SelectMany(b => b.BookingPassengers))
                {
                    if (bp.Passenger == null) continue;

                    tickets.TryGetValue(bp.PassengerId, out var ticket);

                    passengerList.Add(new ManifestPassengerDto
                    {
                        PassengerId = bp.PassengerId,
                        FullName = $"{bp.Passenger.FirstName} {bp.Passenger.LastName}",
                        PassportNumber = bp.Passenger.PassportNumber,
                        SeatNumber = bp.SeatAssignment?.SeatNumber ?? "N/A",
                        CabinClass = bp.SeatAssignment?.CabinClass?.Name ?? "N/A",
                        TicketStatus = ticket?.Status.ToString() ?? "Unknown",
                        BookingReference = bp.Booking.BookingRef,
                        FrequentFlyerNumber = bp.Passenger.User?.FrequentFlyer?.CardNumber // Requires User.FF include
                    });
                }

                var manifest = new PassengerManifestDto
                {
                    FlightInstanceId = instance.InstanceId,
                    FlightNumber = instance.Schedule.FlightNo,
                    ScheduledDeparture = instance.ScheduledDeparture,
                    Origin = instance.Schedule.Route.OriginAirportId,
                    Destination = instance.Schedule.Route.DestinationAirportId,
                    AircraftTailNumber = instance.AircraftId,
                    ConfirmedPassengers = passengerList.Count,
                    Passengers = passengerList.OrderBy(p => p.SeatNumber).ToList()
                };

                return ServiceResult<PassengerManifestDto>.Success(manifest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Passenger Manifest for FlightInstanceId {FlightId}.", flightInstanceId);
                return ServiceResult<PassengerManifestDto>.Failure("An internal error occurred while generating the manifest.");
            }
        }

        // Retrieves a list of manifests for all flights departing from an airport on a specific day.
        public async Task<ServiceResult<IEnumerable<PassengerManifestDto>>> GetDailyDepartureManifestsAsync(string airportIataCode, DateTime forDate)
        {
            _logger.LogInformation("Getting daily departure manifests for {Airport} on {Date}.", airportIataCode, forDate.Date);
            try
            {
                var startDate = forDate.Date;
                var endDate = forDate.Date.AddDays(1);
                airportIataCode = airportIataCode.ToUpper();
                // Find all flight instances departing from this airport on this day
                var departingFlights = (await _unitOfWork.FlightInstances.GetByScheduledDateRangeAsync(startDate, endDate))
                    .Where(i => i.Schedule.Route.OriginAirportId == airportIataCode && i.Status != "Cancelled")
                    .OrderBy(i => i.ScheduledDeparture)
                    .ToList();

                if (!departingFlights.Any())
                {
                    _logger.LogInformation("No departing flights found for {Airport} on {Date}.", airportIataCode, forDate.Date);
                    return ServiceResult<IEnumerable<PassengerManifestDto>>.Success(new List<PassengerManifestDto>());
                }

                // Generate a manifest for each flight
                var manifests = new List<PassengerManifestDto>();
                foreach (var flight in departingFlights)
                {
                    var manifestResult = await GetPassengerManifestAsync(flight.InstanceId);
                    if (manifestResult.IsSuccess)
                    {
                        manifests.Add(manifestResult.Data);
                    }
                    else
                    {
                        // Log the error but continue processing other flights
                        _logger.LogWarning("Failed to generate manifest for FlightInstanceId {FlightId}: {Errors}", flight.InstanceId, string.Join("; ", manifestResult.Errors));
                    }
                }

                return ServiceResult<IEnumerable<PassengerManifestDto>>.Success(manifests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily departure manifests for {Airport} on {Date}.", airportIataCode, forDate.Date);
                return ServiceResult<IEnumerable<PassengerManifestDto>>.Failure("An internal error occurred.");
            }
        }
    }
}