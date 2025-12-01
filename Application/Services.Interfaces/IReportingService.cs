using Application.DTOs.Reporting;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Interface for generating high-level business and operational reports.
    public interface IReportingService
    {
        // Generates a comprehensive sales and revenue summary.
        Task<ServiceResult<SalesSummaryReportDto>> GetSalesSummaryReportAsync(ReportRequestDto request);

        // Generates an operational report on flight performance (delays, cancellations).
        Task<ServiceResult<FlightPerformanceReportDto>> GetFlightPerformanceReportAsync(ReportRequestDto request);

        // Generates a report on flight occupancy and load factors.
        Task<ServiceResult<LoadFactorReportDto>> GetLoadFactorReportAsync(ReportRequestDto request);

        // Retrieves the full passenger manifest for a single flight instance.
        Task<ServiceResult<PassengerManifestDto>> GetPassengerManifestAsync(int flightInstanceId);

        // Retrieves a list of manifests for all flights departing from an airport on a specific day.
        Task<ServiceResult<IEnumerable<PassengerManifestDto>>> GetDailyDepartureManifestsAsync(string airportIataCode, DateTime forDate); 

    }
}