using Application.DTOs.Employee;
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services
{
    // Service implementation for HR and operational management of employees.
    public class EmployeeManagementService : IEmployeeManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeManagementService> _logger;
        private readonly IUserRepository _userRepository;  

        // Constructor for dependency injection
        public EmployeeManagementService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<EmployeeManagementService> logger, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
        }

        // Retrieves a paginated and filtered list of all employees.
        public async Task<ServiceResult<PaginatedResult<EmployeeSummaryDto>>> GetEmployeesPaginatedAsync(EmployeeFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Attempting to retrieve paginated employees for page {PageNumber}. Filter Role: {Role}", pageNumber, filter.Role);
            try
            {
                // The method should handle filtering by filter.Role (UserType enum)
                var (appUsers, totalCount) = await _userRepository.GetPaginatedUsersAsync(pageNumber, pageSize, filter.Role);

                // Filter further if needed (e.g., NameContains)
                var filteredAppUsers = appUsers;
                if (!string.IsNullOrWhiteSpace(filter.NameContains))
                {
                    var name = filter.NameContains.ToLower();
                    filteredAppUsers = filteredAppUsers.Where(u =>
                        u.FirstName.ToLower().Contains(name) ||
                        u.LastName.ToLower().Contains(name));
                }
                var upperCrewBaseAirportIata = filter.CrewBaseAirportIata.ToUpper();
                // We must get the Employee/Crew details for each AppUser
                var summaryDtos = new List<EmployeeSummaryDto>();
                foreach (var user in filteredAppUsers)
                {
                    // This is N+1, but necessary given the repo structure.
                    var summary = await GetEmployeeSummaryByAppUserIdAsync(user.Id);
                    if (summary.IsSuccess)
                    {
                         
                        // Apply additional filters
                        if (filter.IncludeDeleted == false && summary.Data.IsActive == false) continue;
                        if (upperCrewBaseAirportIata != null && summary.Data.CrewBaseAirportIata != upperCrewBaseAirportIata) continue;

                        summaryDtos.Add(summary.Data);
                    }
                }

                var paginatedResult = new PaginatedResult<EmployeeSummaryDto>(summaryDtos, totalCount, pageNumber, pageSize);
                _logger.LogInformation("Successfully retrieved {Count} employees for page {PageNumber}.", summaryDtos.Count, pageNumber);
                return ServiceResult<PaginatedResult<EmployeeSummaryDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated employees.");
                return ServiceResult<PaginatedResult<EmployeeSummaryDto>>.Failure("An error occurred while retrieving employees.");
            }
        }

        // Retrieves a single employee's summary by their Employee ID.
        public async Task<ServiceResult<EmployeeSummaryDto>> GetEmployeeSummaryByIdAsync(int employeeId)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetWithRoleDetailsAsync(employeeId);
                if (employee == null)
                {
                    _logger.LogWarning("Employee ID {EmployeeId} not found.", employeeId);
                    return ServiceResult<EmployeeSummaryDto>.Failure($"Employee with ID {employeeId} not found.");
                }

                var dto = _mapper.Map<EmployeeSummaryDto>(employee);
                return ServiceResult<EmployeeSummaryDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee ID {EmployeeId}.", employeeId);
                return ServiceResult<EmployeeSummaryDto>.Failure("An error occurred while retrieving the employee summary.");
            }
        }

        // Retrieves a single employee's summary by their AppUser ID.
        public async Task<ServiceResult<EmployeeSummaryDto>> GetEmployeeSummaryByAppUserIdAsync(string appUserId)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByAppUserIdAsync(appUserId);
                if (employee == null)
                {
                    // This user might be a Passenger, not an employee.
                    _logger.LogInformation("AppUser ID {AppUserId} is not an employee.", appUserId);
                    return ServiceResult<EmployeeSummaryDto>.Failure($"No employee profile found for AppUser ID {appUserId}.");
                }

                var dto = _mapper.Map<EmployeeSummaryDto>(employee);
                return ServiceResult<EmployeeSummaryDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee by AppUser ID {AppUserId}.", appUserId);
                return ServiceResult<EmployeeSummaryDto>.Failure("An error occurred while retrieving the employee summary.");
            }
        }

        // Retrieves all employees belonging to a specific role.
        public async Task<ServiceResult<IEnumerable<EmployeeSummaryDto>>> GetEmployeesByRoleAsync(UserType role)
        {
            _logger.LogInformation("Retrieving all employees for role {Role}.", role);
            try
            {
                var employees = new List<Employee>();

                // Use the specific methods from UserRepository based on the UserType enum value
                if (role == UserType.Pilot)
                {
                    var pilots = await _userRepository.GetAllActivePilotsAsync();
                    employees = pilots.Where(p => p.CrewMember?.Employee != null).Select(p => p.CrewMember!.Employee!).ToList();
                }
                else if (role == UserType.Attendant)
                {
                    var attendants = await _userRepository.GetAllActiveAttendantsAsync();
                    employees = attendants.Where(a => a.CrewMember?.Employee != null).Select(a => a.CrewMember!.Employee!).ToList();
                }
                else if (role == UserType.Admin)
                {
                    var admins = await _userRepository.GetAllActiveAdminsAsync();
                    employees = admins.Where(a => a.Employee != null).Select(a => a.Employee!).ToList();
                }
                else if (role == UserType.Supervisor)
                {
                    var supervisors = await _userRepository.GetAllActiveSupervisorsAsync();
                    employees = supervisors.Where(s => s.Employee != null).Select(s => s.Employee!).ToList();
                }
                // Check for non-employee roles that shouldn't be searched (e.g., User, SuperAdmin)
                else if (role == UserType.User || role == UserType.SuperAdmin)
                {
                    // This is typically handled by separate methods or access control.
                    // For this function, we assume these roles are not meant for bulk retrieval here.
                    return ServiceResult<IEnumerable<EmployeeSummaryDto>>.Failure($"Retrieving bulk data for role '{role}' is not supported by this endpoint.");
                }
                else
                {
                    // Catch any unhandled/invalid enum values
                    return ServiceResult<IEnumerable<EmployeeSummaryDto>>.Failure($"Invalid or unhandled employee role specified: {role}.");
                }

                // Map to DTOs and return
                var dtos = _mapper.Map<IEnumerable<EmployeeSummaryDto>>(employees);
                _logger.LogInformation("Successfully retrieved {Count} employees for role {Role}.", dtos.Count(), role);
                return ServiceResult<IEnumerable<EmployeeSummaryDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees by role {Role}.", role);
                return ServiceResult<IEnumerable<EmployeeSummaryDto>>.Failure("An error occurred while retrieving employees.");
            }
        }

        // Retrieves employees hired within a specific date range.
        public async Task<ServiceResult<IEnumerable<EmployeeSummaryDto>>> GetEmployeesHiredByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Retrieving employees hired between {StartDate} and {EndDate}.", startDate, endDate);
            try
            {
                var employees = await _unitOfWork.Employees.GetByHireDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<EmployeeSummaryDto>>(employees);
                return ServiceResult<IEnumerable<EmployeeSummaryDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees by hire date range.");
                return ServiceResult<IEnumerable<EmployeeSummaryDto>>.Failure("An error occurred while retrieving the report.");
            }
        }

        // Gets analytics data for the HR dashboard.
        public async Task<ServiceResult<EmployeeAnalyticsDto>> GetEmployeeDashboardAnalyticsAsync()
        {
            _logger.LogInformation("Generating employee dashboard analytics.");
            try
            {
                var dto = new EmployeeAnalyticsDto
                {
                    TotalActiveEmployees = await _userRepository.GetTotalActiveUserCountAsync() - await _userRepository.GetUserCountByRoleAsync(UserType.User),
                    TotalPilots = await _userRepository.GetUserCountByRoleAsync(UserType.Pilot),
                    TotalAttendants = await _userRepository.GetUserCountByRoleAsync(UserType.Attendant),
                    TotalAdmins = await _userRepository.GetUserCountByRoleAsync(UserType.Admin),
                    TotalSupervisors = await _userRepository.GetUserCountByRoleAsync(UserType.Supervisor)
                };

                // Get all employees for salary and base analytics
                var allEmployees = await _unitOfWork.Employees.GetAllActiveWithAppUserAsync();

                if (allEmployees.Any())
                {
                    dto.AverageSalary = allEmployees.Where(e => e.Salary.HasValue).Average(e => e.Salary ?? 0);
                }

                // Get crew members for base analytics
                var allCrew = await _unitOfWork.CrewMembers.GetAllAsync(); // Assumes this includes IsDeleted check
                dto.EmployeesByBase = allCrew
                    .Where(c => !string.IsNullOrEmpty(c.CrewBaseAirportId))
                    .GroupBy(c => c.CrewBaseAirportId)
                    .ToDictionary(g => g.Key, g => g.Count());

                return ServiceResult<EmployeeAnalyticsDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating employee dashboard analytics.");
                return ServiceResult<EmployeeAnalyticsDto>.Failure("An error occurred while generating analytics.");
            }
        }

        // Updates an employee's salary (HR-specific action).
        public async Task<ServiceResult<EmployeeSummaryDto>> UpdateEmployeeSalaryAsync(int employeeId, UpdateSalaryRequestDto dto, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {PerformingUser} attempting to update salary for Employee ID {EmployeeId}.", performingUser.Identity?.Name, employeeId);

            // Authorization: Only SuperAdmin can change salary
            if (!performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} is not a SuperAdmin.", performingUser.Identity?.Name);
                // Changed return type for failure
                return ServiceResult<EmployeeSummaryDto>.Failure("Access denied. Only SuperAdmins can update salaries.");
            }

            try
            {
                // 1. Fetch the active employee entity
                var employee = await _unitOfWork.Employees.GetActiveByIdAsync(employeeId);
                if (employee == null)
                {
                    _logger.LogWarning("Update salary failed: Employee ID {EmployeeId} not found.", employeeId);
                    // Changed return type for failure
                    return ServiceResult<EmployeeSummaryDto>.Failure($"Employee with ID {employeeId} not found.");
                }

                // 2. Update the salary and save changes
                employee.Salary = dto.NewSalary;
                _unitOfWork.Employees.Update(employee);
                await _unitOfWork.SaveChangesAsync();

                // 3. Fetch the fully updated employee summary to return
                // Assuming there is a GetEmployeeSummaryByIdAsync method that returns EmployeeSummaryDto
                var updatedSummaryResult = await GetEmployeeSummaryByIdAsync(employeeId);

                if (!updatedSummaryResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to retrieve updated employee summary for ID {EmployeeId}.", employeeId);
                    // If fetching the summary fails, return a success but without data or handle as a partial error
                    return ServiceResult<EmployeeSummaryDto>.Failure("Salary updated successfully, but failed to retrieve the updated employee data.");
                }

                _logger.LogInformation("Successfully updated salary for Employee ID {EmployeeId}.", employeeId);
                // 4. Return the updated DTO
                return ServiceResult<EmployeeSummaryDto>.Success(updatedSummaryResult.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating salary for Employee ID {EmployeeId}.", employeeId);
                // Changed return type for failure
                return ServiceResult<EmployeeSummaryDto>.Failure("An error occurred while updating the employee's salary.");
            }
        }

        // Soft-deletes an employee account (administrative action).
        public async Task<ServiceResult> DeactivateEmployeeAsync(int employeeId, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {PerformingUser} attempting to deactivate Employee ID {EmployeeId}.", performingUser.Identity?.Name, employeeId);

            var employee = await _unitOfWork.Employees.GetWithRoleDetailsAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Deactivation failed: Employee ID {EmployeeId} not found.", employeeId);
                return ServiceResult.Failure($"Employee with ID {employeeId} not found.");
            }

            var appUser = employee.AppUser;
            if (appUser == null)
                return ServiceResult.Failure("Employee is not linked to a user account.");

            // Authorization check
            if (appUser.UserType == UserType.SuperAdmin)
            {
                _logger.LogWarning("Deactivation failed: Cannot deactivate a SuperAdmin account.");
                return ServiceResult.Failure("Cannot deactivate a SuperAdmin account.");
            }

            if (!performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} is not a SuperAdmin.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access denied. Only SuperAdmins can deactivate employees.");
            }

            try
            {
                // Soft delete all related entities
                _unitOfWork.Employees.SoftDelete(employee);
                if (employee.CrewMember != null) _unitOfWork.CrewMembers.SoftDelete(employee.CrewMember);

                // Use UserRepository to soft delete the AppUser
                await _userRepository.SoftDeleteUserAsync(appUser.Id);

                _logger.LogInformation("Successfully deactivated Employee ID {EmployeeId} and AppUser {AppUserId}.", employeeId, appUser.Id);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating Employee ID {EmployeeId}.", employeeId);
                return ServiceResult.Failure("An error occurred during deactivation.");
            }
        }

        // Reactivates a soft-deleted employee account.
        public async Task<ServiceResult> ReactivateEmployeeAsync(int employeeId, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {PerformingUser} attempting to reactivate Employee ID {EmployeeId}.", performingUser.Identity?.Name, employeeId);

            // Authorization check
            if (!performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} is not a SuperAdmin.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access denied. Only SuperAdmins can reactivate employees.");
            }

            try
            {
                // Must query including deleted
                var employee = (await _unitOfWork.Employees.GetAllIncludingDeletedAsync()).FirstOrDefault(e => e.EmployeeId == employeeId);
                if (employee == null)
                {
                    _logger.LogWarning("Reactivation failed: Employee ID {EmployeeId} not found.", employeeId);
                    return ServiceResult.Failure($"Employee with ID {employeeId} not found.");
                }

                // Reactivate AppUser first
                var reactivateResult = await _userRepository.ReactivateUserAsync(employee.AppUserId);
                if (!reactivateResult)
                {
                    return ServiceResult.Failure("Failed to reactivate the user account in Identity.");
                }

                // Reactivate employee record
                employee.IsDeleted = false;
                _unitOfWork.Employees.Update(employee);

                // Reactivate crew member record if it exists
                var crewMember = (await _unitOfWork.CrewMembers.GetAllIncludingDeletedAsync()).FirstOrDefault(c => c.EmployeeId == employeeId);
                if (crewMember != null)
                {
                    crewMember.IsDeleted = false;
                    _unitOfWork.CrewMembers.Update(crewMember);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully reactivated Employee ID {EmployeeId}.", employeeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating Employee ID {EmployeeId}.", employeeId);
                return ServiceResult.Failure("An error occurred during reactivation.");
            }
        }
    }
}