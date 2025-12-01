using Application.DTOs.Airport;
using Application.Models;  
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions; // Required for Expression<Func<>>
using System;
using Microsoft.EntityFrameworkCore; // Required for StringComparison

namespace Application.Services
{ 
    public class AirportService : IAirportService
    {
        private readonly IUnitOfWork _unitOfWork;
         
        public AirportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // --- Helper Method for Mapping ---
        private AirportDto MapToDto(Airport airport)
        {
            return new AirportDto
            {
                IataCode = airport.IataCode,
                IcaoCode = airport.IcaoCode,
                Name = airport.Name,
                City = airport.City,
                CountryIsoCode = airport.CountryId,
                CountryName = airport.Country?.Name ?? "N/A", // Safely access included country name
                Latitude = airport.Latitude,
                Longitude = airport.Longitude,
                Altitude = airport.Altitude
            };
        }

        /// <summary>
        /// Retrieves all active airports, ordered by name.
        /// </summary>
        public async Task<ServiceResult<IEnumerable<AirportDto>>> GetAllActiveAirportsAsync()
        {
            // Use specific repo method that includes Country
            var airports = await _unitOfWork.Airports.GetAllActiveAsync(); // Assuming GetAllActiveAsync includes Country
            var airportDtos = airports.OrderBy(a => a.Name).Select(MapToDto);
            return ServiceResult<IEnumerable<AirportDto>>.Success(airportDtos);
        }

        /// <summary>
        /// Retrieves a specific active airport by its IATA code.
        /// </summary>
        public async Task<ServiceResult<AirportDto>> GetAirportByIataCodeAsync(string iataCode)
        {
            if (string.IsNullOrWhiteSpace(iataCode) || iataCode.Length != 3)
            {
                return ServiceResult<AirportDto>.Failure("Invalid IATA code provided.");
            }
            // Use specific repo method that includes Country
            var airport = await _unitOfWork.Airports.GetWithCountryAsync(iataCode);
            if (airport == null) // Repository method already checks IsDeleted
            {
                return ServiceResult<AirportDto>.Failure($"Airport with IATA code '{iataCode}' not found or is inactive.");
            }
            return ServiceResult<AirportDto>.Success(MapToDto(airport));
        }

        /// <summary>
        /// Retrieves a specific active airport by its ICAO code.
        /// </summary>
        public async Task<ServiceResult<AirportDto>> GetAirportByIcaoCodeAsync(string icaoCode)
        {
            if (string.IsNullOrWhiteSpace(icaoCode) || icaoCode.Length != 4)
            {
                return ServiceResult<AirportDto>.Failure("Invalid ICAO code provided.");
            }
            // Need to fetch and include Country separately if GetByIcaoCodeAsync doesn't include it
            var airport = await _unitOfWork.Airports.GetByIcaoCodeAsync(icaoCode);
            if (airport == null)
            {
                return ServiceResult<AirportDto>.Failure($"Airport with ICAO code '{icaoCode}' not found or is inactive.");
            }
            // Eager load country if not already loaded by the repository method
            if (airport.Country == null)
            {
                airport.Country = await _unitOfWork.Countries.GetByIsoCodeAsync(airport.CountryId);
            }
            return ServiceResult<AirportDto>.Success(MapToDto(airport));
        }

        /// <summary>
        /// Finds active airports where the name contains the specified text.
        /// </summary>
        public async Task<ServiceResult<IEnumerable<AirportDto>>> FindAirportsByNameAsync(string nameSubstring)
        {
            if (string.IsNullOrWhiteSpace(nameSubstring))
            {
                return ServiceResult<IEnumerable<AirportDto>>.Failure("Search term cannot be empty.");
            }
            var airports = await _unitOfWork.Airports.FindByNameAsync(nameSubstring);
            // Need to ensure Country is loaded for mapping
            var airportsWithCountry = new List<Airport>();
            foreach (var airport in airports)
            {
                if (airport.Country == null) airport.Country = await _unitOfWork.Countries.GetByIsoCodeAsync(airport.CountryId);
                airportsWithCountry.Add(airport);
            }
            return ServiceResult<IEnumerable<AirportDto>>.Success(airportsWithCountry.Select(MapToDto));
        }

        /// <summary>
        /// Retrieves all active airports located within a specific city.
        /// </summary>
        public async Task<ServiceResult<IEnumerable<AirportDto>>> GetAirportsByCityAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return ServiceResult<IEnumerable<AirportDto>>.Failure("City name cannot be empty.");
            }
            var airports = await _unitOfWork.Airports.GetByCityAsync(city);
            // Ensure Country is loaded
            var airportsWithCountry = new List<Airport>();
            foreach (var airport in airports)
            {
                if (airport.Country == null) airport.Country = await _unitOfWork.Countries.GetByIsoCodeAsync(airport.CountryId);
                airportsWithCountry.Add(airport);
            }
            return ServiceResult<IEnumerable<AirportDto>>.Success(airportsWithCountry.Select(MapToDto));
        }

        /// <summary>
        /// Retrieves all active airports located within a specific country.
        /// </summary>
        public async Task<ServiceResult<IEnumerable<AirportDto>>> GetAirportsByCountryAsync(string countryIsoCode)
        {
            if (string.IsNullOrWhiteSpace(countryIsoCode) || countryIsoCode.Length != 3)
            {
                return ServiceResult<IEnumerable<AirportDto>>.Failure("Invalid Country ISO code provided.");
            }
            var airports = await _unitOfWork.Airports.GetByCountryAsync(countryIsoCode); // Repo method includes Country
            return ServiceResult<IEnumerable<AirportDto>>.Success(airports.Select(MapToDto));
        }

        /// <summary>
        /// Performs an advanced search for airports based on multiple optional filter criteria.
        /// Supports pagination.
        /// </summary>
        public async Task<ServiceResult<PaginatedResult<AirportDto>>> SearchAirportsAsync(AirportFilterDto filter, int pageNumber, int pageSize)
        {
            // Build the filter expression dynamically
            Expression<Func<Airport, bool>> filterExpression = a => (filter.IncludeDeleted || !a.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.NameContains))
            {
                filterExpression = filterExpression.And(a => EF.Functions.Like(a.Name, $"%{filter.NameContains}%"));
            }
            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                //filterExpression = filterExpression.And(a => a.City.Equals(filter.City, StringComparison.OrdinalIgnoreCase));
                var citySearchTerm = filter.City.ToLowerInvariant();
                filterExpression = filterExpression.And(a => a.City.ToLower().Contains(citySearchTerm));
            }
            if (!string.IsNullOrWhiteSpace(filter.CountryIsoCode))
            {
                filterExpression = filterExpression.And(a => a.CountryId == filter.CountryIsoCode.ToUpperInvariant());
            }
            if (!string.IsNullOrWhiteSpace(filter.Continent)) // Filter by continent via country
            {
                // This requires a join or subquery logic, potentially better handled in repo if frequent
                // For service layer: Get country codes for continent first
                var countryCodesInContinent = (await _unitOfWork.Countries.GetByContinentAsync(filter.Continent)).Select(c => c.IsoCode);
                filterExpression = filterExpression.And(a => countryCodesInContinent.Contains(a.CountryId));
            }
            // Add coordinate/altitude filters
            if (filter.MinLatitude.HasValue) filterExpression = filterExpression.And(a => a.Latitude >= filter.MinLatitude.Value);
            if (filter.MaxLatitude.HasValue) filterExpression = filterExpression.And(a => a.Latitude <= filter.MaxLatitude.Value);
            if (filter.MinLongitude.HasValue) filterExpression = filterExpression.And(a => a.Longitude >= filter.MinLongitude.Value);
            if (filter.MaxLongitude.HasValue) filterExpression = filterExpression.And(a => a.Longitude <= filter.MaxLongitude.Value);
            if (filter.MinAltitude.HasValue) filterExpression = filterExpression.And(a => a.Altitude.HasValue && a.Altitude >= filter.MinAltitude.Value);
            if (filter.MaxAltitude.HasValue) filterExpression = filterExpression.And(a => a.Altitude.HasValue && a.Altitude <= filter.MaxAltitude.Value);


            // Fetch paged results using the generic repository method
            // Note: Include Country manually after fetching if GetPagedAsync doesn't support includes easily
            var (airports, totalCount) = await _unitOfWork.Airports.GetPagedAsync(
                pageNumber,
                pageSize,
                filterExpression,
                orderBy: q => q.OrderBy(a => a.Name) // Default ordering
            );

            // Ensure Country is loaded for mapping DTOs
            var airportsWithCountry = new List<Airport>();
            foreach (var airport in airports)
            {
                if (airport.Country == null) airport.Country = await _unitOfWork.Countries.GetByIsoCodeAsync(airport.CountryId); // Fetch if missing
                airportsWithCountry.Add(airport);
            }

            var airportDtos = airportsWithCountry.Select(MapToDto).ToList();

            var paginatedResult = new PaginatedResult<AirportDto>(airportDtos, totalCount, pageNumber, pageSize);
            return ServiceResult<PaginatedResult<AirportDto>>.Success(paginatedResult);
        }


        /// <summary>
        /// Creates a new airport after validation.
        /// </summary>
        public async Task<ServiceResult<AirportDto>> CreateAirportAsync(CreateAirportDto createDto)
        {
            // Normalize codes
            var iataUpper = createDto.IataCode.ToUpperInvariant();
            var icaoUpper = createDto.IcaoCode.ToUpperInvariant();
            var countryUpper = createDto.CountryIsoCode.ToUpperInvariant();

            // Validate Country exists
            if (!await _unitOfWork.Countries.ExistsByIsoCodeAsync(countryUpper))
            {
                return ServiceResult<AirportDto>.Failure($"Country with ISO code '{countryUpper}' does not exist.");
            }

            // Check uniqueness of codes (including deleted records to prevent reuse issues)
            if (await _unitOfWork.Airports.ExistsByIataCodeAsync(iataUpper))
            {
                return ServiceResult<AirportDto>.Failure($"Airport with IATA code '{iataUpper}' already exists.");
            }
            if (await _unitOfWork.Airports.ExistsByIcaoCodeAsync(icaoUpper))
            {
                return ServiceResult<AirportDto>.Failure($"Airport with ICAO code '{icaoUpper}' already exists.");
            }

            var newAirport = new Airport
            {
                IataCode = iataUpper,
                IcaoCode = icaoUpper,
                Name = createDto.Name,
                City = createDto.City,
                CountryId = countryUpper,
                Latitude = createDto.Latitude,
                Longitude = createDto.Longitude,
                Altitude = createDto.Altitude,
                IsDeleted = false
            };

            await _unitOfWork.Airports.AddAsync(newAirport);
            await _unitOfWork.SaveChangesAsync();

            // Load country for the DTO response
            newAirport.Country = await _unitOfWork.Countries.GetByIsoCodeAsync(newAirport.CountryId);

            return ServiceResult<AirportDto>.Success(MapToDto(newAirport));
        }

        /// <summary>
        /// Updates an existing airport's details.
        /// </summary>
        public async Task<ServiceResult<AirportDto>> UpdateAirportAsync(string iataCode, UpdateAirportDto updateDto)
        {
            var iataUpper = iataCode.ToUpperInvariant(); 

            // A better approach is to load it with the Country for a complete DTO mapping.
            // If you don't have GetByIataCodeWithCountryAsync, you'd need to fetch the country separately:
            var airport = await _unitOfWork.Airports.GetByIataCodeAsync(iataUpper); // Get active by PK

            if (airport == null)
            {
                return ServiceResult<AirportDto>.Failure($"Active airport with IATA code '{iataUpper}' not found.");
            }

            // 2. Validate new Country exists if changed
            var newCountryUpper = updateDto.CountryIsoCode.ToUpperInvariant();
            if (airport.CountryId != newCountryUpper)
            {
                if (!await _unitOfWork.Countries.ExistsByIsoCodeAsync(newCountryUpper))
                {
                    return ServiceResult<AirportDto>.Failure($"Country with ISO code '{newCountryUpper}' does not exist.");
                }
            }

            // 3. Apply updates
            bool changed = false;
            if (airport.Name != updateDto.Name) { airport.Name = updateDto.Name; changed = true; }
            if (airport.City != updateDto.City) { airport.City = updateDto.City; changed = true; }
            if (airport.CountryId != newCountryUpper) { airport.CountryId = newCountryUpper; changed = true; }
            if (airport.Latitude != updateDto.Latitude) { airport.Latitude = updateDto.Latitude; changed = true; }
            if (airport.Longitude != updateDto.Longitude) { airport.Longitude = updateDto.Longitude; changed = true; }
            if (airport.Altitude != updateDto.Altitude) { airport.Altitude = updateDto.Altitude; changed = true; }
            // NOTE: If ICAO is part of the DTO, it should be updated here as well.

            if (!changed)
            {
                // 4. If no changes, still return the DTO of the current state
                // Ensure the Country navigation property is loaded to use MapToDto
                if (airport.Country == null)
                {
                    airport.Country = await _unitOfWork.Countries.GetByIsoCodeAsync(airport.CountryId);
                }
                var currentDto = MapToDto(airport);
                return ServiceResult<AirportDto>.Success(currentDto);
            }

            _unitOfWork.Airports.Update(airport);
            await _unitOfWork.SaveChangesAsync();

            // 5. After successful update, fetch the Country name if not already loaded,
            // and map the updated entity back to a DTO for the response.
            if (airport.Country == null)
            {
                airport.Country = await _unitOfWork.Countries.GetByIsoCodeAsync(airport.CountryId);
            }

            var updatedDto = MapToDto(airport);

            // 6. Return success with the updated DTO
            return ServiceResult<AirportDto>.Success(updatedDto);
        }

        /// <summary>
        /// Soft deletes an airport after checking dependencies.
        /// </summary>
        public async Task<ServiceResult> DeleteAirportAsync(string iataCode)
        {
            var iataUpper = iataCode.ToUpperInvariant();
            var airport = await _unitOfWork.Airports.GetByIataCodeAsync(iataUpper);

            if (airport == null)
            {
                return ServiceResult.Failure($"Active airport with IATA code '{iataUpper}' not found.");
            }

            // Check for dependencies: Active routes, schedules, base for airlines, crew base etc.
            bool hasActiveRoutes = await _unitOfWork.Routes.AnyAsync(r => !r.IsDeleted && (r.OriginAirportId == iataUpper || r.DestinationAirportId == iataUpper));
            bool hasActiveSchedules = await _unitOfWork.FlightSchedules.AnyAsync(fs => !fs.IsDeleted && (fs.Route.OriginAirportId == iataUpper || fs.Route.DestinationAirportId == iataUpper)); // More complex query needed
            bool isBaseForAirline = await _unitOfWork.Airlines.AnyAsync(al => !al.IsDeleted && al.BaseAirportId == iataUpper);
            bool isCrewBase = await _unitOfWork.CrewMembers.AnyAsync(cm => !cm.IsDeleted && cm.CrewBaseAirportId == iataUpper);

            if (hasActiveRoutes || hasActiveSchedules || isBaseForAirline || isCrewBase)
            {
                List<string> dependencies = new();
                if (hasActiveRoutes) dependencies.Add("active routes");
                if (hasActiveSchedules) dependencies.Add("active flight schedules");
                if (isBaseForAirline) dependencies.Add("base for airlines");
                if (isCrewBase) dependencies.Add("crew base");
                return ServiceResult.Failure($"Cannot delete airport '{iataUpper}' as it is currently used by: {string.Join(", ", dependencies)}. Please resolve dependencies first.");
            }

            _unitOfWork.Airports.SoftDelete(airport);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success();
        }

        /// <summary>
        /// Reactivates a soft-deleted airport.
        /// </summary>
        public async Task<ServiceResult> ReactivateAirportAsync(string iataCode)
        {
            var iataUpper = iataCode.ToUpperInvariant();
            // Fetch including deleted using GetByIdAsync from Generic Repo, assuming it finds by PK
            var airport = await _unitOfWork.Airports.GetByIdAsync(iataUpper);

            if (airport == null)
            {
                return ServiceResult.Failure($"Airport with IATA code '{iataUpper}' not found.");
            }
            if (!airport.IsDeleted)
            {
                return ServiceResult.Failure($"Airport '{iataUpper}' is already active.");
            }

            airport.IsDeleted = false; // Reactivate
            _unitOfWork.Airports.Update(airport);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success();
        }

        /// <summary>
        /// Retrieves all airports, including soft-deleted ones. Includes country name.
        /// </summary>
        public async Task<ServiceResult<IEnumerable<AirportDto>>> GetAllAirportsIncludingDeletedAsync()
        {
            var airports = await _unitOfWork.Airports.GetAllIncludingDeletedAsync();
            // Ensure Country is loaded
            var airportsWithCountry = new List<Airport>();
            foreach (var airport in airports)
            {
                if (airport.Country == null) airport.Country = await _unitOfWork.Countries.GetByIsoCodeAsync(airport.CountryId); // Fetch if missing
                airportsWithCountry.Add(airport);
            }
            var airportDtos = airportsWithCountry.OrderBy(a => a.Name).Select(MapToDto);
            return ServiceResult<IEnumerable<AirportDto>>.Success(airportDtos);
        }

        
    }

    
}