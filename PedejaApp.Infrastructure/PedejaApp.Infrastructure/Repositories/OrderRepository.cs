using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém pedidos ativos que contêm um produto específico
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <returns>Coleção de pedidos ativos com o produto</returns>
    public async Task<IEnumerable<Order>> GetActiveOrdersWithProductAsync(int productId)
    {
        return await _dbSet
            .Where(o =>o.Status != OrderStatus.Cancelled && 
                       o.Status != OrderStatus.Delivered &&
                       o.Items.Any(i => i.ProductId == productId))
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.StatusHistory)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtém pedidos por status
    /// </summary>
    /// <param name="status">Status do pedido</param>
    /// <returns>Coleção de pedidos com o status especificado</returns>
    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.StatusHistory)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
    
    /// <summary>
    /// Obtém pedidos de um período específico
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <returns>Coleção de pedidos do período</returns>
    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(o => o.CreatedAt >= startDate && 
                       o.CreatedAt <= endDate)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.StatusHistory)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}