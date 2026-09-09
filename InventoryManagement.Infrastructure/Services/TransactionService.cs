using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for transaction management.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionService> _logger;

        /// <summary>
        /// Initializes a new instance of the TransactionService class.
        /// </summary>
        /// <param name="unitOfWork">Unit of work instance.</param>
        /// <param name="logger">Logger instance.</param>
        public TransactionService(IUnitOfWork unitOfWork, ILogger<TransactionService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<TransactionListResponseDto> GetTransactionsAsync(TransactionFilterDto filter, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                Expression<Func<InventoryTransaction, bool>>? predicate = null;

                if (filter.ProductId.HasValue)
                {
                    predicate = t => t.ProductId == filter.ProductId.Value;
                }

                if (filter.WarehouseId.HasValue)
                {
                    var warehouseId = filter.WarehouseId.Value;
                    predicate = predicate == null
                        ? t => t.WarehouseId == warehouseId
                        : CombinePredicates(predicate, t => t.WarehouseId == warehouseId);
                }

                if (filter.Type.HasValue)
                {
                    var type = filter.Type.Value;
                    predicate = predicate == null
                        ? t => t.Type == type
                        : CombinePredicates(predicate, t => t.Type == type);
                }

                if (filter.StartDate.HasValue)
                {
                    var startDate = filter.StartDate.Value.Date;
                    predicate = predicate == null
                        ? t => t.TransactionDate >= startDate
                        : CombinePredicates(predicate, t => t.TransactionDate >= startDate);
                }

                if (filter.EndDate.HasValue)
                {
                    var endDate = filter.EndDate.Value.Date.AddDays(1);
                    predicate = predicate == null
                        ? t => t.TransactionDate < endDate
                        : CombinePredicates(predicate, t => t.TransactionDate < endDate);
                }

                var (transactions, totalCount) = await _unitOfWork.Transactions.GetPagedAsync(
                    filter.PageNumber,
                    filter.PageSize,
                    predicate,
                    q => q.OrderByDescending(t => t.TransactionDate),
                    cancellationToken);

                var transactionDtos = transactions.Select(MapToResponseDto);

                return new TransactionListResponseDto
                {
                    Transactions = transactionDtos,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions with filter: {@Filter}", filter);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TransactionResponseDto?> GetTransactionByIdAsync(int id, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _unitOfWork.Transactions.GetByIdAsync(id, cancellationToken);
                return transaction != null ? MapToResponseDto(transaction) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction by ID: {TransactionId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TransactionResponseDto> CreateTransactionAsync(TransactionCreateDto createDto, 
            int userId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate product exists
                var product = await _unitOfWork.Products.GetByIdAsync(createDto.ProductId, cancellationToken);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {createDto.ProductId} does not exist.");
                }

                // Validate warehouse exists
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(createDto.WarehouseId, cancellationToken);
                if (warehouse == null)
                {
                    throw new InvalidOperationException($"Warehouse with ID {createDto.WarehouseId} does not exist.");
                }

                // Validate stock availability for outbound transactions
                if (createDto.Type == TransactionType.Outbound && product.StockQuantity < createDto.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}, Requested: {createDto.Quantity}");
                }

                // Check warehouse capacity for inbound transactions
                if (createDto.Type == TransactionType.Inbound)
                {
                    var newOccupancy = warehouse.CurrentOccupancy + createDto.Quantity;
                    if (newOccupancy > warehouse.Capacity)
                    {
                        throw new InvalidOperationException($"Warehouse capacity exceeded. Capacity: {warehouse.Capacity}, Current: {warehouse.CurrentOccupancy}, Requested: {createDto.Quantity}");
                    }
                }

                using (var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        // Create transaction record
                        var inventoryTransaction = new InventoryTransaction
                        {
                            ProductId = createDto.ProductId,
                            WarehouseId = createDto.WarehouseId,
                            Type = createDto.Type,
                            Quantity = createDto.Quantity,
                            UnitPrice = createDto.UnitPrice,
                            TotalAmount = createDto.Quantity * createDto.UnitPrice,
                            TransactionDate = DateTime.UtcNow,
                            Notes = createDto.Notes,
                            CreatedByUserId = userId,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.Transactions.AddAsync(inventoryTransaction, cancellationToken);

                        // Update stock
                        var stockChange = createDto.Type == TransactionType.Inbound ? createDto.Quantity : -createDto.Quantity;
                        var newStock = product.StockQuantity + stockChange;
                        await _unitOfWork.Products.UpdateStockAsync(createDto.ProductId, newStock, cancellationToken);

                        // Update warehouse occupancy
                        var occupancyChange = createDto.Type == TransactionType.Inbound ? createDto.Quantity : -createDto.Quantity;
                        await _unitOfWork.Warehouses.UpdateOccupancyAsync(createDto.WarehouseId, occupancyChange, cancellationToken);

                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);

                        _logger.LogInformation("Transaction created successfully: {TransactionType} {Quantity}x {ProductName} by User {UserId}",
                            createDto.Type, createDto.Quantity, product.Name, userId);

                        return MapToResponseDto(inventoryTransaction);
                    }
                    catch
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction: {@CreateDto}", createDto);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteTransactionAsync(int id, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _unitOfWork.Transactions.GetByIdAsync(id, cancellationToken);
                if (transaction == null)
                {
                    _logger.LogWarning("Transaction not found for deletion: {TransactionId}", id);
                    return false;
                }

                using (var dbTransaction = await _unitOfWork.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        // Reverse the transaction effects
                        var product = await _unitOfWork.Products.GetByIdAsync(transaction.ProductId, cancellationToken);
                        if (product != null)
                        {
                            var stockChange = transaction.Type == TransactionType.Inbound ? -transaction.Quantity : transaction.Quantity;
                            var newStock = product.StockQuantity + stockChange;
                            await _unitOfWork.Products.UpdateStockAsync(transaction.ProductId, newStock, cancellationToken);
                        }

                        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(transaction.WarehouseId, cancellationToken);
                        if (warehouse != null)
                        {
                            var occupancyChange = transaction.Type == TransactionType.Inbound ? -transaction.Quantity : transaction.Quantity;
                            await _unitOfWork.Warehouses.UpdateOccupancyAsync(transaction.WarehouseId, occupancyChange, cancellationToken);
                        }

                        await _unitOfWork.Transactions.DeleteByIdAsync(id, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        await dbTransaction.CommitAsync(cancellationToken);

                        _logger.LogInformation("Transaction deleted successfully: {TransactionId}", id);
                        return true;
                    }
                    catch
                    {
                        await dbTransaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction: {TransactionId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TransactionSummaryDto> GetTransactionSummaryAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var allTransactions = await _unitOfWork.Transactions.GetAllAsync(null, cancellationToken);

                var totalInbound = allTransactions
                    .Where(t => t.Type == TransactionType.Inbound)
                    .Sum(t => t.Quantity);

                var totalOutbound = allTransactions
                    .Where(t => t.Type == TransactionType.Outbound)
                    .Sum(t => t.Quantity);

                return new TransactionSummaryDto
                {
                    TotalTransactions = allTransactions.Count(),
                    TotalInboundQuantity = totalInbound,
                    TotalOutboundQuantity = totalOutbound,
                    TotalValue = allTransactions.Sum(t => t.TotalAmount),
                    TodayTransactions = allTransactions.Count(t => t.TransactionDate.Date == today),
                    WeekTransactions = allTransactions.Count(t => t.TransactionDate >= weekStart),
                    MonthTransactions = allTransactions.Count(t => t.TransactionDate >= monthStart)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction summary");
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

        private static TransactionResponseDto MapToResponseDto(InventoryTransaction transaction)
        {
            return new TransactionResponseDto
            {
                Id = transaction.Id,
                ProductId = transaction.ProductId,
                ProductName = transaction.Product?.Name ?? string.Empty,
                WarehouseId = transaction.WarehouseId,
                WarehouseName = transaction.Warehouse?.Name ?? string.Empty,
                Type = transaction.Type,
                TypeDisplayName = transaction.Type == TransactionType.Inbound ? "Inbound" : "Outbound",
                Quantity = transaction.Quantity,
                UnitPrice = transaction.UnitPrice,
                TotalAmount = transaction.TotalAmount,
                TransactionDate = transaction.TransactionDate,
                Notes = transaction.Notes,
                CreatedByUserName = transaction.CreatedByUser?.Username ?? string.Empty,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}
