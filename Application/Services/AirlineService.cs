using Application.DTOs.Airline;
using Application.DTOs.Aircraft;
using Application.Models;
using Application.Maps;  
using AutoMapper;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using System; 
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;  
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;   
using AutoMapper;
using static Application.Maps.AirlineMappingProfile;

namespace Application.Services
{
    // Service implementation for managing Airline data.
    public class AirlineService : IAirlineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper; 

        // Constructor injection of IUnitOfWork
        public AirlineService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;  
        }

        // Retrieves all active airlines, ordered by name.
        public async Task<ServiceResult<IEnumerable<AirlineDto>>> GetAllActiveAirlinesAsync()
        {
            // Repository method now includes BaseAirport
            var airlines = await _unitOfWork.Airlines.GetAllActiveAsync(); 
            var airlineDtos = airlines.OrderBy(a => a.Name).Select(AirlineMapper.MapToDto);
            return ServiceResult<IEnumerable<AirlineDto>>.Success(airlineDtos);
        }

        // Retrieves a specific active airline by its IATA code.
        public async Task<ServiceResult<AirlineDto>> GetAirlineByIataCodeAsync(string iataCode)
        {
            if (string.IsNullOrWhiteSpace(iataCode) || iataCode.Length != 2)
                return ServiceResult<AirlineDto>.Failure("Invalid IATA code provided.");

            var airline = await _unitOfWork.Airlines.GetWithBaseAirportAsync(iataCode); // Repo method includes BaseAirport
            if (airline == null) 
                return ServiceResult<AirlineDto>.Failure($"Airline with IATA code '{iataCode}' not found or is inactive.");

            return ServiceResult<AirlineDto>.Success(AirlineMapper.MapToDto(airline));
        }

        // Finds active airlines where the name contains the specified text.
        public async Task<ServiceResult<IEnumerable<AirlineDto>>> FindAirlinesByNameAsync(string nameSubstring)
        {
            if (string.IsNullOrWhiteSpace(nameSubstring))
                return ServiceResult<IEnumerable<AirlineDto>>.Failure("Search term cannot be empty.");

            var airlines = await _unitOfWork.Airlines.FindByNameAsync(nameSubstring);
            // We assume FindByNameAsync does NOT include BaseAirport, so we load it
            var airlinesWithDetails = new List<Airline>();
            foreach (var airline in airlines)
            {
                if (airline.BaseAirport == null)
                    airline.BaseAirport = await _unitOfWork.Airports.GetByIataCodeAsync(airline.BaseAirportId);
                airlinesWithDetails.Add(airline);
            }
            return ServiceResult<IEnumerable<AirlineDto>>.Success(airlinesWithDetails.Select(AirlineMapper.MapToDto));
        }

        // Retrieves all active airlines based at a specific airport.
        public async Task<ServiceResult<IEnumerable<AirlineDto>>> GetAirlinesByBaseAirportAsync(string airportIataCode)
        {
            if (string.IsNullOrWhiteSpace(airportIataCode) || airportIataCode.Length != 3)
                return ServiceResult<IEnumerable<AirlineDto>>.Failure("Invalid Airport IATA code provided.");

            var airlines = await _unitOfWork.Airlines.GetByBaseAirportAsync(airportIataCode); // Repo method includes BaseAirport
            return ServiceResult<IEnumerable<AirlineDto>>.Success(airlines.Select(AirlineMapper.MapToDto));
        }

        // Retrieves all active airlines operating within a specific region.
        public async Task<ServiceResult<IEnumerable<AirlineDto>>> GetAirlinesByOperatingRegionAsync(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
                return ServiceResult<IEnumerable<AirlineDto>>.Failure("Operating region cannot be empty.");

            var airlines = await _unitOfWork.Airlines.GetByOperatingRegionAsync(region);
            // Ensure BaseAirport is loaded
            var airlinesWithDetails = new List<Airline>();
            foreach (var airline in airlines)
            {
                if (airline.BaseAirport == null)
                    airline.BaseAirport = await _unitOfWork.Airports.GetByIataCodeAsync(airline.BaseAirportId);
                airlinesWithDetails.Add(airline);
            }
            return ServiceResult<IEnumerable<AirlineDto>>.Success(airlinesWithDetails.Select(AirlineMapper.MapToDto));
        }

        // Retrieves detailed information for an active airline, including its fleet.
        public async Task<ServiceResult<AirlineDetailDto>> GetAirlineWithFleetAsync(string iataCode)
        {
            if (string.IsNullOrWhiteSpace(iataCode) || iataCode.Length != 2)
                return ServiceResult<AirlineDetailDto>.Failure("Invalid IATA code provided.");

            // Repository method now includes Aircrafts.AircraftType and BaseAirport
            var airline = await _unitOfWork.Airlines.GetWithAircraftAsync(iataCode); 
            if (airline == null)
                return ServiceResult<AirlineDetailDto>.Failure($"Airline with IATA code '{iataCode}' not found or is inactive.");

            // Map base details
            var detailDto = new AirlineDetailDto
            {
                IataCode = airline.IataCode,
                Name = airline.Name,
                Callsign = airline.Callsign,
                OperatingRegion = airline.OperatingRegion,
                BaseAirportIataCode = airline.BaseAirportId,
                BaseAirportName = airline.BaseAirport?.Name ?? "N/A",
                // Map the fleet using AutoMapper (assuming Aircraft -> AircraftDto map exists)
                Fleet = _mapper.Map<List<AircraftDto>>(airline.Aircrafts?.Where(ac => !ac.IsDeleted).ToList())
            };

            return ServiceResult<AirlineDetailDto>.Success(detailDto);
        }

        // Creates a new airline after validation.
        public async Task<ServiceResult<AirlineDto>> CreateAirlineAsync(CreateAirlineDto createDto)
        {
            var iataUpper = createDto.IataCode.ToUpperInvariant();

            var baseAirport = await _unitOfWork.Airports.GetByIataCodeAsync(createDto.BaseAirportIataCode.ToUpperInvariant());
            if (baseAirport == null) 
                return ServiceResult<AirlineDto>.Failure($"Base airport '{createDto.BaseAirportIataCode}' not found or is inactive.");

            if (await _unitOfWork.Airlines.ExistsByIataCodeAsync(iataUpper))
                return ServiceResult<AirlineDto>.Failure($"Airline with IATA code '{iataUpper}' already exists.");
            if (await _unitOfWork.Airlines.ExistsByNameAsync(createDto.Name))
                return ServiceResult<AirlineDto>.Failure($"Airline with name '{createDto.Name}' already exists.");

            var newAirline = new Airline
            {
                IataCode = iataUpper,
                Name = createDto.Name,
                Callsign = createDto.Callsign,
                OperatingRegion = createDto.OperatingRegion,
                BaseAirportId = baseAirport.IataCode, 
                IsDeleted = false
            };

            await _unitOfWork.Airlines.AddAsync(newAirline);
            await _unitOfWork.SaveChangesAsync();

            newAirline.BaseAirport = baseAirport; 
            return ServiceResult<AirlineDto>.Success(AirlineMapper.MapToDto(newAirline));
        }

        // Updates an existing airline's details.
        public async Task<ServiceResult<AirlineDto>> UpdateAirlineAsync(string iataCode, UpdateAirlineDto updateDto)
        {
            var iataUpper = iataCode.ToUpperInvariant();
            // 1. Get the airline entity. We assume GetByIataCodeAsync does NOT eager-load BaseAirport,
            // so we might need to load it later for a complete DTO.
            var airline = await _unitOfWork.Airlines.GetByIataCodeAsync(iataUpper);

            if (airline == null)
                return ServiceResult<AirlineDto>.Failure($"Active airline with IATA code '{iataUpper}' not found.");

            // 2. Validate new Base Airport
            var newBaseAirportUpper = updateDto.BaseAirportIataCode.ToUpperInvariant();
            if (airline.BaseAirportId != newBaseAirportUpper)
            {
                // Use GetByIataCodeAsync to check existence (and activity)
                var newBaseAirport = await _unitOfWork.Airports.GetByIataCodeAsync(newBaseAirportUpper);
                if (newBaseAirport == null)
                    return ServiceResult<AirlineDto>.Failure($"New base airport '{newBaseAirportUpper}' not found or is inactive.");

                // Load the new base airport entity into the navigation property if we proceed with the update
                // This is important for the mapping step later if the BaseAirportId changes.
                airline.BaseAirport = newBaseAirport;
            }
            // If BaseAirportId is not changing, and BaseAirport is not loaded (default for GetByIataCodeAsync), 
            // we need to load it now to ensure the DTO mapping succeeds later.
            else if (airline.BaseAirport == null)
            {
                airline.BaseAirport = await _unitOfWork.Airports.GetByIataCodeAsync(airline.BaseAirportId);
            }


            // 3. Check name conflict
            var existingByName = await _unitOfWork.Airlines.FindByNameAsync(updateDto.Name);
            if (existingByName.Any(a => a.IataCode != iataUpper && a.Name.Equals(updateDto.Name, StringComparison.OrdinalIgnoreCase)))
                return ServiceResult<AirlineDto>.Failure($"Another airline with the name '{updateDto.Name}' already exists.");


            // 4. Apply updates
            bool changed = false;
            if (airline.Name != updateDto.Name) { airline.Name = updateDto.Name; changed = true; }
            if (airline.Callsign != updateDto.Callsign) { airline.Callsign = updateDto.Callsign; changed = true; }
            if (airline.OperatingRegion != updateDto.OperatingRegion) { airline.OperatingRegion = updateDto.OperatingRegion; changed = true; }
            if (airline.BaseAirportId != newBaseAirportUpper) { airline.BaseAirportId = newBaseAirportUpper; changed = true; }

            if (!changed)
            {
                // Return the current DTO if no changes were made
                var currentDto = _mapper.Map<AirlineDto>(airline);
                return ServiceResult<AirlineDto>.Success(currentDto);
            }

            _unitOfWork.Airlines.Update(airline);
            await _unitOfWork.SaveChangesAsync();

            // 5. Map the updated entity back to a DTO for the response.
            // Ensure BaseAirport is correctly set (done in step 2) for the DTO mapping (using AutoMapper).
            var updatedDto = _mapper.Map<AirlineDto>(airline);

            // 6. Return success with the updated DTO
            return ServiceResult<AirlineDto>.Success(updatedDto);
        }

        // Soft deletes an airline after checking dependencies.
        public async Task<ServiceResult> DeleteAirlineAsync(string iataCode)
        {
            var iataUpper = iataCode.ToUpperInvariant();
            var airline = await _unitOfWork.Airlines.GetByIataCodeAsync(iataUpper);

            if (airline == null)
                return ServiceResult.Failure($"Active airline with IATA code '{iataUpper}' not found.");

            bool hasActiveAircraft = await _unitOfWork.Aircrafts.AnyAsync(ac => ac.AirlineId == iataUpper && !ac.IsDeleted);
            bool hasActiveSchedules = await _unitOfWork.FlightSchedules.AnyAsync(fs => fs.AirlineId == iataUpper && !fs.IsDeleted);
            bool hasActiveRouteOps = await _unitOfWork.RouteOperators.AnyAsync(ro => ro.AirlineId == iataUpper && !ro.IsDeleted);

            if (hasActiveAircraft || hasActiveSchedules || hasActiveRouteOps)
            {
                List<string> dependencies = new();
                if (hasActiveAircraft) dependencies.Add("active aircraft");
                if (hasActiveSchedules) dependencies.Add("active flight schedules");
                if (hasActiveRouteOps) dependencies.Add("active route operations");
                return ServiceResult.Failure($"Cannot delete airline '{iataUpper}' as it has associated: {string.Join(", ", dependencies)}. Please resolve dependencies first.");
            }

            _unitOfWork.Airlines.SoftDelete(airline);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success();
        }

        // Reactivates a soft-deleted airline.
        public async Task<ServiceResult> ReactivateAirlineAsync(string iataCode)
        {
            var iataUpper = iataCode.ToUpperInvariant();
            var airline = await _unitOfWork.Airlines.GetByIdAsync(iataUpper); 

            if (airline == null)
                return ServiceResult.Failure($"Airline with IATA code '{iataUpper}' not found.");

            if (!airline.IsDeleted)
                return ServiceResult.Failure($"Airline '{iataUpper}' is already active.");

            airline.IsDeleted = false; 
            _unitOfWork.Airlines.Update(airline);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success();
        }

        // Retrieves all airlines, including soft-deleted ones.
        public async Task<ServiceResult<IEnumerable<AirlineDto>>> GetAllAirlinesIncludingDeletedAsync()
        {
            var airlines = await _unitOfWork.Airlines.GetAllIncludingDeletedAsync();
            var airlinesWithDetails = new List<Airline>();
            foreach (var airline in airlines)
            {
                if (airline.BaseAirport == null)
                    airline.BaseAirport = await _unitOfWork.Airports.GetByIataCodeAsync(airline.BaseAirportId);
                airlinesWithDetails.Add(airline);
            }
            var airlineDtos = airlinesWithDetails.OrderBy(a => a.Name).Select(AirlineMapper.MapToDto);
            return ServiceResult<IEnumerable<AirlineDto>>.Success(airlineDtos);
        }

        // Retrieves a paginated list of active airlines, optionally filtered by region.
        public async Task<ServiceResult<PaginatedResult<AirlineDto>>> GetPaginatedAirlinesAsync(int pageNumber, int pageSize, string? regionFilter = null)
        {
            Expression<Func<Airline, bool>> filter = a => !a.IsDeleted;
            if (!string.IsNullOrWhiteSpace(regionFilter))
            {
                 
                var upperRegion = regionFilter.ToUpper();
                filter = filter.And(a => a.OperatingRegion.ToUpper() == upperRegion);
            }

            var (airlines, totalCount) = await _unitOfWork.Airlines.GetPagedAsync(
                pageNumber,
                pageSize,
                filter,
                orderBy: q => q.OrderBy(a => a.Name),
                includeProperties: "BaseAirport" // Ensure BaseAirport is included in GetPagedAsync
            );

            // Mapping (BaseAirport should be included now)
            var airlineDtos = airlines.Select(AirlineMapper.MapToDto).ToList();
            var paginatedResult = new PaginatedResult<AirlineDto>(airlineDtos, totalCount, pageNumber, pageSize);
            return ServiceResult<PaginatedResult<AirlineDto>>.Success(paginatedResult);
        }
    }
}