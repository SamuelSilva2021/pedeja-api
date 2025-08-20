using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Infrastructure.Data;
using PedejaApp.Application.Models;
using System.ComponentModel.DataAnnotations;
using PedejaApp.Web.Models.DTOs;

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
    public async Task<ActionResult<object>> CreateOrder([FromBody] CreateOrderRequestDto request)
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
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequestDto request)
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
    public async Task<ActionResult<object>> AcceptOrder(int id, AcceptOrderRequestDto request)
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
    public async Task<ActionResult<object>> RejectOrder(int id, RejectOrderRequestDto request)
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