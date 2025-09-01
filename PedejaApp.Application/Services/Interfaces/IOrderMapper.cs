using PedejaApp.Domain.Entities;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

/// <summary>
/// Interface para mapeamento entre entidades Order e DTOs
/// </summary>
public interface IOrderMapper
{
    /// <summary>
    /// Mapeia uma entidade Order para OrderResponseDto
    /// </summary>
    /// <param name="order">Entidade Order</param>
    /// <returns>OrderResponseDto mapeado</returns>
    OrderResponseDto MapToDto(Order order);
    
    /// <summary>
    /// Mapeia uma coleção de entidades Order para OrderResponseDto
    /// </summary>
    /// <param name="orders">Coleção de entidades Order</param>
    /// <returns>Coleção de OrderResponseDto mapeados</returns>
    IEnumerable<OrderResponseDto> MapToDtos(IEnumerable<Order> orders);
    
    /// <summary>
    /// Mapeia um CreateOrderRequestDto para entidade Order
    /// </summary>
    /// <param name="request">Request de criação</param>
    /// <returns>Entidade Order</returns>
    Order MapToEntity(CreateOrderRequestDto request);
    
    /// <summary>
    /// Atualiza uma entidade Order com dados do UpdateOrderRequestDto
    /// </summary>
    /// <param name="request">Request de atualização</param>
    /// <param name="order">Entidade Order a ser atualizada</param>
    void MapToEntity(UpdateOrderRequestDto request, Order order);
    
    /// <summary>
    /// Mapeia uma entidade OrderItem para OrderItemResponseDto
    /// </summary>
    /// <param name="orderItem">Entidade OrderItem</param>
    /// <returns>OrderItemResponseDto mapeado</returns>
    OrderItemResponseDto MapToOrderItemDto(OrderItem orderItem);
    
    /// <summary>
    /// Mapeia uma coleção de entidades OrderItem para OrderItemResponseDto
    /// </summary>
    /// <param name="orderItems">Coleção de entidades OrderItem</param>
    /// <returns>Coleção de OrderItemResponseDto mapeados</returns>
    IEnumerable<OrderItemResponseDto> MapToOrderItemDtos(IEnumerable<OrderItem> orderItems);
    
    /// <summary>
    /// Mapeia uma entidade OrderItemAddon para OrderItemAddonResponseDto
    /// </summary>
    /// <param name="orderItemAddon">Entidade OrderItemAddon</param>
    /// <returns>OrderItemAddonResponseDto mapeado</returns>
    OrderItemAddonResponseDto MapToOrderItemAddonDto(OrderItemAddon orderItemAddon);
    
    /// <summary>
    /// Mapeia uma coleção de entidades OrderItemAddon para OrderItemAddonResponseDto
    /// </summary>
    /// <param name="orderItemAddons">Coleção de entidades OrderItemAddon</param>
    /// <returns>Coleção de OrderItemAddonResponseDto mapeados</returns>
    IEnumerable<OrderItemAddonResponseDto> MapToOrderItemAddonDtos(IEnumerable<OrderItemAddon> orderItemAddons);
    
    /// <summary>
    /// Mapeia um CreateOrderItemRequestDto para entidade OrderItem
    /// </summary>
    /// <param name="request">Request de criação do item</param>
    /// <param name="orderId">ID do pedido</param>
    /// <returns>Entidade OrderItem</returns>
    OrderItem MapToOrderItemEntity(CreateOrderItemRequestDto request, int orderId);
    
    /// <summary>
    /// Mapeia uma coleção de CreateOrderItemRequestDto para entidades OrderItem
    /// </summary>
    /// <param name="requests">Coleção de requests de criação</param>
    /// <param name="orderId">ID do pedido</param>
    /// <returns>Coleção de entidades OrderItem</returns>
    IEnumerable<OrderItem> MapToOrderItemEntities(IEnumerable<CreateOrderItemRequestDto> requests, int orderId);
}