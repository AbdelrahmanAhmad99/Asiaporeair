using Application.DTOs.Country;
using Application.Models; 
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{ 
    public class CountryService : ICountryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;   

        public CountryService(IUnitOfWork unitOfWork, IMapper mapper)  
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all active countries, ordered by name.
        /// </summary>
        public async Task<ServiceResult<IEnumerable<CountryDto>>> GetAllActiveCountriesAsync()
        {
            // Use the specific repository method for clarity and potentially better performance
            var countries = await _unitOfWork.Countries.GetAllActiveAsync();
            var countryDtos = countries.OrderBy(c => c.Name).Select(c => new CountryDto
            {
                IsoCode = c.IsoCode,
                Name = c.Name,
                Continent = c.Continent // Map ContinentFk to Continent in DTO
            });
            return ServiceResult<IEnumerable<CountryDto>>.Success(countryDtos);
        }

        /// <summary>
        /// Retrieves a specific active country by its ISO code.
        /// </summary>
        public async Task<ServiceResult<CountryDto>> GetCountryByIsoCodeAsync(string isoCode)
        {
            if (string.IsNullOrWhiteSpace(isoCode) || isoCode.Length != 3)
            {
                return ServiceResult<CountryDto>.Failure("Invalid ISO code provided.");
            }

            // Use the specific repository method
            var country = await _unitOfWork.Countries.GetByIsoCodeAsync(isoCode);
            if (country == null)
            {
                return ServiceResult<CountryDto>.Failure($"Country with ISO code '{isoCode}' not found or is inactive.");
            }

            var countryDto = new CountryDto
            {
                IsoCode = country.IsoCode,
                Name = country.Name,
                Continent = country.Continent
            };
            return ServiceResult<CountryDto>.Success(countryDto);
        }

        /// <summary>
        /// Retrieves a specific active country by its name (case-insensitive).
        /// </summary>
        public async Task<ServiceResult<CountryDto>> GetCountryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ServiceResult<CountryDto>.Failure("Country name cannot be empty.");
            }

            var country = await _unitOfWork.Countries.GetByNameAsync(name);
            if (country == null)
            {
                return ServiceResult<CountryDto>.Failure($"Country with name '{name}' not found or is inactive.");
            }

            var countryDto = new CountryDto
            {
                IsoCode = country.IsoCode,
                Name = country.Name,
                Continent = country.Continent
            };
            return ServiceResult<CountryDto>.Success(countryDto);
        }

        /// <summary>
        /// Retrieves all active countries belonging to a specific continent.
        /// </summary>
        public async Task<ServiceResult<IEnumerable<CountryDto>>> GetCountriesByContinentAsync(string continentName)
        {
            if (string.IsNullOrWhiteSpace(continentName))
            {
                return ServiceResult<IEnumerable<CountryDto>>.Failure("Continent name cannot be empty.");
            }

            var countries = await _unitOfWork.Countries.GetByContinentAsync(continentName);
            var countryDtos = countries.Select(c => new CountryDto
            {
                IsoCode = c.IsoCode,
                Name = c.Name,
                Continent = c.Continent
            });
            return ServiceResult<IEnumerable<CountryDto>>.Success(countryDtos);
        }

        /// <summary>
        /// Retrieves a specific active country by its ISO code, including its airports.
        /// </summary>
        public async Task<ServiceResult<CountryWithAirportsDto>> GetCountryWithAirportsByIsoCodeAsync(string isoCode)
        {
            var isoCodeUpper = isoCode.ToUpperInvariant();
             
            var country = await _unitOfWork.Countries.GetWithAirportsAsync(isoCodeUpper);

            if (country == null)
            {
                return ServiceResult<CountryWithAirportsDto>.Failure($"Country with ISO code '{isoCodeUpper}' not found.");
            }
             
            var countryDto = _mapper.Map<CountryWithAirportsDto>(country);

            return ServiceResult<CountryWithAirportsDto>.Success(countryDto);
        }

        /// <summary>
        /// Creates a new country after validation.
        /// </summary>
        public async Task<ServiceResult<CountryDto>> CreateCountryAsync(CreateCountryDto createDto)
        {
            // Normalize ISO code
            var isoCodeUpper = createDto.IsoCode.ToUpperInvariant();

            // Check for uniqueness (ISO code and Name) - checking includes deleted to prevent reuse issues
            if (await _unitOfWork.Countries.ExistsByIsoCodeAsync(isoCodeUpper))
            {
                return ServiceResult<CountryDto>.Failure($"Country with ISO code '{isoCodeUpper}' already exists.");
            }
            if (await _unitOfWork.Countries.ExistsByNameAsync(createDto.Name))
            {
                return ServiceResult<CountryDto>.Failure($"Country with name '{createDto.Name}' already exists.");
            }

            // Map DTO to entity
            var newCountry = new Country
            {
                IsoCode = isoCodeUpper,
                Name = createDto.Name,
                Continent = createDto.Continent,
                IsDeleted = false // Ensure it's active on creation
            };

            await _unitOfWork.Countries.AddAsync(newCountry);
            await _unitOfWork.SaveChangesAsync();

            // Map back to DTO for the response
            var countryDto = new CountryDto
            {
                IsoCode = newCountry.IsoCode,
                Name = newCountry.Name,
                Continent = newCountry.Continent
            };
            return ServiceResult<CountryDto>.Success(countryDto);
        }

        /// <summary>
        /// Updates an existing country's details.
        /// </summary>
        public async Task<ServiceResult<CountryDto>> UpdateCountryAsync(string isoCode, UpdateCountryDto updateDto)
        {
            var isoCodeUpper = isoCode.ToUpperInvariant();
            var country = await _unitOfWork.Countries.GetByIsoCodeAsync(isoCodeUpper); // Get active country by PK

            if (country == null)
            {
                return ServiceResult<CountryDto>.Failure($"Active country with ISO code '{isoCodeUpper}' not found.");
            }

            // Check if the new name conflicts with another existing country (excluding itself)
            var existingByName = await _unitOfWork.Countries.GetByNameAsync(updateDto.Name);
            if (existingByName != null && existingByName.IsoCode != isoCodeUpper)
            {
                return ServiceResult<CountryDto>.Failure($"Another country with the name '{updateDto.Name}' already exists.");
            }

            // Apply updates
            bool changed = false;
            if (country.Name != updateDto.Name)
            {
                country.Name = updateDto.Name;
                changed = true;
            }
            if (country.Continent != updateDto.Continent)
            {
                country.Continent = updateDto.Continent;
                changed = true;
            }
            _mapper.Map(updateDto, country);
            var countryDto = _mapper.Map<CountryDto>(country);

            if (!changed)
            {
                return ServiceResult<CountryDto>.Success(countryDto); // No changes needed
            }

            _mapper.Map(updateDto, country);

            _unitOfWork.Countries.Update(country);
            await _unitOfWork.SaveChangesAsync();


            return ServiceResult<CountryDto>.Success(countryDto);
        }

        /// <summary>
        /// Soft deletes a country. Checks for dependent airports.
        /// </summary>
        public async Task<ServiceResult> DeleteCountryAsync(string isoCode)
        {
            var isoCodeUpper = isoCode.ToUpperInvariant();
            var country = await _unitOfWork.Countries.GetByIsoCodeAsync(isoCodeUpper); // Find active country

            if (country == null)
            {
                return ServiceResult.Failure($"Active country with ISO code '{isoCodeUpper}' not found.");
            }

            // Check for dependencies (e.g., active airports in this country)
            var hasActiveAirports = await _unitOfWork.Airports.AnyAsync(a => a.CountryId == isoCodeUpper && !a.IsDeleted);
            if (hasActiveAirports)
            {
                return ServiceResult.Failure($"Cannot delete country '{isoCodeUpper}' as it has associated active airports. Please delete or reassign airports first.");
            }

            _unitOfWork.Countries.SoftDelete(country);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success();
        }

        /// <summary>
        /// Reactivates a soft-deleted country.
        /// </summary>
        public async Task<ServiceResult> ReactivateCountryAsync(string isoCode)
        {
            var isoCodeUpper = isoCode.ToUpperInvariant();
            // Need to fetch including deleted
            var country = await _unitOfWork.Countries.GetByIdAsync(isoCodeUpper); // Using GetByIdAsync which might fetch deleted

            if (country == null)
            {
                return ServiceResult.Failure($"Country with ISO code '{isoCodeUpper}' not found.");
            }

            if (!country.IsDeleted)
            {
                return ServiceResult.Failure($"Country '{isoCodeUpper}' is already active.");
            }

            country.IsDeleted = false; // Reactivate
            _unitOfWork.Countries.Update(country);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success();
        }
 
    }
}