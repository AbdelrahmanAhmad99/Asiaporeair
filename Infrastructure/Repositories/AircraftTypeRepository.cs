using Domain.Entities;
using Domain.Repositories.Interfaces;
using Infrastructure.Data;  
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{ 
    public class AircraftTypeRepository : GenericRepository<AircraftType>, IAircraftTypeRepository
    {
        public AircraftTypeRepository(ApplicationDbContext context) : base(context)
        {
        }
         
        public async Task<AircraftType?> GetActiveByIdAsync(int typeId)
        {
            // Use FirstOrDefaultAsync with the primary key and IsDeleted check
            return await _dbSet
                .Where(at => at.TypeId == typeId && !at.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<IEnumerable<AircraftType>> FindByModelAsync(string model)
        {
            return await _dbSet
                .Where(at => !at.IsDeleted && EF.Functions.Like(at.Model, $"%{model}%"))
                .OrderBy(at => at.Manufacturer).ThenBy(at => at.Model)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<AircraftType>> GetByManufacturerAsync(string manufacturer)
        {
            var upperManufacturer = manufacturer.ToUpper();
            return await _dbSet
                .Where(at => !at.IsDeleted && at.Manufacturer.ToUpper() == upperManufacturer)
                .OrderBy(at => at.Model)
                .ToListAsync();
        }
         
        public async Task<IEnumerable<AircraftType>> FindByCriteriaAsync(int? minRangeKm = null, int? minSeats = null, decimal? minCargoCapacity = null)
        {
            var query = _dbSet.Where(at => !at.IsDeleted);

            if (minRangeKm.HasValue)
            {
                query = query.Where(at => at.RangeKm >= minRangeKm.Value);
            }
            if (minSeats.HasValue)
            {
                query = query.Where(at => at.MaxSeats >= minSeats.Value);
            }
            if (minCargoCapacity.HasValue)
            {
                query = query.Where(at => at.CargoCapacity >= minCargoCapacity.Value);
            }

            return await query.OrderBy(at => at.Manufacturer).ThenBy(at => at.Model).ToListAsync();
        }
         
        public async Task<IEnumerable<AircraftType>> GetAllIncludingDeletedAsync()
        {
            // Bypasses the default soft delete filter if present in the base class override
            return await _context.AircraftTypes.IgnoreQueryFilters().ToListAsync(); // Use IgnoreQueryFilters if global filters are set 
        }
         
        public async Task<IEnumerable<AircraftType>> GetAllActiveAsync()
        {
            return await _dbSet.Where(at => !at.IsDeleted).ToListAsync();
        }
         
        public async Task<AircraftType?> GetWithAircraftAsync(int typeId)
        {
            return await _dbSet
                .Include(at => at.Aircrafts)  
                .Where(at => at.TypeId == typeId && !at.IsDeleted)
                .FirstOrDefaultAsync();
        }
         
        public async Task<bool> ExistsByModelAsync(string model)
        {
            var upperModel = model.ToUpper();
            return await _dbSet.AnyAsync(at => at.Model.ToUpper() == upperModel);
        }
         
        public async Task<bool> ExistsByManufacturerAsync(string manufacturer)
        {
            var upperManufacturer = manufacturer.ToUpper();
            return await _dbSet.AnyAsync(at => at.Manufacturer.ToUpper() == upperManufacturer);
        }
         
        public override async Task<IEnumerable<AircraftType>> GetAllAsync()
        {
            return await _dbSet.Where(at => !at.IsDeleted).ToListAsync();
        }
    }
}