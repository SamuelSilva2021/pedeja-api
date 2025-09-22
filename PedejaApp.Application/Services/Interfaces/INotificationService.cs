using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;

namespace PedejaApp.Application.Services.Interfaces;

/// <summary>
/// Interface para serviço de notificações em tempo real via SignalR
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notifica administradores sobre novo pedido criado
    /// </summary>
    Task NotifyNewOrderAsync(OrderResponseDto order);

    /// <summary>
    /// Notifica cliente sobre mudança de status do pedido
    /// </summary>
    Task NotifyOrderStatusChangedAsync(int orderId, OrderStatus oldStatus, OrderStatus newStatus, string? notes = null);

    /// <summary>
    /// Notifica administradores sobre pedido aceito
    /// </summary>
    Task NotifyOrderAcceptedAsync(OrderResponseDto order);

    /// <summary>
    /// Notifica administradores sobre pedido rejeitado
    /// </summary>
    Task NotifyOrderRejectedAsync(OrderResponseDto order, string reason);

    /// <summary>
    /// Notifica cliente sobre pedido pronto para retirada/entrega
    /// </summary>
    Task NotifyOrderReadyAsync(int orderId);

    /// <summary>
    /// Notifica cliente sobre pedido entregue/concluído
    /// </summary>
    Task NotifyOrderCompletedAsync(int orderId);

    /// <summary>
    /// Notifica todos os clientes sobre atualização do cardápio
    /// </summary>
    Task NotifyMenuUpdatedAsync(string changeType, object changedItem);

    /// <summary>
    /// Notifica sobre novo produto adicionado ao cardápio
    /// </summary>
    Task NotifyProductAddedAsync(ProductDto product);

    /// <summary>
    /// Notifica sobre produto removido do cardápio
    /// </summary>
    Task NotifyProductRemovedAsync(int productId, string productName);

    /// <summary>
    /// Notifica sobre mudança de preço de produto
    /// </summary>
    Task NotifyProductPriceChangedAsync(int productId, string productName, decimal oldPrice, decimal newPrice);

    /// <summary>
    /// Notifica sobre produto indisponível
    /// </summary>
    Task NotifyProductUnavailableAsync(int productId, string productName);

    /// <summary>
    /// Envia notificação personalizada para um grupo específico
    /// </summary>
    Task SendToGroupAsync(string groupName, string method, object data);

    /// <summary>
    /// Envia notificação personalizada para uma conexão específica
    /// </summary>
    Task SendToConnectionAsync(string connectionId, string method, object data);

    /// <summary>
    /// Envia notificação para todos os clientes conectados
    /// </summary>
    Task SendToAllAsync(string method, object data);

    /// <summary>
    /// Verifica quantos clientes estão conectados
    /// </summary>
    Task<int> GetConnectedClientsCountAsync();
}