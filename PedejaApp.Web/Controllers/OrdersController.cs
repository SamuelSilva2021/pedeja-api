using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using ApiResponse = PedejaApp.Domain.DTOs.ApiResponse;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os pedidos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetOrders()
    {
        try
        {
            var result = await _orderService.GetOrdersAsync();
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.SuccessResponse(result.Data!, "Pedidos obtidos com sucesso"));
            }
            return BadRequest(ApiResponse<IEnumerable<OrderResponseDto>>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedidos");
            return StatusCode(500, ApiResponse<IEnumerable<OrderResponseDto>>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obtém um pedido específico por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetOrder(int id)
    {
        try
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(result.Data!, "Pedido obtido com sucesso"));
            }
            return NotFound(ApiResponse<OrderResponseDto>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedido {OrderId}", id);
            return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Cria um novo pedido
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> CreateOrder(CreateOrderRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse("Dados inválidos"));
            }

            var result = await _orderService.CreateOrderAsync(request);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetOrder), new { id = result.Data!.Id }, 
                    ApiResponse<OrderResponseDto>.SuccessResponse(result.Data, "Pedido criado com sucesso"));
            }
            return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido");
            return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Atualiza o status de um pedido
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateOrderStatus(int id, UpdateOrderStatusRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse("Dados inválidos"));
            }

            var result = await _orderService.UpdateOrderStatusAsync(id, request);
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(result.Data!, "Status do pedido atualizado com sucesso"));
            }
            return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status do pedido {OrderId}", id);
            return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Aceita um pedido
    /// </summary>
    [HttpPost("{id}/accept")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> AcceptOrder(int id, AcceptOrderRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse("Dados inválidos"));
            }

            var result = await _orderService.AcceptOrderAsync(id, request.EstimatedPreparationMinutes, request.Notes, request.UserId);
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(result.Data!, "Pedido aceito com sucesso"));
            }
            return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aceitar pedido {OrderId}", id);
            return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Rejeita um pedido
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> RejectOrder(int id, RejectOrderRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse("Dados inválidos"));
            }

            var result = await _orderService.RejectOrderAsync(id, request.Reason, request.Notes, request.RejectedBy);
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(result.Data!, "Pedido rejeitado com sucesso"));
            }
            return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao rejeitar pedido {OrderId}", id);
            return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Exclui um pedido
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteOrder(int id)
    {
        try
        {
            var result = await _orderService.DeleteOrderAsync(id);
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(result.Data, "Pedido excluído com sucesso"));
            }
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir pedido {OrderId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obtém pedidos por status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetOrdersByStatus(OrderStatus status)
    {
        try
        {
            var result = await _orderService.GetOrdersByStatusAsync(status);
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.SuccessResponse(result.Data!, "Pedidos obtidos com sucesso"));
            }
            return BadRequest(ApiResponse<IEnumerable<OrderResponseDto>>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedidos por status {Status}", status);
            return StatusCode(500, ApiResponse<IEnumerable<OrderResponseDto>>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obtém pedidos por telefone do cliente
    /// </summary>
    [HttpGet("customer/{customerPhone}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetOrdersByCustomer(string customerPhone)
    {
        try
        {
            var result = await _orderService.GetOrdersByCustomerAsync(customerPhone);
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.SuccessResponse(result.Data!, "Pedidos do cliente obtidos com sucesso"));
            }
            return BadRequest(ApiResponse<IEnumerable<OrderResponseDto>>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedidos do cliente {CustomerPhone}", customerPhone);
            return StatusCode(500, ApiResponse<IEnumerable<OrderResponseDto>>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Atualiza informações básicas de um pedido
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateOrder(int id, UpdateOrderRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse("Dados inválidos"));
            }

            var result = await _orderService.UpdateOrderAsync(id, request);
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(result.Data!, "Pedido atualizado com sucesso"));
            }
            return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar pedido {OrderId}", id);
            return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("Erro interno do servidor"));
        }
    }
}