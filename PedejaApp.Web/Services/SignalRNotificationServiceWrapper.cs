using Microsoft.AspNetCore.SignalR;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Hubs;

namespace PedejaApp.Web.Services;

/// <summary>
/// Wrapper para o NotificationService que usa o Hub específico
/// Garante que as notificações sejam enviadas através do OrderNotificationHub correto
/// </summary>
public class SignalRNotificationServiceWrapper : INotificationService
{
    private readonly IHubContext<OrderNotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationServiceWrapper> _logger;

    public SignalRNotificationServiceWrapper(
        IHubContext<OrderNotificationHub> hubContext,
        ILogger<SignalRNotificationServiceWrapper> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNewOrderAsync(OrderResponseDto order)
    {
        try
        {
            _logger.LogInformation("🔄 [WRAPPER] Iniciando envio de notificação para novo pedido #{OrderId}", order.Id);
            
            var notification = new
            {
                Type = "NewOrder",
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                TotalAmount = order.Total,
                ItemsCount = order.Items?.Count() ?? 0,
                CreatedAt = order.CreatedAt,
                Message = $"Novo pedido #{order.Id} de {order.CustomerName}",
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("📤 [WRAPPER] Enviando para grupo 'Administrators': {Notification}", 
                System.Text.Json.JsonSerializer.Serialize(notification));

            await _hubContext.Clients.Group("Administrators")
                .SendAsync("NewOrderReceived", notification);

            _logger.LogInformation("✅ [WRAPPER] Notificação enviada com sucesso: Pedido #{OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [WRAPPER] Erro ao enviar notificação de novo pedido {OrderId}", order.Id);
        }
    }

    public async Task NotifyOrderStatusChangedAsync(int orderId, OrderStatus oldStatus, OrderStatus newStatus, string? notes = null)
    {
        try
        {
            var notification = new
            {
                Type = "OrderStatusChanged",
                OrderId = orderId,
                OldStatus = oldStatus.ToString(),
                NewStatus = newStatus.ToString(),
                StatusMessage = GetStatusMessage(newStatus),
                Notes = notes,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"Order_{orderId}")
                .SendAsync("OrderStatusUpdated", notification);

            await _hubContext.Clients.Group("Administrators")
                .SendAsync("OrderStatusChanged", notification);

            _logger.LogInformation("Notificação de mudança de status enviada: Pedido #{OrderId} - {OldStatus} → {NewStatus}", 
                orderId, oldStatus, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de mudança de status do pedido {OrderId}", orderId);
        }
    }

    public async Task NotifyOrderAcceptedAsync(OrderResponseDto order)
    {
        try
        {
            var notification = new
            {
                Type = "OrderAccepted",
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                EstimatedPreparationTime = order.EstimatedPreparationMinutes,
                Message = $"Pedido #{order.Id} aceito - Tempo estimado: {order.EstimatedPreparationMinutes} min",
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"Order_{order.Id}")
                .SendAsync("OrderAccepted", notification);

            _logger.LogInformation("Notificação de pedido aceito enviada: Pedido #{OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de pedido aceito {OrderId}", order.Id);
        }
    }

    public async Task NotifyOrderRejectedAsync(OrderResponseDto order, string reason)
    {
        try
        {
            var notification = new
            {
                Type = "OrderRejected",
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                Reason = reason,
                Message = $"Pedido #{order.Id} foi rejeitado",
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"Order_{order.Id}")
                .SendAsync("OrderRejected", notification);

            _logger.LogInformation("Notificação de pedido rejeitado enviada: Pedido #{OrderId} - Motivo: {Reason}", 
                order.Id, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de pedido rejeitado {OrderId}", order.Id);
        }
    }

    public async Task NotifyOrderReadyAsync(int orderId)
    {
        try
        {
            var notification = new
            {
                Type = "OrderReady",
                OrderId = orderId,
                Message = $"Seu pedido #{orderId} está pronto!",
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"Order_{orderId}")
                .SendAsync("OrderReady", notification);

            _logger.LogInformation("Notificação de pedido pronto enviada: Pedido #{OrderId}", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de pedido pronto {OrderId}", orderId);
        }
    }

    public async Task NotifyOrderCompletedAsync(int orderId)
    {
        try
        {
            var notification = new
            {
                Type = "OrderCompleted",
                OrderId = orderId,
                Message = $"Pedido #{orderId} foi concluído com sucesso!",
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"Order_{orderId}")
                .SendAsync("OrderCompleted", notification);

            _logger.LogInformation("Notificação de pedido concluído enviada: Pedido #{OrderId}", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de pedido concluído {OrderId}", orderId);
        }
    }

    public async Task NotifyMenuUpdatedAsync(string changeType, object changedItem)
    {
        try
        {
            var notification = new
            {
                Type = "MenuUpdated",
                ChangeType = changeType,
                ChangedItem = changedItem,
                Message = $"Cardápio atualizado: {changeType}",
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("MenuUpdates")
                .SendAsync("MenuUpdated", notification);

            _logger.LogInformation("Notificação de atualização do cardápio enviada: {ChangeType}", changeType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de atualização do cardápio: {ChangeType}", changeType);
        }
    }

    public async Task NotifyProductAddedAsync(ProductDto product)
    {
        await NotifyMenuUpdatedAsync("ProductAdded", new { 
            ProductId = product.Id, 
            ProductName = product.Name,
            CategoryName = product.CategoryName,
            Price = product.Price
        });
    }

    public async Task NotifyProductRemovedAsync(int productId, string productName)
    {
        await NotifyMenuUpdatedAsync("ProductRemoved", new { 
            ProductId = productId, 
            ProductName = productName 
        });
    }

    public async Task NotifyProductPriceChangedAsync(int productId, string productName, decimal oldPrice, decimal newPrice)
    {
        await NotifyMenuUpdatedAsync("ProductPriceChanged", new { 
            ProductId = productId, 
            ProductName = productName,
            OldPrice = oldPrice,
            NewPrice = newPrice
        });
    }

    public async Task NotifyProductUnavailableAsync(int productId, string productName)
    {
        await NotifyMenuUpdatedAsync("ProductUnavailable", new { 
            ProductId = productId, 
            ProductName = productName 
        });
    }

    public async Task SendToGroupAsync(string groupName, string method, object data)
    {
        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync(method, data);
            _logger.LogInformation("Notificação personalizada enviada para grupo {GroupName}: {Method}", 
                groupName, method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação para grupo {GroupName}", groupName);
        }
    }

    public async Task SendToConnectionAsync(string connectionId, string method, object data)
    {
        try
        {
            await _hubContext.Clients.Client(connectionId).SendAsync(method, data);
            _logger.LogInformation("Notificação personalizada enviada para conexão {ConnectionId}: {Method}", 
                connectionId, method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação para conexão {ConnectionId}", connectionId);
        }
    }

    public async Task SendToAllAsync(string method, object data)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync(method, data);
            _logger.LogInformation("Notificação broadcast enviada: {Method}", method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação broadcast: {Method}", method);
        }
    }

    public async Task<int> GetConnectedClientsCountAsync()
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("Ping");
            return 1;
        }
        catch
        {
            return 0;
        }
    }

    private static string GetStatusMessage(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Aguardando confirmação",
        OrderStatus.Confirmed => "Pedido confirmado",
        OrderStatus.Preparing => "Em preparação",
        OrderStatus.Ready => "Pronto para retirada",
        OrderStatus.OutForDelivery => "Saiu para entrega",
        OrderStatus.Delivered => "Entregue",
        OrderStatus.Cancelled => "Cancelado",
        OrderStatus.Rejected => "Rejeitado",
        _ => "Status desconhecido"
    };
}