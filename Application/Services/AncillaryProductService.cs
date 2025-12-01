using Application.DTOs.AncillaryProduct;
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;  

namespace Application.Services
{
    // Service implementation for managing ancillary products and sales.
    public class AncillaryProductService : IAncillaryProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AncillaryProductService> _logger;
        private readonly IUserRepository _userRepository; // For getting user details if needed

        public AncillaryProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AncillaryProductService> logger, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
        }

        // --- Product Management (Admin) ---

        // Retrieves a single ancillary product by ID.
        public async Task<ServiceResult<AncillaryProductDto>> GetProductByIdAsync(int productId)
        {
            _logger.LogDebug("Retrieving Ancillary Product ID {ProductId}.", productId);
            try
            {
                var product = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(productId);
                if (product == null)
                    return ServiceResult<AncillaryProductDto>.Failure("Ancillary product not found or is inactive.");

                var dto = _mapper.Map<AncillaryProductDto>(product);
                return ServiceResult<AncillaryProductDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Ancillary Product ID {ProductId}.", productId);
                return ServiceResult<AncillaryProductDto>.Failure("An error occurred while retrieving the product.");
            }
        }

        // Retrieves a paginated list of ancillary products based on filters.
        public async Task<ServiceResult<PaginatedResult<AncillaryProductDto>>> GetProductsPaginatedAsync(AncillaryProductFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Searching ancillary products page {PageNumber}.", pageNumber);
            try
            {
                // Build filter expression
                Expression<Func<AncillaryProduct, bool>> filterExpression = p => (filter.IncludeDeleted || !p.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.NameContains))
                    filterExpression = filterExpression.And(p => p.Name.Contains(filter.NameContains));
                if (!string.IsNullOrWhiteSpace(filter.Category))
                    filterExpression = filterExpression.And(p => p.Category == filter.Category);
                if (filter.MinCost.HasValue)
                    filterExpression = filterExpression.And(p => p.BaseCost >= filter.MinCost.Value);
                if (filter.MaxCost.HasValue)
                    filterExpression = filterExpression.And(p => p.BaseCost <= filter.MaxCost.Value);

                var (items, totalCount) = await _unitOfWork.AncillaryProducts.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderBy(p => p.Category).ThenBy(p => p.Name)
                );

                var dtos = _mapper.Map<List<AncillaryProductDto>>(items);
                var paginatedResult = new PaginatedResult<AncillaryProductDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<AncillaryProductDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching ancillary products.");
                return ServiceResult<PaginatedResult<AncillaryProductDto>>.Failure("An error occurred during product search.");
            }
        }

        // Creates a new ancillary product definition.
        public async Task<ServiceResult<AncillaryProductDto>> CreateProductAsync(CreateAncillaryProductDto createDto, ClaimsPrincipal user)
        {
            _logger.LogInformation("User {User} creating ancillary product '{ProductName}'.", user.Identity?.Name, createDto.Name);
            // Authorization: Admin only
            if (!user.IsInRole("Admin") && !user.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot create ancillary products.", user.Identity?.Name);
                return ServiceResult<AncillaryProductDto>.Failure("Access Denied.");
            }

            // Validation: Check name uniqueness
            if (await _unitOfWork.AncillaryProducts.ExistsByNameAsync(createDto.Name))
                return ServiceResult<AncillaryProductDto>.Failure($"Ancillary product with name '{createDto.Name}' already exists.");

            try
            {
                var product = _mapper.Map<AncillaryProduct>(createDto);
                await _unitOfWork.AncillaryProducts.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully created Ancillary Product ID {ProductId}.", product.ProductId);
                var dto = _mapper.Map<AncillaryProductDto>(product);
                return ServiceResult<AncillaryProductDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ancillary product '{ProductName}'.", createDto.Name);
                return ServiceResult<AncillaryProductDto>.Failure("An error occurred while creating the product.");
            }
        }

        // Updates an existing ancillary product definition.
        public async Task<ServiceResult<AncillaryProductDto>> UpdateProductAsync(int productId, UpdateAncillaryProductDto updateDto, ClaimsPrincipal user)
        {
            _logger.LogInformation("User {User} updating Ancillary Product ID {ProductId}.", user.Identity?.Name, productId);
            // Authorization: Admin only
            if (!user.IsInRole("Admin") && !user.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot update ancillary products.", user.Identity?.Name);
                return ServiceResult<AncillaryProductDto>.Failure("Access Denied.");
            }

            var product = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(productId);
            if (product == null)
                return ServiceResult<AncillaryProductDto>.Failure("Ancillary product not found.");

            // Validation: Check name uniqueness (excluding self)
            if (!product.Name.Equals(updateDto.Name, StringComparison.OrdinalIgnoreCase) &&
                await _unitOfWork.AncillaryProducts.ExistsByNameAsync(updateDto.Name))
            {
                return ServiceResult<AncillaryProductDto>.Failure($"Another ancillary product with name '{updateDto.Name}' already exists.");
            }

            try
            {
                _mapper.Map(updateDto, product);
                _unitOfWork.AncillaryProducts.Update(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated Ancillary Product ID {ProductId}.", productId);
                var dto = _mapper.Map<AncillaryProductDto>(product);
                return ServiceResult<AncillaryProductDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Ancillary Product ID {ProductId}.", productId);
                return ServiceResult<AncillaryProductDto>.Failure("An error occurred while updating the product.");
            }
        }

        // Soft-deletes an ancillary product definition.
        public async Task<ServiceResult> DeleteProductAsync(int productId, ClaimsPrincipal user)
        {
            _logger.LogInformation("User {User} deleting Ancillary Product ID {ProductId}.", user.Identity?.Name, productId);
            // Authorization: Admin only
            if (!user.IsInRole("Admin") && !user.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot delete ancillary products.", user.Identity?.Name);
                return ServiceResult.Failure("Access Denied.");
            }

            var product = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(productId);
            if (product == null)
                return ServiceResult.Failure("Ancillary product not found.");

            // Dependency Check: Has this product been sold (check AncillarySale)?
            bool hasSales = await _unitOfWork.AncillarySales.AnyAsync(s => s.ProductId == productId && !s.IsDeleted);
            if (hasSales)
            {
                _logger.LogWarning("Delete failed for Product ID {ProductId}: Product has active sales records.", productId);
                // Option 1: Prevent deletion
                return ServiceResult.Failure($"Cannot delete product '{product.Name}'. It has associated sales records. Consider marking as unavailable instead.");
                // Option 2: Allow deletion (product becomes historical) - Depends on requirements
            }

            try
            {
                _unitOfWork.AncillaryProducts.SoftDelete(product);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully soft-deleted Ancillary Product ID {ProductId}.", productId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Ancillary Product ID {ProductId}.", productId);
                return ServiceResult.Failure("An error occurred while deleting the product.");
            }
        }

        // --- Ancillary Sales Management ---

        
        /// <summary>
        /// Retrieves available ancillary products applicable to a *specific* flight instance.
        /// </summary>
        /// <param name="flightInstanceId">The ID of the flight instance to check availability for.</param>
        /// <returns>A ServiceResult containing a list of applicable ancillary products.</returns>
        public async Task<ServiceResult<IEnumerable<AncillaryProductDto>>> GetAvailableProductsAsync(int flightInstanceId)
        {
            _logger.LogInformation("Retrieving available ancillary products for FlightInstanceId {FlightId}.", flightInstanceId);

            try
            {
                // 1. --- VALIDATION: Check if the flight instance exists ---
                // We must fetch flight details to apply filters.
                // (Assuming IUnitOfWork has FlightInstances repo with GetWithDetailsAsync like other repos)
                var flight = await _unitOfWork.FlightInstances.GetWithDetailsAsync(flightInstanceId);

                if (flight == null || flight.Schedule == null || flight.Schedule.Route == null)
                {
                    _logger.LogWarning("FlightInstanceId {FlightId} not found or is missing required details (Schedule/Route).", flightInstanceId);
                    // This now correctly handles the user's "1011" example.
                    return ServiceResult<IEnumerable<AncillaryProductDto>>.Failure("Flight instance not found.");
                }

                // 2. --- FILTERING: Get all active products and then filter them ---
                var allProducts = await _unitOfWork.AncillaryProducts.GetAvailableAsync(); // Gets all active products

                // Calculate flight duration for potential filtering
                var flightDuration = flight.ScheduledArrival - flight.ScheduledDeparture;

                // Apply professional filtering rules based on flight context
                var availableProducts = allProducts.Where(product =>
                {
                    // --- Rule 1: Filter by Category ---
                    // Exclude "SEAT" category, as seat selection is handled by the dedicated
                    // seat map service (evident from BookingPassengerRepository.cs).
                    if (product.Category.Equals("SEAT", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    // --- Rule 2: (Example) Filter by Flight Duration ---
                    // Only offer "Premium Meal Upgrade" on flights longer than 2 hours.
                    if (product.Category.Equals("MEAL", StringComparison.OrdinalIgnoreCase) && flightDuration.TotalHours < 2)
                    {
                        _logger.LogDebug("Excluding product '{ProductName}' (ID: {ProductId}) for Flight {FlightId}: Flight duration ({Duration}h) is too short.",
                            product.Name, product.ProductId, flightInstanceId, flightDuration.TotalHours);
                        return false;
                    }
                     

                    // Default: Product is available
                    return true;
                }).ToList();

                // 3. --- Return Filtered List ---
                _logger.LogInformation("Found {Count} applicable ancillary products for FlightInstanceId {FlightId}.", availableProducts.Count(), flightInstanceId);
                var dtos = _mapper.Map<IEnumerable<AncillaryProductDto>>(availableProducts);
                return ServiceResult<IEnumerable<AncillaryProductDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available ancillary products for FlightId {FlightId}.", flightInstanceId);
                return ServiceResult<IEnumerable<AncillaryProductDto>>.Failure("An error occurred while retrieving available products.");
            }
        }

        // Adds ancillary items to an *existing* booking.
        public async Task<ServiceResult<AncillarySaleDto>> AddAncillaryToBookingAsync(CreateAncillarySaleDto saleDto, ClaimsPrincipal user)
        {
            _logger.LogInformation("User {User} adding ProductId {ProductId} (Qty: {Qty}) to BookingId {BookingId}.",
                user.Identity?.Name, saleDto.ProductId, saleDto.Quantity, saleDto.BookingId);

            try
            {
                // 1. Validate Booking and User access
                var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(saleDto.BookingId);
                if (booking == null) return ServiceResult<AncillarySaleDto>.Failure("Booking not found.");
                var authResult = await AuthorizeBookingAccessAsync(user, booking); // Use helper
                if (!authResult.IsSuccess) return ServiceResult<AncillarySaleDto>.Failure(authResult.Errors);

                // 2. Validate Product
                var product = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(saleDto.ProductId);
                if (product == null) return ServiceResult<AncillarySaleDto>.Failure("Ancillary product not found or unavailable.");

                // 3. TODO: Validate Applicability (e.g., is this meal allowed on this flight/cabin?)

                // 4. Create Sale Entry
                var sale = new AncillarySale
                {
                    BookingId = saleDto.BookingId,
                    ProductId = saleDto.ProductId,
                    Quantity = saleDto.Quantity,
                    PricePaid = (product.BaseCost ?? 0) * saleDto.Quantity, // Price at time of addition
                    IsDeleted = false
                    // Set PassengerId or SegmentId if provided in DTO and needed
                };

                await _unitOfWork.AncillarySales.AddAsync(sale);

                // 5. Update Booking Total Price
                booking.PriceTotal = (booking.PriceTotal ?? 0) + sale.PricePaid.Value;
                _unitOfWork.Bookings.Update(booking);

                await _unitOfWork.SaveChangesAsync(); // Save sale and booking price update

                _logger.LogInformation("Successfully added Sale ID {SaleId} for Product ID {ProductId} to Booking ID {BookingId}.", sale.SaleId, sale.ProductId, sale.BookingId);

                // Map result DTO
                var resultDto = _mapper.Map<AncillarySaleDto>(sale);
                resultDto.ProductName = product.Name; // Add product name

                return ServiceResult<AncillarySaleDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ProductId {ProductId} to BookingId {BookingId}.", saleDto.ProductId, saleDto.BookingId);
                return ServiceResult<AncillarySaleDto>.Failure("An internal error occurred while adding the ancillary item.");
            }
        }

        // Removes an ancillary item purchase from a booking.
        public async Task<ServiceResult> RemoveAncillaryFromBookingAsync(int saleId, ClaimsPrincipal user)
        {
            _logger.LogInformation("User {User} removing AncillarySale ID {SaleId}.", user.Identity?.Name, saleId);
            try
            {
                var sale = await _unitOfWork.AncillarySales.GetActiveByIdAsync(saleId);
                if (sale == null) return ServiceResult.Failure("Ancillary purchase not found.");

                var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(sale.BookingId);
                if (booking == null) return ServiceResult.Failure("Associated booking not found."); // Should not happen ideally
                var authResult = await AuthorizeBookingAccessAsync(user, booking);
                if (!authResult.IsSuccess) return authResult;

                // TODO: Add validation - Can it be removed? (e.g., too close to flight?)

                // Update Booking Total Price (subtract removed item cost)
                booking.PriceTotal = (booking.PriceTotal ?? 0) - (sale.PricePaid ?? 0);
                if (booking.PriceTotal < 0) booking.PriceTotal = 0; // Avoid negative total
                _unitOfWork.Bookings.Update(booking);

                // Soft delete the sale record
                _unitOfWork.AncillarySales.SoftDelete(sale);

                await _unitOfWork.SaveChangesAsync(); // Save price change and deletion

                _logger.LogInformation("Successfully removed AncillarySale ID {SaleId} from Booking ID {BookingId}.", saleId, sale.BookingId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing AncillarySale ID {SaleId}.", saleId);
                return ServiceResult.Failure("An internal error occurred while removing the ancillary item.");
            }
        }

        // Retrieves all ancillary sales associated with a specific booking.
        public async Task<ServiceResult<IEnumerable<AncillarySaleDto>>> GetAncillariesForBookingAsync(int bookingId, ClaimsPrincipal user)
        {
            _logger.LogDebug("Retrieving ancillary sales for Booking ID {BookingId}.", bookingId);
            try
            {
                var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(bookingId);
                if (booking == null) return ServiceResult<IEnumerable<AncillarySaleDto>>.Failure("Booking not found.");
                var authResult = await AuthorizeBookingAccessAsync(user, booking);
                if (!authResult.IsSuccess) return ServiceResult<IEnumerable<AncillarySaleDto>>.Failure(authResult.Errors);

                var sales = await _unitOfWork.AncillarySales.GetByBookingAsync(bookingId); // Repo includes Product
                var dtos = _mapper.Map<IEnumerable<AncillarySaleDto>>(sales);
                return ServiceResult<IEnumerable<AncillarySaleDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ancillary sales for Booking ID {BookingId}.", bookingId);
                return ServiceResult<IEnumerable<AncillarySaleDto>>.Failure("An error occurred while retrieving ancillary items.");
            }
        }

        // Original method for adding during initial booking (Kept but might be refactored)
        // Note: This adds sales but doesn't update the booking total price here. Assumes caller (BookingService) handles total price calculation.
        public async Task<ServiceResult<List<AncillarySaleDto>>> AddAncillariesToBookingAsync(List<AddAncillaryDto> purchasesDto, int bookingId)
        {
            _logger.LogInformation("Adding {Count} ancillary items during creation of Booking ID {BookingId}.", purchasesDto.Count, bookingId);
            var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(bookingId);
            if (booking == null)
            {
                return ServiceResult<List<AncillarySaleDto>>.Failure("Booking not found.");
            }

            var addedSales = new List<AncillarySale>();
            var addedDtos = new List<AncillarySaleDto>();
            var errors = new List<string>();

            foreach (var purchaseDto in purchasesDto)
            {
                var product = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(purchaseDto.ProductId);
                if (product == null)
                {
                    errors.Add($"Product with ID {purchaseDto.ProductId} not found.");
                    _logger.LogWarning("Ancillary Product ID {ProductId} not found for Booking ID {BookingId} (during initial add).", purchaseDto.ProductId, bookingId);
                    continue; // Skip this product
                }

                var sale = new AncillarySale
                {
                    BookingId = bookingId,
                    ProductId = purchaseDto.ProductId,
                    Quantity = purchaseDto.Quantity,
                    PricePaid = (product.BaseCost ?? 0) * purchaseDto.Quantity,
                    IsDeleted = false
                    // SegmentId, PassengerId if needed
                };
                addedSales.Add(sale);
            }

            if (errors.Any())
            {
                // Decide: Fail the whole operation or just add the valid ones?
                // Let's proceed with valid ones but log the errors.
                _logger.LogWarning("Errors encountered while adding ancillaries to Booking {BookingId}: {Errors}", bookingId, string.Join("; ", errors));
                // Return failure if any error occurred?
                // return ServiceResult<List<AncillarySaleDto>>.Failure(errors);
            }

            if (!addedSales.Any())
            {
                return ServiceResult<List<AncillarySaleDto>>.Success(new List<AncillarySaleDto>()); // No valid items to add
            }

            try
            {
                await _unitOfWork.AncillarySales.AddRangeAsync(addedSales);
                await _unitOfWork.SaveChangesAsync(); // Save the added sales

                // Map to DTOs for response
                foreach (var s in addedSales)
                {
                    var product = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(s.ProductId); // Fetch again for name
                    var dto = _mapper.Map<AncillarySaleDto>(s);
                    dto.ProductName = product?.Name ?? "N/A";
                    addedDtos.Add(dto);
                }

                return ServiceResult<List<AncillarySaleDto>>.Success(addedDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error saving ancillary sales for Booking ID {BookingId} (during initial add).", bookingId);
                return ServiceResult<List<AncillarySaleDto>>.Failure($"Failed to save ancillary items: {ex.Message}");
            }
        }

        // --- Helper Methods ---

        // Duplicated from BookingService/SeatService - Refactor needed.
        private async Task<ServiceResult> AuthorizeBookingAccessAsync(ClaimsPrincipal user, Booking booking)
        {
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId)) return ServiceResult.Failure("Authentication required.");

            var userProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(appUserId);
            if (userProfile != null && booking.UserId == userProfile.UserId)
            {
                return ServiceResult.Success(); // Owner
            }

            if (user.IsInRole("Admin") || user.IsInRole("Supervisor") || user.IsInRole("SuperAdmin"))
            {
                return ServiceResult.Success(); // Admin role
            }

            _logger.LogWarning("User {UserId} unauthorized attempt to access Booking ID {BookingId} owned by User ID {OwnerId}.", appUserId, booking.BookingId, booking.UserId);
            return ServiceResult.Failure("Access denied to this booking.");
        }
    }
}