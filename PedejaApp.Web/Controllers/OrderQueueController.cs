using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderQueueController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderQueueController> _logger;

    public OrderQueueController(ApplicationDbContext context, ILogger<OrderQueueController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get orders in queue with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetOrderQueue(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .Include(o => o.Rejection)
                .AsQueryable();

            // Apply status filter
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }
            else
            {
                // Default: show active orders (not delivered/cancelled/rejected)
                query = query.Where(o => o.Status != OrderStatus.Delivered 
                                      && o.Status != OrderStatus.Cancelled 
                                      && o.Status != OrderStatus.Rejected);
            }

            // Apply date filters
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= toDate.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(o => o.CustomerName.Contains(searchTerm) ||
                                      o.CustomerPhone.Contains(searchTerm) ||
                                      o.Id.ToString().Contains(searchTerm));
            }

            // Apply ordering (queue position, then creation time)
            query = query.OrderBy(o => o.QueuePosition).ThenBy(o => o.CreatedAt);

            // Apply pagination
            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                o.Id,
                o.CustomerName,
                o.CustomerPhone,
                o.CustomerEmail,
                o.DeliveryAddress,
                o.Total,
                o.Status,
                o.CreatedAt,
                o.UpdatedAt,
                o.Notes,
                o.IsDelivery,
                o.EstimatedPreparationMinutes,
                o.EstimatedDeliveryTime,
                o.QueuePosition,
                Items = o.Items.Select(i => new
                {
                    i.Id,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.Subtotal,
                    i.Notes
                }),
                StatusHistory = o.StatusHistory.OrderBy(sh => sh.Timestamp).Select(sh => new
                {
                    sh.Id,
                    sh.Status,
                    sh.Timestamp,
                    sh.Notes,
                    sh.UserId
                }),
                Rejection = o.Rejection != null ? new
                {
                    o.Rejection.Reason,
                    o.Rejection.Notes,
                    o.Rejection.RejectedAt,
                    o.Rejection.RejectedBy
                } : null,
                TimeElapsedMinutes = (DateTime.UtcNow - o.CreatedAt).TotalMinutes
            });

            return Ok(new
            {
                Orders = result,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order queue");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Accept an order and add it to the preparation queue
    /// </summary>
    [HttpPost("{id}/accept")]
    public async Task<ActionResult> AcceptOrder(int id, [FromBody] QueueAcceptOrderRequest request)
    {
        try
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound($"Order {id} not found");
            }

            if (order.Status != OrderStatus.Pending)
            {
                return BadRequest($"Order {id} cannot be accepted (current status: {order.Status})");
            }

            // Update order status
            order.Status = OrderStatus.Confirmed;
            order.EstimatedPreparationMinutes = request.EstimatedPreparationMinutes;
            order.EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(request.EstimatedPreparationMinutes + (order.IsDelivery ? 20 : 0));
            order.UpdatedAt = DateTime.UtcNow;

            // Set queue position (last in queue)
            var maxPosition = await _context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Preparing)
                .MaxAsync(o => (int?)o.QueuePosition) ?? 0;
            order.QueuePosition = maxPosition + 1;

            // Add status history
            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Confirmed,
                Timestamp = DateTime.UtcNow,
                Notes = request.Notes,
                UserId = request.UserId ?? "system"
            };
            _context.OrderStatusHistory.Add(statusHistory);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} accepted by user {UserId}", id, request.UserId);
            return Ok(new { Message = "Order accepted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Reject an order with a reason
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<ActionResult> RejectOrder(int id, [FromBody] QueueRejectOrderRequest request)
    {
        try
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound($"Order {id} not found");
            }

            if (order.Status != OrderStatus.Pending)
            {
                return BadRequest($"Order {id} cannot be rejected (current status: {order.Status})");
            }

            // Update order status
            order.Status = OrderStatus.Rejected;
            order.UpdatedAt = DateTime.UtcNow;

            // Add rejection record
            var rejection = new OrderRejection
            {
                OrderId = order.Id,
                Reason = request.Reason,
                Notes = request.Notes,
                RejectedAt = DateTime.UtcNow,
                RejectedBy = request.RejectedBy ?? "system"
            };
            _context.OrderRejections.Add(rejection);

            // Add status history
            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = OrderStatus.Rejected,
                Timestamp = DateTime.UtcNow,
                Notes = $"Rejected: {request.Reason}",
                UserId = request.RejectedBy ?? "system"
            };
            _context.OrderStatusHistory.Add(statusHistory);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} rejected by user {UserId} with reason: {Reason}", 
                id, request.RejectedBy, request.Reason);
            return Ok(new { Message = "Order rejected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update order status in the preparation workflow
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] QueueUpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound($"Order {id} not found");
            }

            // Validate status transition
            if (!IsValidStatusTransition(order.Status, request.NewStatus))
            {
                return BadRequest($"Invalid status transition from {order.Status} to {request.NewStatus}");
            }

            // Update order status
            order.Status = request.NewStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // Add status history
            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = request.NewStatus,
                Timestamp = DateTime.UtcNow,
                Notes = request.Notes,
                UserId = request.UserId ?? "system"
            };
            _context.OrderStatusHistory.Add(statusHistory);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} status updated to {Status} by user {UserId}", 
                id, request.NewStatus, request.UserId);
            return Ok(new { Message = "Order status updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update estimated preparation time for an order
    /// </summary>
    [HttpPut("{id}/preparation-time")]
    public async Task<ActionResult> UpdatePreparationTime(int id, [FromBody] QueueUpdatePreparationTimeRequest request)
    {
        try
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound($"Order {id} not found");
            }

            order.EstimatedPreparationMinutes = request.EstimatedMinutes;
            order.EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(request.EstimatedMinutes + (order.IsDelivery ? 20 : 0));
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} preparation time updated to {Minutes} minutes", 
                id, request.EstimatedMinutes);
            return Ok(new { Message = "Preparation time updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preparation time for order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Reorder queue manually
    /// </summary>
    [HttpPut("reorder")]
    public async Task<ActionResult> ReorderQueue([FromBody] QueueReorderRequest request)
    {
        try
        {
            foreach (var orderPosition in request.OrderPositions)
            {
                var order = await _context.Orders.FindAsync(orderPosition.OrderId);
                if (order != null)
                {
                    order.QueuePosition = orderPosition.Position;
                    order.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Queue reordered by user {UserId}", request.UserId);
            return Ok(new { Message = "Queue reordered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering queue");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get daily metrics for the restaurant
    /// </summary>
    [HttpGet("metrics/daily")]
    public async Task<ActionResult<object>> GetDailyMetrics([FromQuery] DateTime? date = null)
    {
        try
        {
            var targetDate = date ?? DateTime.Today;
            var startOfDay = targetDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            var orders = await _context.Orders
                .Where(o => o.CreatedAt >= startOfDay && o.CreatedAt < endOfDay)
                .Include(o => o.StatusHistory)
                .ToListAsync();

            var metrics = new
            {
                Date = targetDate,
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                ConfirmedOrders = orders.Count(o => o.Status == OrderStatus.Confirmed),
                PreparingOrders = orders.Count(o => o.Status == OrderStatus.Preparing),
                ReadyOrders = orders.Count(o => o.Status == OrderStatus.Ready),
                DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                RejectedOrders = orders.Count(o => o.Status == OrderStatus.Rejected),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                TotalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
                AverageOrderValue = orders.Where(o => o.Status == OrderStatus.Delivered).Average(o => (decimal?)o.Total) ?? 0,
                AveragePreparationTime = CalculateAveragePreparationTime(orders),
                OrdersByHour = orders
                    .GroupBy(o => o.CreatedAt.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Hour)
                    .ToList()
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily metrics");
            return StatusCode(500, "Internal server error");
        }
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus == OrderStatus.Confirmed || newStatus == OrderStatus.Rejected || newStatus == OrderStatus.Cancelled,
            OrderStatus.Confirmed => newStatus == OrderStatus.Preparing || newStatus == OrderStatus.Cancelled,
            OrderStatus.Preparing => newStatus == OrderStatus.Ready || newStatus == OrderStatus.Cancelled,
            OrderStatus.Ready => newStatus == OrderStatus.OutForDelivery || newStatus == OrderStatus.Delivered,
            OrderStatus.OutForDelivery => newStatus == OrderStatus.Delivered,
            _ => false
        };
    }

    private static double CalculateAveragePreparationTime(List<Order> orders)
    {
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered && o.StatusHistory.Any()).ToList();
        if (!completedOrders.Any()) return 0;

        var preparationTimes = new List<double>();
        foreach (var order in completedOrders)
        {
            var confirmedTime = order.StatusHistory.FirstOrDefault(sh => sh.Status == OrderStatus.Confirmed)?.Timestamp;
            var readyTime = order.StatusHistory.FirstOrDefault(sh => sh.Status == OrderStatus.Ready)?.Timestamp;
            
            if (confirmedTime.HasValue && readyTime.HasValue)
            {
                preparationTimes.Add((readyTime.Value - confirmedTime.Value).TotalMinutes);
            }
        }

        return preparationTimes.Any() ? preparationTimes.Average() : 0;
    }
}

// Request DTOs for Queue Management
public class QueueAcceptOrderRequest
{
    public int EstimatedPreparationMinutes { get; set; } = 30;
    public string? Notes { get; set; }
    public string? UserId { get; set; }
}

public class QueueRejectOrderRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? RejectedBy { get; set; }
}

public class QueueUpdateOrderStatusRequest
{
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
    public string? UserId { get; set; }
}

public class QueueUpdatePreparationTimeRequest
{
    public int EstimatedMinutes { get; set; }
}

public class QueueReorderRequest
{
    public List<OrderPosition> OrderPositions { get; set; } = new();
    public string? UserId { get; set; }
}

public class OrderPosition
{
    public int OrderId { get; set; }
    public int Position { get; set; }
}
