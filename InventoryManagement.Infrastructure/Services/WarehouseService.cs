using InventoryManagement.Application.DTOs.Warehouse;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for warehouse management.
    /// </summary>
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WarehouseService> _logger;

        /// <summary>
        /// Initializes a new instance of the WarehouseService class.
        /// </summary>
        /// <param name="unitOfWork">Unit of work instance.</param>
        /// <param name="logger">Logger instance.</param>
        public WarehouseService(IUnitOfWork unitOfWork, ILogger<WarehouseService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WarehouseResponseDto>> GetWarehousesAsync(WarehouseFilterDto filter, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                Expression<Func<Warehouse, bool>>? predicate = null;

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    predicate = w => w.Name.ToLower().Contains(searchTerm) ||
                                     w.Location.ToLower().Contains(searchTerm);
                }

                if (filter.AvailableOnly == true)
                {
                    predicate = predicate == null
                        ? w => w.CurrentOccupancy < w.Capacity
                        : CombinePredicates(predicate, w => w.CurrentOccupancy < w.Capacity);
                }

                var warehouses = await _unitOfWork.Warehouses.GetAllAsync(predicate, cancellationToken);

                return warehouses
                    .OrderBy(w => w.Name)
                    .Select(MapToResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses with filter: {@Filter}", filter);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<WarehouseResponseDto?> GetWarehouseByIdAsync(int id, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
                return warehouse != null ? MapToResponseDto(warehouse) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse by ID: {WarehouseId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<WarehouseResponseDto> CreateWarehouseAsync(WarehouseCreateDto createDto, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var warehouse = new Warehouse
                {
                    Name = createDto.Name,
                    Location = createDto.Location,
                    Capacity = createDto.Capacity,
                    CurrentOccupancy = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Warehouses.AddAsync(warehouse, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Warehouse created successfully: {WarehouseName}", warehouse.Name);
                return MapToResponseDto(warehouse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse: {@CreateDto}", createDto);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<WarehouseResponseDto> UpdateWarehouseAsync(int id, WarehouseCreateDto updateDto, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
                if (warehouse == null)
                {
                    throw new InvalidOperationException($"Warehouse with ID {id} does not exist.");
                }

                warehouse.Name = updateDto.Name;
                warehouse.Location = updateDto.Location;
                warehouse.Capacity = updateDto.Capacity;
                warehouse.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Warehouses.UpdateAsync(warehouse, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Warehouse updated successfully: {WarehouseName} (ID: {WarehouseId})", warehouse.Name, warehouse.Id);
                return MapToResponseDto(warehouse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse: {WarehouseId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteWarehouseAsync(int id, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
                if (warehouse == null)
                {
                    _logger.LogWarning("Warehouse not found for deletion: {WarehouseId}", id);
                    return false;
                }

                // Check if warehouse has transactions
                var hasTransactions = await _unitOfWork.Transactions.AnyAsync(t => t.WarehouseId == id, cancellationToken);
                if (hasTransactions)
                {
                    throw new InvalidOperationException("Cannot delete warehouse with existing transactions.");
                }

                await _unitOfWork.Warehouses.DeleteByIdAsync(id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Warehouse deleted successfully: {WarehouseId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse: {WarehouseId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WarehouseResponseDto>> GetAllWarehousesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var warehouses = await _unitOfWork.Warehouses.GetAllAsync(null, cancellationToken);
                return warehouses.OrderBy(w => w.Name).Select(MapToResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all warehouses");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WarehouseResponseDto>> GetAvailableWarehousesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var warehouses = await _unitOfWork.Warehouses.GetAvailableWarehousesAsync(cancellationToken);
                return warehouses.Select(MapToResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available warehouses");
                throw;
            }
        }

        private static Expression<Func<T, bool>> CombinePredicates<T>(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T));
            var combined = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter));
            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }

        private static WarehouseResponseDto MapToResponseDto(Warehouse warehouse)
        {
            return new WarehouseResponseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Location = warehouse.Location,
                Capacity = warehouse.Capacity,
                CurrentOccupancy = warehouse.CurrentOccupancy,
                OccupancyPercentage = warehouse.Capacity > 0
                    ? Math.Round((double)warehouse.CurrentOccupancy / warehouse.Capacity * 100, 2)
                    : 0,
                CreatedAt = warehouse.CreatedAt
            };
        }
    }
}
