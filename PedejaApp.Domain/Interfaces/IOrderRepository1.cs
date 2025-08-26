using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces
{
    public interface IOrderRepository1
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(string customerId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
    }
}