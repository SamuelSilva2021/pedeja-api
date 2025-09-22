using Microsoft.Extensions.Logging;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services;

/// <summary>
/// Implementação do serviço de pedidos seguindo princípios SOLID e Clean Architecture
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAddonRepository _addonRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository, 
        IProductRepository productRepository, 
        IAddonRepository addonRepository,
        INotificationService notificationService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _addonRepository = addonRepository;
        _notificationService = notificationService;
        _logger = logger;
    }
    /// <summary>
    /// Obtém todos os pedidos
    /// </summary>
    public async Task<ResultDto<IEnumerable<OrderResponseDto>>> GetOrdersAsync()
    {
        try
        {
            var orders = await _orderRepository.GetAllWithIncludesAsync(
                o => o.Items,
                o => o.StatusHistory,
                o => o.Rejection);

            var orderDtos = orders.Select(MapToOrderResponseDto);
            return ResultDto<IEnumerable<OrderResponseDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter pedidos");
            return ResultDto<IEnumerable<OrderResponseDto>>.Fail("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém um pedido por ID
    /// </summary>
    public async Task<ResultDto<OrderResponseDto>> GetOrderByIdAsync(int id)
    {
        try
        {
            var order = await _orderRepository.GetByIdWithIncludesAsync(id,
                o => o.Items,
                o => o.StatusHistory,
                o => o.Rejection);

            if (order == null)
                return ResultDto<OrderResponseDto>.Fail("Pedido não encontrado");

            var orderDto = MapToOrderResponseDto(order);
            return ResultDto<OrderResponseDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter pedido {OrderId}", id);
            return ResultDto<OrderResponseDto>.Fail("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Cria um novo pedido
    /// </summary>
    public async Task<ResultDto<OrderResponseDto>> CreateOrderAsync(CreateOrderRequestDto requestDto)
    {
        try
        {
            // Validar produtos e calcular valores
            var validationResult = await ValidateOrderItemsAsync(requestDto.Items);
            if (!validationResult.IsValid)
                return ResultDto<OrderResponseDto>.Fail(validationResult.ErrorMessage);

            var order = new Order
            {
                CustomerName = requestDto.CustomerName,
                CustomerPhone = requestDto.CustomerPhone,
                CustomerEmail = requestDto.CustomerEmail,
                DeliveryAddress = requestDto.DeliveryAddress,
                IsDelivery = requestDto.IsDelivery,
                Notes = requestDto.Notes,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Criar itens do pedido
            foreach (var itemDto in requestDto.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null) continue;

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = itemDto.Quantity,
                    Notes = itemDto.Notes,
                    Subtotal = product.Price * itemDto.Quantity
                };

                // Adicionar addons
                foreach (var addonDto in itemDto.Addons)
                {
                    var addon = await _addonRepository.GetByIdAsync(addonDto.AddonId);
                    if (addon == null) continue;

                    var orderItemAddon = new OrderItemAddon
                    {
                        AddonId = addonDto.AddonId,
                        AddonName = addon.Name,
                        UnitPrice = addon.Price,
                        Quantity = addonDto.Quantity,
                        Subtotal = addon.Price * addonDto.Quantity
                    };

                    orderItem.Addons.Add(orderItemAddon);
                    orderItem.Subtotal += orderItemAddon.Subtotal;
                }

                order.Items.Add(orderItem);
            }

            // Calcular totais
            order.Subtotal = order.Items.Sum(i => i.Subtotal);
            order.DeliveryFee = order.IsDelivery ? 5.00m : 0; // Taxa fixa de entrega
            order.Total = order.Subtotal + order.DeliveryFee;

            var createdOrder = await _orderRepository.AddAsync(order);
            var orderDto = MapToOrderResponseDto(createdOrder);

            _logger.LogInformation("Pedido {OrderId} criado com sucesso", createdOrder.Id);
            
            // Enviar notificação de novo pedido para administradores
            try
            {
                await _notificationService.NotifyNewOrderAsync(orderDto);
            }
            catch (Exception notificationEx)
            {
                _logger.LogWarning(notificationEx, "Erro ao enviar notificação de novo pedido {OrderId}", createdOrder.Id);
                // Não falhar a criação do pedido por erro de notificação
            }
            
            return ResultDto<OrderResponseDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido");
            return ResultDto<OrderResponseDto>.Fail("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Atualiza um pedido existente
    /// </summary>
    public async Task<ResultDto<OrderResponseDto>> UpdateOrderAsync(int id, UpdateOrderRequestDto requestDto)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return ResultDto<OrderResponseDto>.Fail("Pedido não encontrado");

            if (order.Status != OrderStatus.Pending)
                return ResultDto<OrderResponseDto>.Fail("Apenas pedidos pendentes podem ser atualizados");

            // Atualizar propriedades
            order.CustomerName = requestDto.CustomerName;
            order.CustomerPhone = requestDto.CustomerPhone;
            order.CustomerEmail = requestDto.CustomerEmail;
            order.DeliveryAddress = requestDto.DeliveryAddress;
            if (requestDto.IsDelivery.HasValue)
                order.IsDelivery = requestDto.IsDelivery.Value;
            order.Notes = requestDto.Notes;
            order.EstimatedPreparationMinutes = requestDto.EstimatedPreparationMinutes;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            var orderDto = MapToOrderResponseDto(order);
            _logger.LogInformation("Pedido {OrderId} atualizado com sucesso", id);
            return ResultDto<OrderResponseDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar pedido {OrderId}", id);
            return ResultDto<OrderResponseDto>.Fail("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Exclui um pedido (soft delete)
    /// </summary>
    public async Task<ResultDto<bool>> DeleteOrderAsync(int id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return ResultDto<bool>.Fail("Pedido não encontrado");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Cancelled)
                return ResultDto<bool>.Fail("Apenas pedidos pendentes ou cancelados podem ser excluídos");

            await _orderRepository.DeleteVirtualAsync(id);

            _logger.LogInformation("Pedido {OrderId} excluído com sucesso", id);
            return ResultDto<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir pedido {OrderId}", id);
            return ResultDto<bool>.Fail("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Atualiza o status de um pedido
    /// </summary>
    public async Task<ResultDto<OrderResponseDto>> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequestDto requestDto)
    {
        try
        {
            var order = await _orderRepository.GetByIdWithIncludesAsync(id, o => o.StatusHistory);
            if (order == null)
                return ResultDto<OrderResponseDto>.Fail("Pedido não encontrado");

            // Validar transição de status
            if (!IsValidStatusTransition(order.Status, requestDto.Status))
                return ResultDto<OrderResponseDto>.Fail($"Transição de status inválida: {order.Status} -> {requestDto.Status}");

            var previousStatus = order.Status;
            order.Status = requestDto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            // Adicionar histórico de status
            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = requestDto.Status,
                Timestamp = DateTime.UtcNow,
                Notes = requestDto.Notes,
                UserId = requestDto.UserId ?? "system"
            };

            order.StatusHistory.Add(statusHistory);
            await _orderRepository.UpdateAsync(order);

            var orderDto = MapToOrderResponseDto(order);
            _logger.LogInformation("Status do pedido {OrderId} alterado de {PreviousStatus} para {NewStatus}", 
                id, previousStatus, requestDto.Status);
            
            // Enviar notificação de mudança de status
            try
            {
                await _notificationService.NotifyOrderStatusChangedAsync(id, previousStatus, requestDto.Status, requestDto.Notes);
                
                // Notificações específicas por status
                switch (requestDto.Status)
                {
                    case OrderStatus.Ready:
                        await _notificationService.NotifyOrderReadyAsync(id);
                        break;
                    case OrderStatus.Delivered:
                        await _notificationService.NotifyOrderCompletedAsync(id);
                        break;
                }
            }
            catch (Exception notificationEx)
            {
                _logger.LogWarning(notificationEx, "Erro ao enviar notificação de mudança de status do pedido {OrderId}", id);
                // Não falhar a atualização por erro de notificação
            }
            
            return ResultDto<OrderResponseDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status do pedido {OrderId}", id);
            return ResultDto<OrderResponseDto>.Fail("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém pedidos por status
    /// </summary>
    public async Task<ResultDto<IEnumerable<OrderResponseDto>>> GetOrdersByStatusAsync(OrderStatus status)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(status);
            var orderDtos = orders.Select(MapToOrderResponseDto);
            return ResultDto<IEnumerable<OrderResponseDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter pedidos por status {Status}", status);
            return ResultDto<IEnumerable<OrderResponseDto>>.Fail("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém pedidos por telefone do cliente
    /// </summary>
    public async Task<ResultDto<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerAsync(string customerPhone)
    {
        try
        {
            var orders = await _orderRepository.FindAsync(o => o.CustomerPhone == customerPhone);
            var orderDtos = orders.Select(MapToOrderResponseDto);
            return ResultDto<IEnumerable<OrderResponseDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter pedidos do cliente {CustomerPhone}", customerPhone);
            return ResultDto<IEnumerable<OrderResponseDto>>.Fail("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Aceita um pedido
    /// </summary>
    public async Task<ResultDto<OrderResponseDto>> AcceptOrderAsync(int id, int estimatedPreparationMinutes, string? notes = null, string? userId = null)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return ResultDto<OrderResponseDto>.Fail("Pedido não encontrado");

            if (order.Status != OrderStatus.Pending)
                return ResultDto<OrderResponseDto>.Fail("Apenas pedidos pendentes podem ser aceitos");

            order.Status = OrderStatus.Confirmed;
            order.EstimatedPreparationMinutes = estimatedPreparationMinutes;
            order.EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(estimatedPreparationMinutes + (order.IsDelivery ? 30 : 0));
            order.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(notes))
                order.Notes = notes;

            await _orderRepository.UpdateAsync(order);

            var orderDto = MapToOrderResponseDto(order);
            _logger.LogInformation("Pedido {OrderId} aceito com tempo estimado de {EstimatedMinutes} minutos", 
                id, estimatedPreparationMinutes);
            
            // Enviar notificação de pedido aceito
            try
            {
                await _notificationService.NotifyOrderAcceptedAsync(orderDto);
            }
            catch (Exception notificationEx)
            {
                _logger.LogWarning(notificationEx, "Erro ao enviar notificação de pedido aceito {OrderId}", id);
                // Não falhar a aceitação por erro de notificação
            }
            
            return ResultDto<OrderResponseDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aceitar pedido {OrderId}", id);
            return ResultDto<OrderResponseDto>.Fail("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Rejeita um pedido
    /// </summary>
    public async Task<ResultDto<OrderResponseDto>> RejectOrderAsync(int id, string reason, string? notes = null, string? rejectedBy = null)
    {
        try
        {
            var order = await _orderRepository.GetByIdWithIncludesAsync(id, o => o.Rejection);
            if (order == null)
                return ResultDto<OrderResponseDto>.Fail("Pedido não encontrado");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                return ResultDto<OrderResponseDto>.Fail("Apenas pedidos pendentes ou confirmados podem ser rejeitados");

            order.Status = OrderStatus.Rejected;
            order.UpdatedAt = DateTime.UtcNow;

            // Criar registro de rejeição
            var rejection = new OrderRejection
            {
                OrderId = order.Id,
                Reason = reason,
                Notes = notes,
                RejectedAt = DateTime.UtcNow,
                RejectedBy = rejectedBy ?? "system"
            };

            order.Rejection = rejection;
            await _orderRepository.UpdateAsync(order);

            var orderDto = MapToOrderResponseDto(order);
            _logger.LogInformation("Pedido {OrderId} rejeitado. Motivo: {Reason}", id, reason);
            
            // Enviar notificação de pedido rejeitado
            try
            {
                await _notificationService.NotifyOrderRejectedAsync(orderDto, reason);
            }
            catch (Exception notificationEx)
            {
                _logger.LogWarning(notificationEx, "Erro ao enviar notificação de pedido rejeitado {OrderId}", id);
                // Não falhar a rejeição por erro de notificação
            }
            
            return ResultDto<OrderResponseDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao rejeitar pedido {OrderId}", id);
            return ResultDto<OrderResponseDto>.Fail("Erro interno do servidor");
        }
    }

    #region Private Methods

    /// <summary>
    /// Mapeia uma entidade Order para OrderResponseDto
    /// </summary>
    private static OrderResponseDto MapToOrderResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            DeliveryAddress = order.DeliveryAddress,
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            Total = order.Total,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            IsDelivery = order.IsDelivery,
            Notes = order.Notes,
            EstimatedPreparationMinutes = order.EstimatedPreparationMinutes,
            EstimatedDeliveryTime = order.EstimatedDeliveryTime,
            QueuePosition = order.QueuePosition,
            Items = order.Items?.Select(MapToOrderItemResponseDto).ToList() ?? []
        };
    }

    /// <summary>
    /// Mapeia uma entidade OrderItem para OrderItemResponseDto
    /// </summary>
    private static OrderItemResponseDto MapToOrderItemResponseDto(OrderItem orderItem)
    {
        return new OrderItemResponseDto
        {
            Id = orderItem.Id,
            ProductId = orderItem.ProductId,
            ProductName = orderItem.ProductName,
            UnitPrice = orderItem.UnitPrice,
            Quantity = orderItem.Quantity,
            Subtotal = orderItem.Subtotal,
            Notes = orderItem.Notes,
            Addons = orderItem.Addons?.Select(MapToOrderItemAddonResponseDto).ToList() ?? []
        };
    }

    /// <summary>
    /// Mapeia uma entidade OrderItemAddon para OrderItemAddonResponseDto
    /// </summary>
    private static OrderItemAddonResponseDto MapToOrderItemAddonResponseDto(OrderItemAddon addon)
    {
        return new OrderItemAddonResponseDto
        {
            Id = addon.Id,
            AddonId = addon.AddonId,
            AddonName = addon.AddonName,
            UnitPrice = addon.UnitPrice,
            Quantity = addon.Quantity,
            Subtotal = addon.Subtotal
        };
    }

    /// <summary>
    /// Valida os itens do pedido
    /// </summary>
    private async Task<(bool IsValid, string ErrorMessage)> ValidateOrderItemsAsync(List<CreateOrderItemRequestDto> items)
    {
        if (!items.Any())
            return (false, "O pedido deve conter pelo menos um item");

        foreach (var item in items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
                return (false, $"Produto com ID {item.ProductId} não encontrado");

            if (!product.IsActive)
                return (false, $"Produto '{product.Name}' não está ativo");

            if (item.Quantity <= 0)
                return (false, "Quantidade deve ser maior que zero");

            // Validar addons
            foreach (var addon in item.Addons)
            {
                var addonEntity = await _addonRepository.GetByIdAsync(addon.AddonId);
                if (addonEntity == null)
                    return (false, $"Adicional com ID {addon.AddonId} não encontrado");

                if (!addonEntity.IsActive)
                    return (false, $"Adicional '{addonEntity.Name}' não está ativo");

                if (addon.Quantity <= 0)
                    return (false, "Quantidade do adicional deve ser maior que zero");
            }
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Valida se a transição de status é válida
    /// </summary>
    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus is OrderStatus.Confirmed or OrderStatus.Cancelled or OrderStatus.Rejected,
            OrderStatus.Confirmed => newStatus is OrderStatus.Preparing or OrderStatus.Cancelled or OrderStatus.Rejected,
            OrderStatus.Preparing => newStatus is OrderStatus.Ready or OrderStatus.Cancelled,
            OrderStatus.Ready => newStatus is OrderStatus.OutForDelivery or OrderStatus.Delivered or OrderStatus.Cancelled,
            OrderStatus.OutForDelivery => newStatus is OrderStatus.Delivered or OrderStatus.Cancelled,
            OrderStatus.Delivered => false, // Status final
            OrderStatus.Cancelled => false, // Status final
            OrderStatus.Rejected => false, // Status final
            _ => false
        };
    }

    #endregion
}
