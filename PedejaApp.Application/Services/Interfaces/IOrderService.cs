using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services.Interfaces
{
    /// <summary>
    /// Interface para serviços de pedidos seguindo princípios SOLID
    /// </summary>
    public interface IOrderService
    {
        Task<ResultDto<IEnumerable<OrderResponseDto>>> GetOrdersAsync();
        Task<ResultDto<OrderResponseDto>> GetOrderByIdAsync(int id);
        Task<ResultDto<OrderResponseDto>> CreateOrderAsync(CreateOrderRequestDto requestDto);
        Task<ResultDto<OrderResponseDto>> UpdateOrderAsync(int id, UpdateOrderRequestDto requestDto);
        Task<ResultDto<bool>> DeleteOrderAsync(int id);
        Task<ResultDto<OrderResponseDto>> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequestDto requestDto);
        Task<ResultDto<IEnumerable<OrderResponseDto>>> GetOrdersByStatusAsync(OrderStatus status);
        Task<ResultDto<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerAsync(string customerPhone);
        Task<ResultDto<OrderResponseDto>> AcceptOrderAsync(int id, int estimatedPreparationMinutes, string? notes = null, string? userId = null);
        Task<ResultDto<OrderResponseDto>> RejectOrderAsync(int id, string reason, string? notes = null, string? rejectedBy = null);
    }
}
