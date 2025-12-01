using System.Collections.Generic;
using System.Threading.Tasks; 
using Application.DTOs.Flight;
using Application.Models;

namespace Application.Services.Interfaces
{
    public interface IFlightService
    { 

        // Performs a comprehensive search for flights (One-Way, Round-Trip, Multi-City).
        Task<ServiceResult<IEnumerable<FlightItineraryDto>>> SearchFlightsAsync(FlightSearchRequestDto searchRequest);

        // Retrieves detailed information for a specific flight instance (for details modal).
        Task<ServiceResult<FlightDetailsDto>> GetFlightDetailsAsync(int flightInstanceId);

        // Retrieves the available fare options (e.g., Lite, Standard, Flex) for a specific flight.
        Task<ServiceResult<IEnumerable<FlightFareOptionDto>>> GetFareOptionsForFlightAsync(int flightInstanceId, int totalPassengers);

        // Public-facing method to check the real-time status of a flight by its number and date.
        Task<ServiceResult<FlightSegmentDto>> GetFlightStatusByNumberAsync(FlightStatusRequestDto statusRequest);

        // Quick check for seat availability for a specific flight.
        Task<ServiceResult<bool>> CheckFlightAvailabilityAsync(int flightInstanceId, int passengerCount);
    }
}