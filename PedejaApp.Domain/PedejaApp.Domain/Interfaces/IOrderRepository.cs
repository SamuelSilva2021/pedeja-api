using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

/// <summary>
/// Interface do repositório de pedidos
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Obtém pedidos ativos que contêm um produto específico
    /// </summary>
    /// <param name="productId">ID do produto</param>
    /// <returns>Coleção de pedidos ativos com o produto</returns>
    Task<IEnumerable<Order>> GetActiveOrdersWithProductAsync(int productId);
    
    /// <summary>
    /// Obtém pedidos por status
    /// </summary>
    /// <param name="status">Status do pedido</param>
    /// <returns>Coleção de pedidos com o status especificado</returns>
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    
    /// <summary>
    /// Obtém pedidos de um período específico
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <returns>Coleção de pedidos do período</returns>
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
}
