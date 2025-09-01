using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Enuns;
using PedejaApp.Web.Models;

namespace PedejaApp.Application.Services.Interfaces;

/// <summary>
/// Interface para validações de negócio relacionadas a pedidos
/// </summary>
public interface IOrderValidationService
{
    /// <summary>
    /// Valida a criação de um novo pedido
    /// </summary>
    /// <param name="request">Request de criação do pedido</param>
    /// <returns>Resultado da validação</returns>
    Task<ApiResponse<bool>> ValidateCreateOrderAsync(CreateOrderRequestDto request);
    
    /// <summary>
    /// Valida a atualização de um pedido
    /// </summary>
    /// <param name="orderId">ID do pedido</param>
    /// <param name="request">Request de atualização</param>
    /// <returns>Resultado da validação</returns>
    Task<ApiResponse<bool>> ValidateUpdateOrderAsync(int orderId, UpdateOrderRequestDto request);
    
    /// <summary>
    /// Valida a mudança de status de um pedido
    /// </summary>
    /// <param name="orderId">ID do pedido</param>
    /// <param name="newStatus">Novo status</param>
    /// <returns>Resultado da validação</returns>
    Task<ApiResponse<bool>> ValidateStatusChangeAsync(int orderId, OrderStatus newStatus);
    
    /// <summary>
    /// Valida se um pedido pode ser aceito
    /// </summary>
    /// <param name="orderId">ID do pedido</param>
    /// <param name="request">Request de aceitação</param>
    /// <returns>Resultado da validação</returns>
    Task<ApiResponse<bool>> ValidateAcceptOrderAsync(int orderId, AcceptOrderRequestDto request);
    
    /// <summary>
    /// Valida se um pedido pode ser rejeitado
    /// </summary>
    /// <param name="orderId">ID do pedido</param>
    /// <param name="request">Request de rejeição</param>
    /// <returns>Resultado da validação</returns>
    Task<ApiResponse<bool>> ValidateRejectOrderAsync(int orderId, RejectOrderRequestDto request);
    
    /// <summary>
    /// Valida se um pedido pode ser cancelado
    /// </summary>
    /// <param name="orderId">ID do pedido</param>
    /// <returns>Resultado da validação</returns>
    Task<ApiResponse<bool>> ValidateCancelOrderAsync(int orderId);
    
    /// <summary>
    /// Valida se um pedido pode ser excluído
    /// </summary>
    /// <param name="orderId">ID do pedido</param>
    /// <returns>Resultado da validação</returns>
    Task<ApiResponse<bool>> ValidateDeleteOrderAsync(int orderId);
    
    /// <summary>
    /// Valida os itens de um pedido
    /// </summary>
    /// <param name="items">Lista de itens do pedido</param>
    /// <returns>Resultado da validação</returns>
    Task<ApiResponse<bool>> ValidateOrderItemsAsync(IEnumerable<CreateOrderItemRequestDto> items);
    
    /// <summary>
    /// Valida se uma transição de status é válida
    /// </summary>
    /// <param name="currentStatus">Status atual</param>
    /// <param name="newStatus">Novo status</param>
    /// <returns>True se a transição é válida</returns>
    bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus);
    
    /// <summary>
    /// Valida informações do cliente
    /// </summary>
    /// <param name="customerName">Nome do cliente</param>
    /// <param name="customerPhone">Telefone do cliente</param>
    /// <returns>Resultado da validação</returns>
    ApiResponse<bool> ValidateCustomerInfo(string customerName, string customerPhone);
}