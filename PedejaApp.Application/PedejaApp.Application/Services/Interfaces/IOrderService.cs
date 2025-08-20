using PedejaApp.Web.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<object>> GetOrdersAsync();
        Task<object> GetOrderByIdAsync(int id);
        Task<ResultDto<OrderResponseDto>> CreateOrderAsync(CreateOrderRequestDto requestDto);
        Task<object> UpdateOrderAsync(int id, object orderDto);
        Task<bool> DeleteOrderAsync(int id);
        Task<object> ChangeOrderStatusAsync(int id, string status);
        Task<object> GetOrdersByStatusAsync(string status);
        Task<object> GetOrdersByCustomerAsync(string customerId);
    }
}
