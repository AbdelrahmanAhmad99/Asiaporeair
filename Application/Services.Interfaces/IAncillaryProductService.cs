using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks; 
using Application.DTOs.AncillaryProduct;
using Application.Models;

namespace Application.Services.Interfaces
{
    public interface IAncillaryProductService
    { 
        // --- Product Management (Admin) ---

        // Retrieves a single ancillary product by ID.
        Task<ServiceResult<AncillaryProductDto>> GetProductByIdAsync(int productId);

        // Retrieves a paginated list of ancillary products based on filters.
        Task<ServiceResult<PaginatedResult<AncillaryProductDto>>> GetProductsPaginatedAsync(AncillaryProductFilterDto filter, int pageNumber, int pageSize);

        // Creates a new ancillary product definition.
        Task<ServiceResult<AncillaryProductDto>> CreateProductAsync(CreateAncillaryProductDto createDto, ClaimsPrincipal user);

        // Updates an existing ancillary product definition.
        Task<ServiceResult<AncillaryProductDto>> UpdateProductAsync(int productId, UpdateAncillaryProductDto updateDto, ClaimsPrincipal user);

        // Soft-deletes an ancillary product definition.
        Task<ServiceResult> DeleteProductAsync(int productId, ClaimsPrincipal user);

        // --- Ancillary Sales Management ---

        // Retrieves available products potentially applicable to a flight (can be enhanced later).
        Task<ServiceResult<IEnumerable<AncillaryProductDto>>> GetAvailableProductsAsync(int flightInstanceId);

        // Adds ancillary items to an *existing* booking (e.g., via "Manage Booking").
        Task<ServiceResult<AncillarySaleDto>> AddAncillaryToBookingAsync(CreateAncillarySaleDto saleDto, ClaimsPrincipal user);

        // Removes an ancillary item purchase from a booking.
        Task<ServiceResult> RemoveAncillaryFromBookingAsync(int saleId, ClaimsPrincipal user);

        // Retrieves all ancillary sales associated with a specific booking.
        Task<ServiceResult<IEnumerable<AncillarySaleDto>>> GetAncillariesForBookingAsync(int bookingId, ClaimsPrincipal user);

        // (Original method kept for potential use during initial booking creation flow)
        Task<ServiceResult<List<AncillarySaleDto>>> AddAncillariesToBookingAsync(List<AddAncillaryDto> purchasesDto, int bookingId); 
    }
}