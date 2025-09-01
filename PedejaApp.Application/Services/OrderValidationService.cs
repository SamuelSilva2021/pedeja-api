using Microsoft.Extensions.Logging;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Enuns;
using PedejaApp.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace PedejaApp.Application.Services;

/// <summary>
/// Serviço para validações de negócio relacionadas a pedidos
/// </summary>
public class OrderValidationService : IOrderValidationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderValidationService> _logger;

    public OrderValidationService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ILogger<OrderValidationService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Valida a criação de um novo pedido
    /// </summary>
    public async Task<ApiResponse<bool>> ValidateCreateOrderAsync(CreateOrderRequestDto request)
    {
        try
        {
            // Validar informações do cliente
            var customerValidation = ValidateCustomerInfo(request.CustomerName, request.CustomerPhone);
            if (!customerValidation.Success)
                return customerValidation;

            // Validar se há itens no pedido
            if (request.Items == null || !request.Items.Any())
            {
                return ApiResponse<bool>.ErrorResponse("O pedido deve conter pelo menos um item.");
            }

            // Validar itens do pedido
            var itemsValidation = await ValidateOrderItemsAsync(request.Items);
            if (!itemsValidation.Success)
                return itemsValidation;

            return ApiResponse<bool>.SuccessResponse(true, "Pedido válido para criação.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar criação de pedido");
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar pedido.");
        }
    }

    /// <summary>
    /// Valida a atualização de um pedido
    /// </summary>
    public async Task<ApiResponse<bool>> ValidateUpdateOrderAsync(int orderId, UpdateOrderRequestDto request)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return ApiResponse<bool>.ErrorResponse("Pedido não encontrado.");
            }

            // Verificar se o pedido pode ser atualizado
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            {
                return ApiResponse<bool>.ErrorResponse("Não é possível atualizar pedidos finalizados ou cancelados.");
            }

            // Validar informações do cliente se fornecidas
            if (!string.IsNullOrWhiteSpace(request.CustomerName) || !string.IsNullOrWhiteSpace(request.CustomerPhone))
            {
                var customerValidation = ValidateCustomerInfo(
                    request.CustomerName ?? order.CustomerName,
                    request.CustomerPhone ?? order.CustomerPhone);
                if (!customerValidation.Success)
                    return customerValidation;
            }

            return ApiResponse<bool>.SuccessResponse(true, "Pedido válido para atualização.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar atualização de pedido {OrderId}", orderId);
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar atualização.");
        }
    }

    /// <summary>
    /// Valida a mudança de status de um pedido
    /// </summary>
    public async Task<ApiResponse<bool>> ValidateStatusChangeAsync(int orderId, OrderStatus newStatus)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return ApiResponse<bool>.ErrorResponse("Pedido não encontrado.");
            }

            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                return ApiResponse<bool>.BadRequest($"Transição de status inválida: de '{order.Status}' para '{newStatus}'.");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Transição de status válida.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar transição de status do pedido {OrderId}", orderId);
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar transição.");
        }
    }

    /// <summary>
    /// Valida se um pedido pode ser aceito
    /// </summary>
    public async Task<ApiResponse<bool>> ValidateAcceptOrderAsync(int orderId, AcceptOrderRequestDto request)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return ApiResponse<bool>.ErrorResponse("Pedido não encontrado.");
            }

            if (order.Status != OrderStatus.Pending)
            {
                return ApiResponse<bool>.BadRequest("Apenas pedidos pendentes podem ser aceitos.");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Pedido pode ser aceito.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar aceitação de pedido {OrderId}", orderId);
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar aceitação.");
        }
    }

    /// <summary>
    /// Valida se um pedido pode ser rejeitado
    /// </summary>
    public async Task<ApiResponse<bool>> ValidateRejectOrderAsync(int orderId, RejectOrderRequestDto request)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return ApiResponse<bool>.ErrorResponse("Pedido não encontrado.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return ApiResponse<bool>.BadRequest("Motivo da rejeição é obrigatório.");
            }

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                return ApiResponse<bool>.BadRequest("Apenas pedidos pendentes ou confirmados podem ser rejeitados.");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Pedido pode ser rejeitado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar rejeição de pedido {OrderId}", orderId);
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar rejeição.");
        }
    }

    /// <summary>
    /// Valida se um pedido pode ser cancelado
    /// </summary>
    public async Task<ApiResponse<bool>> ValidateCancelOrderAsync(int orderId)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return ApiResponse<bool>.ErrorResponse("Pedido não encontrado.");
            }

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Rejected)
            {
                return ApiResponse<bool>.BadRequest($"Não é possível cancelar um pedido com status '{order.Status}'.");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Pedido pode ser cancelado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar cancelamento de pedido {OrderId}", orderId);
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar cancelamento.");
        }
    }

    /// <summary>
    /// Valida se um pedido pode ser excluído
    /// </summary>
    public async Task<ApiResponse<bool>> ValidateDeleteOrderAsync(int orderId)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return ApiResponse<bool>.ErrorResponse("Pedido não encontrado.");
            }

            // Apenas pedidos pendentes ou cancelados podem ser excluídos
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Cancelled)
            {
                return ApiResponse<bool>.ErrorResponse("Apenas pedidos pendentes ou cancelados podem ser excluídos.");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Pedido válido para exclusão.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar exclusão do pedido {OrderId}", orderId);
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar exclusão.");
        }
    }

    /// <summary>
    /// Valida os itens de um pedido
    /// </summary>
    public async Task<ApiResponse<bool>> ValidateOrderItemsAsync(IEnumerable<CreateOrderItemRequestDto> items)
    {
        try
        {
            var itemList = items.ToList();
            if (!itemList.Any())
            {
                return ApiResponse<bool>.BadRequest("O pedido deve conter pelo menos um item.");
            }

            foreach (var item in itemList)
            {
                var itemValidation = await ValidateItemAsync(item);
                if (!itemValidation.Success)
                {
                    return itemValidation;
                }
            }

            return ApiResponse<bool>.SuccessResponse(true, "Todos os itens são válidos.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar itens do pedido");
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar itens.");
        }
    }

    /// <summary>
    /// Valida um item do pedido
    /// </summary>
    public async Task<ApiResponse<bool>> ValidateItemAsync(CreateOrderItemRequestDto item)
    {
        try
        {
            if (item.Quantity <= 0)
            {
                return ApiResponse<bool>.BadRequest("Quantidade deve ser maior que zero.");
            }

            // O preço unitário será obtido do produto, não do DTO

            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                return ApiResponse<bool>.BadRequest($"Produto com ID {item.ProductId} não encontrado.");
            }

            if (!product.IsActive)
            {
                return ApiResponse<bool>.BadRequest($"Produto '{product.Name}' não está disponível.");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Item válido.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar item do pedido {ProductId}", item.ProductId);
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar item.");
        }
    }

    /// <summary>
    /// Valida se uma transição de status é válida
    /// </summary>
    public bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus is OrderStatus.Confirmed or OrderStatus.Rejected or OrderStatus.Cancelled,
            OrderStatus.Confirmed => newStatus is OrderStatus.Preparing or OrderStatus.Rejected or OrderStatus.Cancelled,
            OrderStatus.Preparing => newStatus is OrderStatus.Ready or OrderStatus.Cancelled,
            OrderStatus.Ready => newStatus is OrderStatus.OutForDelivery or OrderStatus.Delivered or OrderStatus.Cancelled,
            OrderStatus.OutForDelivery => newStatus is OrderStatus.Delivered or OrderStatus.Cancelled,
            OrderStatus.Delivered => false, // Status final
            OrderStatus.Rejected => false, // Status final
            OrderStatus.Cancelled => false, // Status final
            _ => false
        };
    }

    /// <summary>
    /// Valida informações do cliente
    /// </summary>
    public ApiResponse<bool> ValidateCustomerInfo(string customerName, string customerPhone)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                return ApiResponse<bool>.BadRequest("Nome do cliente é obrigatório.");
            }

            if (customerName.Length < 2)
            {
                return ApiResponse<bool>.BadRequest("Nome do cliente deve ter pelo menos 2 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(customerPhone))
            {
                // Validar formato do telefone (apenas números, com 10 ou 11 dígitos)
                var phoneRegex = new Regex(@"^\d{10,11}$");
                var cleanPhone = customerPhone.Replace("[", "").Replace("]", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                if (!phoneRegex.IsMatch(cleanPhone))
                {
                    return ApiResponse<bool>.BadRequest("Formato de telefone inválido.");
                }
            }

            return ApiResponse<bool>.SuccessResponse(true, "Informações do cliente válidas.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar informações do cliente");
            return ApiResponse<bool>.ErrorResponse("Erro interno ao validar cliente.");
        }
    }
}