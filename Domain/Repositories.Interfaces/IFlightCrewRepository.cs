using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for FlightCrew entity, extending the generic repository.
    /// Manages the assignment of CrewMembers to specific FlightInstances.
    /// Critical for airport operations, crew scheduling, and tracking flight personnel.
    /// </summary>
    public interface IFlightCrewRepository : IGenericRepository<FlightCrew>
    {
        /// <summary>
        /// Retrieves an active FlightCrew assignment by its composite primary key.
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance.</param>
        /// <param name="crewMemberEmployeeId">The Employee ID of the crew member.</param>
        /// <returns>The FlightCrew entity if found and active; otherwise, null.</returns>
        Task<FlightCrew?> GetActiveByIdAsync(int flightInstanceId, int crewMemberEmployeeId);

        /// <summary>
        /// Retrieves all active crew members assigned to a specific flight instance.
        /// Includes CrewMember and Employee details.
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance.</param>
        /// <returns>An enumerable collection of active FlightCrew entities with CrewMember details for the flight.</returns>
        Task<IEnumerable<FlightCrew>> GetCrewForFlightAsync(int flightInstanceId);

        /// <summary>
        /// Retrieves all active flight instances a specific crew member is assigned to.
        /// Includes FlightInstance and Schedule details. Useful for viewing a crew member's schedule.
        /// </summary>
        /// <param name="crewMemberEmployeeId">The Employee ID of the crew member.</param>
        /// <returns>An enumerable collection of active FlightCrew entities with FlightInstance details for the crew member.</returns>
        Task<IEnumerable<FlightCrew>> GetFlightsForCrewMemberAsync(int crewMemberEmployeeId);

        /// <summary>
        /// Retrieves active flight assignments for a crew member within a specific date range (based on flight instance departure time).
        /// </summary>
        /// <param name="crewMemberEmployeeId">The Employee ID of the crew member.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>An enumerable collection of active FlightCrew assignments within the date range.</returns>
        Task<IEnumerable<FlightCrew>> GetAssignmentsForCrewMemberByDateRangeAsync(int crewMemberEmployeeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Adds multiple FlightCrew assignments, typically when assigning a full crew to a flight.
        /// </summary>
        /// <param name="flightCrewAssignments">An enumerable collection of FlightCrew entities to add.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task AddMultipleAssignmentsAsync(IEnumerable<FlightCrew> flightCrewAssignments);

        /// <summary>
        /// Removes multiple FlightCrew assignments, typically when changing crew for a flight.
        /// Uses soft delete if implemented, otherwise hard delete.
        /// </summary>
        /// <param name="flightCrewAssignments">An enumerable collection of FlightCrew entities to remove.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task RemoveMultipleAssignmentsAsync(IEnumerable<FlightCrew> flightCrewAssignments);

        /// <summary>
        /// Retrieves all FlightCrew assignments, including those marked as soft-deleted.
        /// For administrative review or historical assignment data.
        /// </summary>
        /// <returns>An enumerable collection of all FlightCrew entities.</returns>
        Task<IEnumerable<FlightCrew>> GetAllIncludingDeletedAsync();

        /// <summary>
        /// Retrieves all active (not soft-deleted) FlightCrew assignments.
        /// </summary>
        /// <returns>An enumerable collection of active FlightCrew entities.</returns>
        Task<IEnumerable<FlightCrew>> GetAllActiveAsync();

        /// <summary>
        /// Checks if a specific crew member is already assigned to a specific flight instance (active assignment).
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance.</param>
        /// <param name="crewMemberEmployeeId">The Employee ID of the crew member.</param>
        /// <returns>True if an active assignment exists; otherwise, false.</returns>
        Task<bool> ExistsAssignmentAsync(int flightInstanceId, int crewMemberEmployeeId);
    }
}