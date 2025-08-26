using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Web.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Application.Services
{
    public class OrderService : IOrderService
    {
        public Task<object> ChangeOrderStatusAsync(int id, string status)
        {
            throw new NotImplementedException();
        }

        public Task<ResultDto<OrderResponseDto>> CreateOrderAsync(CreateOrderRequestDto requestDto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteOrderAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetOrderByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<object>> GetOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<object> GetOrdersByCustomerAsync(string customerId)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetOrdersByStatusAsync(string status)
        {
            throw new NotImplementedException();
        }

        public Task<object> UpdateOrderAsync(int id, object orderDto)
        {
            throw new NotImplementedException();
        }
    }
}
