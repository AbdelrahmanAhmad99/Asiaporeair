using Application.DTOs.Crew;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Interface for managing flight crew (Pilots, Attendants) and their certifications.
    public interface ICrewManagementService
    {
        // Retrieves a paginated list of crew members based on filters.
        Task<ServiceResult<PaginatedResult<CrewMemberSummaryDto>>> GetCrewMembersPaginatedAsync(CrewFilterDto filter, int pageNumber, int pageSize);

        // Retrieves detailed information for a specific crew member by Employee ID.
        Task<ServiceResult<CrewMemberDetailDto>> GetCrewMemberDetailsByIdAsync(int employeeId);

        // Retrieves all active certifications for a specific crew member.
        Task<ServiceResult<IEnumerable<CertificationDto>>> GetCertificationsForCrewMemberAsync(int employeeId);

        // Adds a new certification record for a crew member.
        Task<ServiceResult<CertificationDto>> AddCertificationAsync(int employeeId, CreateCertificationDto dto, ClaimsPrincipal performingUser);

        // Updates an existing certification record.
        Task<ServiceResult<CertificationDto>> UpdateCertificationAsync(int certId, CreateCertificationDto dto, ClaimsPrincipal performingUser);

        // Deletes a certification record (soft delete).
        Task<ServiceResult> DeleteCertificationAsync(int certId, ClaimsPrincipal performingUser);

        // Retrieves crew members whose certifications are expiring soon or have expired.
        Task<ServiceResult<IEnumerable<CrewMemberSummaryDto>>> GetCrewWithExpiringCertificationsAsync(int daysUntilExpiry = 30);

        // Gets analytics data related to flight crew.
        Task<ServiceResult<CrewAnalyticsDto>> GetCrewDashboardAnalyticsAsync();

        // Updates the base airport for a crew member.
        Task<ServiceResult<CrewMemberSummaryDto>> UpdateCrewBaseAsync(int employeeId, string newBaseAirportIata, ClaimsPrincipal performingUser);
    }
}