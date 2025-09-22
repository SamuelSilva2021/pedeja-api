using Microsoft.AspNetCore.SignalR;
using PedejaApp.Application.Hubs;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Web.Hubs;

/// <summary>
/// Hub SignalR para notificações em tempo real de pedidos
/// Gerencia conexões de clientes e administradores para receber atualizações instantâneas
/// </summary>
public class OrderNotificationHub : Hub, IOrderNotificationHub
{
    private readonly ILogger<OrderNotificationHub> _logger;

    public OrderNotificationHub(ILogger<OrderNotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Cliente se conecta ao hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        
        _logger.LogInformation("Nova conexão SignalR: {ConnectionId} - UserAgent: {UserAgent}", 
            connectionId, userAgent);

        // Enviar confirmação de conexão
        await Clients.Caller.SendAsync("Connected", new
        {
            ConnectionId = connectionId,
            Message = "Conectado com sucesso ao sistema de notificações",
            Timestamp = DateTime.UtcNow
        });

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Cliente se desconecta do hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        
        if (exception != null)
        {
            _logger.LogWarning(exception, "Conexão SignalR perdida: {ConnectionId}", connectionId);
        }
        else
        {
            _logger.LogInformation("Conexão SignalR encerrada: {ConnectionId}", connectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Administrador se registra para receber notificações de pedidos
    /// </summary>
    public async Task JoinAdminGroup()
    {
        var connectionId = Context.ConnectionId;
        
        await Groups.AddToGroupAsync(connectionId, "Administrators");
        
        _logger.LogInformation("Administrador conectado: {ConnectionId}", connectionId);
        
        await Clients.Caller.SendAsync("JoinedAdminGroup", new
        {
            Message = "Conectado ao grupo de administradores",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Cliente se registra para receber atualizações de seu pedido específico
    /// </summary>
    public async Task JoinOrderGroup(int orderId)
    {
        var connectionId = Context.ConnectionId;
        var groupName = $"Order_{orderId}";
        
        await Groups.AddToGroupAsync(connectionId, groupName);
        
        _logger.LogInformation("Cliente conectado ao pedido {OrderId}: {ConnectionId}", 
            orderId, connectionId);
        
        await Clients.Caller.SendAsync("JoinedOrderGroup", new
        {
            OrderId = orderId,
            Message = $"Conectado às atualizações do pedido #{orderId}",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Cliente se registra para receber atualizações do cardápio
    /// </summary>
    public async Task JoinMenuGroup()
    {
        var connectionId = Context.ConnectionId;
        
        await Groups.AddToGroupAsync(connectionId, "MenuUpdates");
        
        _logger.LogInformation("Cliente conectado a atualizações do cardápio: {ConnectionId}", connectionId);
        
        await Clients.Caller.SendAsync("JoinedMenuGroup", new
        {
            Message = "Conectado às atualizações do cardápio",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Teste manual de notificação - para debugging
    /// </summary>
    public async Task TestNotification()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("🧪 Teste manual de notificação solicitado por: {ConnectionId}", connectionId);
        
        var testData = new
        {
            Type = "NewOrder",
            OrderId = 999,
            CustomerName = "Cliente Teste Manual",
            CustomerPhone = "(11) 99999-9999",
            TotalAmount = 50.00,
            ItemsCount = 2,
            CreatedAt = DateTime.UtcNow,
            Message = "Teste manual de notificação",
            Timestamp = DateTime.UtcNow
        };
        
        _logger.LogInformation("🧪 Enviando teste para grupo Administrators");
        await Clients.Group("Administrators").SendAsync("NewOrderReceived", testData);
        _logger.LogInformation("🧪 Teste enviado!");
    }

    /// <summary>
    /// Ping para manter a conexão ativa
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", new
        {
            Timestamp = DateTime.UtcNow,
            ConnectionId = Context.ConnectionId
        });
    }

    /// <summary>
    /// Cliente sai de um grupo específico
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        var connectionId = Context.ConnectionId;
        
        await Groups.RemoveFromGroupAsync(connectionId, groupName);
        
        _logger.LogInformation("Cliente saiu do grupo {GroupName}: {ConnectionId}", 
            groupName, connectionId);
        
        await Clients.Caller.SendAsync("LeftGroup", new
        {
            GroupName = groupName,
            Message = $"Saiu do grupo {groupName}",
            Timestamp = DateTime.UtcNow
        });
    }
}