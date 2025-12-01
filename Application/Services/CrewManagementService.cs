using Application.DTOs.Crew;
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;  

namespace Application.Services
{
    // Service implementation for managing flight crew and certifications.
    public class CrewManagementService : ICrewManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CrewManagementService> _logger;
        private readonly IUserRepository _userRepository;  
         
        public CrewManagementService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CrewManagementService> logger, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
        }

        // Retrieves a paginated list of crew members based on filters.
        public async Task<ServiceResult<PaginatedResult<CrewMemberSummaryDto>>> GetCrewMembersPaginatedAsync(CrewFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Retrieving paginated crew members for page {PageNumber}.", pageNumber);
            try
            {
                // This requires a more specific repository method ideally.
                // Workaround: Get all active crew, then filter and paginate in memory (inefficient for large datasets).
                var allCrew = await _unitOfWork.CrewMembers.GetAllActiveWithEmployeeAsync();

                // Apply filters
                var filteredCrew = allCrew;
                if (!string.IsNullOrWhiteSpace(filter.NameContains))
                {
                    var name = filter.NameContains.ToLower();
                    filteredCrew = filteredCrew.Where(c =>
                        (c.Employee?.AppUser?.FirstName?.ToLower().Contains(name) ?? false) ||
                        (c.Employee?.AppUser?.LastName?.ToLower().Contains(name) ?? false));
                }
                if (!string.IsNullOrWhiteSpace(filter.Position))
                {
                    var upperPosition = filter.Position.ToUpper();
                    filteredCrew = filteredCrew.Where(c => c.Position.ToUpper() == upperPosition);
                }
                if (!string.IsNullOrWhiteSpace(filter.CrewBaseAirportIata))
                {
                    var upperCrewBaseAirportIata = filter.CrewBaseAirportIata.ToUpper();
                    filteredCrew = filteredCrew.Where(c => c.CrewBaseAirportId == upperCrewBaseAirportIata);
                }
                if (filter.IncludeDeleted == false) // Already filtered by GetAllActive, but double-check AppUser
                {
                    filteredCrew = filteredCrew.Where(c => c.Employee?.AppUser != null && !c.Employee.AppUser.IsDeleted);
                }
                // TODO: Add filter for HasExpiredCertification (requires loading certifications)

                // Manual Pagination
                var totalCount = filteredCrew.Count();
                var pagedCrew = filteredCrew
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var dtos = _mapper.Map<List<CrewMemberSummaryDto>>(pagedCrew);
                var paginatedResult = new PaginatedResult<CrewMemberSummaryDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<CrewMemberSummaryDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated crew members.");
                return ServiceResult<PaginatedResult<CrewMemberSummaryDto>>.Failure("An error occurred while retrieving crew members.");
            }
        }

        // Retrieves detailed information for a specific crew member by Employee ID.
        public async Task<ServiceResult<CrewMemberDetailDto>> GetCrewMemberDetailsByIdAsync(int employeeId)
        {
            _logger.LogInformation("Retrieving details for crew member Employee ID {EmployeeId}.", employeeId);
            try
            {
                // Need to fetch AppUser, Employee, CrewMember, Pilot/Attendant, Certifications
                var crewMember = await _unitOfWork.CrewMembers.GetWithEmployeeDetailsAsync(employeeId);
                if (crewMember == null || crewMember.Employee?.AppUser == null)
                    return ServiceResult<CrewMemberDetailDto>.Failure($"Crew member with Employee ID {employeeId} not found.");

                // Map base details
                var dto = _mapper.Map<CrewMemberDetailDto>(crewMember);

                // Fetch and map Pilot/Attendant specific data
                if (crewMember.Employee.AppUser.UserType == UserType.Pilot)
                {
                    var pilotProfile = await _userRepository.GetPilotProfileByUserIdAsync(crewMember.Employee.AppUserId);
                    _mapper.Map(pilotProfile, dto); // Map Pilot specific fields onto the DTO
                }
                // Attendant has no specific fields currently in DTO

                // Fetch and map certifications
                var certifications = await _unitOfWork.Certifications.GetByCrewMemberAsync(employeeId);
                dto.Certifications = _mapper.Map<List<CertificationDto>>(certifications);

                return ServiceResult<CrewMemberDetailDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for crew member Employee ID {EmployeeId}.", employeeId);
                return ServiceResult<CrewMemberDetailDto>.Failure("An error occurred while retrieving crew member details.");
            }
        }

        // Retrieves all active certifications for a specific crew member.
        public async Task<ServiceResult<IEnumerable<CertificationDto>>> GetCertificationsForCrewMemberAsync(int employeeId)
        {
            _logger.LogInformation("Retrieving certifications for Employee ID {EmployeeId}.", employeeId);
            if (!await _unitOfWork.CrewMembers.ExistsByEmployeeIdAsync(employeeId))
                return ServiceResult<IEnumerable<CertificationDto>>.Failure($"Crew member with Employee ID {employeeId} not found.");

            try
            {
                var certifications = await _unitOfWork.Certifications.GetByCrewMemberAsync(employeeId);
                var dtos = _mapper.Map<IEnumerable<CertificationDto>>(certifications);
                return ServiceResult<IEnumerable<CertificationDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving certifications for Employee ID {EmployeeId}.", employeeId);
                return ServiceResult<IEnumerable<CertificationDto>>.Failure("An error occurred while retrieving certifications.");
            }
        }

        // Adds a new certification record for a crew member.
        public async Task<ServiceResult<CertificationDto>> AddCertificationAsync(int employeeId, CreateCertificationDto dto, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} adding certification '{CertType}' for Employee ID {EmployeeId}.", performingUser.Identity?.Name, dto.Type, employeeId);

            // Authorization: Check if user is Admin or SuperAdmin
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot add certifications.", performingUser.Identity?.Name);
                return ServiceResult<CertificationDto>.Failure("Access Denied.");
            }

            if (!await _unitOfWork.CrewMembers.ExistsByEmployeeIdAsync(employeeId))
                return ServiceResult<CertificationDto>.Failure($"Crew member with Employee ID {employeeId} not found.");

            try
            {
                var newCert = _mapper.Map<Certification>(dto);
                newCert.CrewMemberId = employeeId;

                await _unitOfWork.Certifications.AddAsync(newCert);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully added certification ID {CertId} for Employee ID {EmployeeId}.", newCert.CertId, employeeId);
                var resultDto = _mapper.Map<CertificationDto>(newCert);
                return ServiceResult<CertificationDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding certification for Employee ID {EmployeeId}.", employeeId);
                return ServiceResult<CertificationDto>.Failure("An error occurred while adding the certification.");
            }
        }

        // Updates an existing certification record.
        public async Task<ServiceResult<CertificationDto>> UpdateCertificationAsync(int certId, CreateCertificationDto dto, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} updating certification ID {CertId}.", performingUser.Identity?.Name, certId);

            // Authorization
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot update certifications.", performingUser.Identity?.Name);
                return ServiceResult<CertificationDto>.Failure("Access Denied.");
            }

            try
            {
                var cert = await _unitOfWork.Certifications.GetActiveByIdAsync(certId);
                if (cert == null)
                    return ServiceResult<CertificationDto>.Failure($"Certification with ID {certId} not found.");

                // Map updates from DTO to entity
                _mapper.Map(dto, cert);

                _unitOfWork.Certifications.Update(cert);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated certification ID {CertId}.", certId);
                var resultDto = _mapper.Map<CertificationDto>(cert);
                // Returns the updated resource (Best Practice)
                return ServiceResult<CertificationDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating certification ID {CertId}.", certId);
                return ServiceResult<CertificationDto>.Failure("An error occurred while updating the certification.");
            }
        }

        // Deletes a certification record (soft delete).
        public async Task<ServiceResult> DeleteCertificationAsync(int certId, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} deleting certification ID {CertId}.", performingUser.Identity?.Name, certId);

            // Authorization
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot delete certifications.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied.");
            }

            try
            {
                var cert = await _unitOfWork.Certifications.GetActiveByIdAsync(certId);
                if (cert == null)
                    return ServiceResult.Failure($"Certification with ID {certId} not found.");

                _unitOfWork.Certifications.SoftDelete(cert);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully soft-deleted certification ID {CertId}.", certId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting certification ID {CertId}.", certId);
                return ServiceResult.Failure("An error occurred while deleting the certification.");
            }
        }

        // Retrieves crew members whose certifications are expiring soon or have expired.
        public async Task<ServiceResult<IEnumerable<CrewMemberSummaryDto>>> GetCrewWithExpiringCertificationsAsync(int daysUntilExpiry = 30)
        {
            _logger.LogInformation("Finding crew with certifications expiring within {Days} days.", daysUntilExpiry);
            try
            {
                var expiryDateThreshold = DateTime.UtcNow.AddDays(daysUntilExpiry);
                var expiredCerts = await _unitOfWork.Certifications.GetExpiredOrExpiringSoonAsync(expiryDateThreshold);

                // Get distinct crew member IDs from the certifications
                var crewMemberIds = expiredCerts.Select(c => c.CrewMemberId).Distinct();

                // Fetch the crew member details for these IDs
                var crewMembers = new List<CrewMember>();
                foreach (var id in crewMemberIds)
                {
                    var crew = await _unitOfWork.CrewMembers.GetWithEmployeeDetailsAsync(id);
                    if (crew != null) crewMembers.Add(crew);
                }

                var dtos = _mapper.Map<IEnumerable<CrewMemberSummaryDto>>(crewMembers);
                return ServiceResult<IEnumerable<CrewMemberSummaryDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving crew with expiring certifications.");
                return ServiceResult<IEnumerable<CrewMemberSummaryDto>>.Failure("An error occurred while generating the report.");
            }
        }

        // Gets analytics data related to flight crew.
        public async Task<ServiceResult<CrewAnalyticsDto>> GetCrewDashboardAnalyticsAsync()
        {
            _logger.LogInformation("Generating crew dashboard analytics.");
            try
            {
                var allActiveCrew = await _unitOfWork.CrewMembers.GetAllActiveWithEmployeeAsync();
                var allCerts = await _unitOfWork.Certifications.GetAllActiveAsync(); // Get all active certs

                var now = DateTime.UtcNow;
                var thirtyDaysFromNow = now.AddDays(30);

                var dto = new CrewAnalyticsDto
                {
                    TotalActiveCrew = allActiveCrew.Count(),
                    TotalPilots = allActiveCrew.Count(c => c.Position.Equals("Pilot", StringComparison.OrdinalIgnoreCase)),
                    TotalAttendants = allActiveCrew.Count(c => c.Position.Equals("Flight Attendant", StringComparison.OrdinalIgnoreCase))
                                    + allActiveCrew.Count(c => c.Position.Equals("Senior Flight Attendant", StringComparison.OrdinalIgnoreCase)) 
                                    + allActiveCrew.Count(c => c.Position.Equals("Chief Attendant", StringComparison.OrdinalIgnoreCase)),
                    CrewCountByBase = allActiveCrew
                        .Where(c => !string.IsNullOrEmpty(c.CrewBaseAirportId))
                        .GroupBy(c => c.CrewBaseAirportId)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    CertificationsExpired = allCerts.Count(c => c.ExpiryDate < now),
                    CertificationsExpiringSoon = allCerts.Count(c => c.ExpiryDate >= now && c.ExpiryDate < thirtyDaysFromNow)
                };

                return ServiceResult<CrewAnalyticsDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating crew dashboard analytics.");
                return ServiceResult<CrewAnalyticsDto>.Failure("An error occurred while generating analytics.");
            }
        }

        // Updates the base airport for a crew member.
        public async Task<ServiceResult<CrewMemberSummaryDto>> UpdateCrewBaseAsync(int employeeId, string newBaseAirportIata, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} updating crew base for Employee ID {EmployeeId} to {NewBase}.", performingUser.Identity?.Name, employeeId, newBaseAirportIata);

            // Authorization: Admin or SuperAdmin
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot update crew base.", performingUser.Identity?.Name);
                // Changed return type for failure
                return ServiceResult<CrewMemberSummaryDto>.Failure("Access Denied.");
            }

            var upperIataCode = newBaseAirportIata.ToUpperInvariant();

            try
            {
                // 1. Fetch the active crew member entity
                var crewMember = await _unitOfWork.CrewMembers.GetActiveByEmployeeIdAsync(employeeId);
                if (crewMember == null)
                    // Changed return type for failure
                    return ServiceResult<CrewMemberSummaryDto>.Failure($"Crew member with Employee ID {employeeId} not found.");

                // 2. Validate new airport code
                var airport = await _unitOfWork.Airports.GetByIataCodeAsync(upperIataCode);
                if (airport == null)
                    // Changed return type for failure
                    return ServiceResult<CrewMemberSummaryDto>.Failure($"Airport with IATA code '{upperIataCode}' not found.");

                // 3. Update entity and save changes
                crewMember.CrewBaseAirportId = upperIataCode;
                _unitOfWork.CrewMembers.Update(crewMember);
                await _unitOfWork.SaveChangesAsync();

                // 4. Map the updated entity to the DTO to be returned (assumes necessary related data is loaded)
                // In a real scenario, you might call a special repository method here: 
                var updatedDto = _mapper.Map<CrewMemberSummaryDto>(crewMember);

                _logger.LogInformation("Successfully updated crew base for Employee ID {EmployeeId}.", employeeId);

                // 5. Return success with the updated DTO
                return ServiceResult<CrewMemberSummaryDto>.Success(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating crew base for Employee ID {EmployeeId}.", employeeId);
                // Changed return type for failure
                return ServiceResult<CrewMemberSummaryDto>.Failure("An error occurred while updating the crew base.");
            }
        }
    }
}