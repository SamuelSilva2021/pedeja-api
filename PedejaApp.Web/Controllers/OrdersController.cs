using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Infrastructure.Data;
using PedejaApp.Application.Models;
using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var response = orders.Select(order => new
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            DeliveryAddress = order.DeliveryAddress,
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            Total = order.Total,
            Status = order.Status,
            IsDelivery = order.IsDelivery,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            QueuePosition = order.QueuePosition,
            EstimatedPreparationMinutes = order.EstimatedPreparationMinutes,
            EstimatedDeliveryTime = order.EstimatedDeliveryTime,
            Items = order.Items.Select(i => new
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.Subtotal,
                Notes = i.Notes
            }).ToList()
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        var response = new
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            DeliveryAddress = order.DeliveryAddress,
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            Total = order.Total,
            Status = order.Status,
            IsDelivery = order.IsDelivery,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            QueuePosition = order.QueuePosition,
            EstimatedPreparationMinutes = order.EstimatedPreparationMinutes,
            EstimatedDeliveryTime = order.EstimatedDeliveryTime,
            Items = order.Items.Select(i => new
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.Subtotal,
                Notes = i.Notes
            }).ToList()
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            CustomerEmail = request.CustomerEmail,
            DeliveryAddress = request.DeliveryAddress,
            IsDelivery = request.IsDelivery,
            Notes = request.Notes,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {item.ProductId} not found");
            }

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                Subtotal = product.Price * item.Quantity,
                Notes = item.Notes
            });
        }

        // Calculate totals
        order.Subtotal = order.Items.Sum(i => i.Subtotal);
        order.DeliveryFee = order.IsDelivery ? 5.00m : 0; // Fixed delivery fee for now
        order.Total = order.Subtotal + order.DeliveryFee;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Return a clean response without circular references
        var response = new
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            DeliveryAddress = order.DeliveryAddress,
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            Total = order.Total,
            Status = order.Status,
            IsDelivery = order.IsDelivery,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.Subtotal,
                Notes = i.Notes
            }).ToList()
        };

        return Ok(response);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/accept")]
    public async Task<ActionResult<object>> AcceptOrder(int id, AcceptOrderRequest request)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = OrderStatus.Confirmed;
        order.EstimatedPreparationMinutes = request.EstimatedPreparationMinutes;
        order.UpdatedAt = DateTime.UtcNow;
        
        if (request.EstimatedPreparationMinutes.HasValue)
        {
            order.EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(request.EstimatedPreparationMinutes.Value);
        }

        await _context.SaveChangesAsync();

        var response = new
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            DeliveryAddress = order.DeliveryAddress,
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            Total = order.Total,
            Status = order.Status,
            IsDelivery = order.IsDelivery,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            EstimatedPreparationMinutes = order.EstimatedPreparationMinutes,
            EstimatedDeliveryTime = order.EstimatedDeliveryTime,
            Items = order.Items.Select(i => new
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.Subtotal,
                Notes = i.Notes
            }).ToList()
        };

        return Ok(response);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<object>> RejectOrder(int id, RejectOrderRequest request)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = OrderStatus.Cancelled;
        order.Notes = string.IsNullOrEmpty(order.Notes) 
            ? $"Rejeitado: {request.Reason}" 
            : $"{order.Notes} | Rejeitado: {request.Reason}";
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = new
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            Status = order.Status,
            Notes = order.Notes,
            UpdatedAt = order.UpdatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateOrderRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20, MinimumLength = 10)]
    public string CustomerPhone { get; set; } = string.Empty;
    
    [EmailAddress]
    public string? CustomerEmail { get; set; }
    
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string DeliveryAddress { get; set; } = string.Empty;
    
    public bool IsDelivery { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0")]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99")]
    public int Quantity { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class AcceptOrderRequest
{
    public int? EstimatedPreparationMinutes { get; set; }
    public string? Notes { get; set; }
}

public class RejectOrderRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Notes { get; set; }
}
