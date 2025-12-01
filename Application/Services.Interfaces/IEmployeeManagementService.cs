using Application.DTOs.Employee;
using Application.Models;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Interface for HR and operational management of employees.
    // This complements AuthService, which handles registration and profile updates.
    public interface IEmployeeManagementService
    {
        // Retrieves a paginated and filtered list of all employees.
        Task<ServiceResult<PaginatedResult<EmployeeSummaryDto>>> GetEmployeesPaginatedAsync(EmployeeFilterDto filter, int pageNumber, int pageSize);

        // Retrieves a single employee's summary by their Employee ID.
        Task<ServiceResult<EmployeeSummaryDto>> GetEmployeeSummaryByIdAsync(int employeeId);

        // Retrieves a single employee's summary by their AppUser ID.
        Task<ServiceResult<EmployeeSummaryDto>> GetEmployeeSummaryByAppUserIdAsync(string appUserId);

        // Retrieves all employees belonging to a specific role (e.g., all pilots).
        Task<ServiceResult<IEnumerable<EmployeeSummaryDto>>> GetEmployeesByRoleAsync(UserType role);

        // Retrieves employees hired within a specific date range.
        Task<ServiceResult<IEnumerable<EmployeeSummaryDto>>> GetEmployeesHiredByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Gets analytics data for the HR dashboard.
        Task<ServiceResult<EmployeeAnalyticsDto>> GetEmployeeDashboardAnalyticsAsync();

        // Updates an employee's salary (HR-specific action).
        Task<ServiceResult<EmployeeSummaryDto>> UpdateEmployeeSalaryAsync(int employeeId, UpdateSalaryRequestDto dto, ClaimsPrincipal performingUser);

        // Soft-deletes an employee account (administrative action).
        Task<ServiceResult> DeactivateEmployeeAsync(int employeeId, ClaimsPrincipal performingUser);

        // Reactivates a soft-deleted employee account.
        Task<ServiceResult> ReactivateEmployeeAsync(int employeeId, ClaimsPrincipal performingUser);
    }
}