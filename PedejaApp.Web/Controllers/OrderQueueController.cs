using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using ApiResponse = PedejaApp.Domain.DTOs.ApiResponse;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderQueueController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderQueueController> _logger;

    public OrderQueueController(IOrderService orderService, ILogger<OrderQueueController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get orders in queue with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetOrderQueue(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var result = await _orderService.GetOrdersAsync();
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.SuccessResponse(result.Data!, "Fila de pedidos obtida com sucesso"));
            }
            return BadRequest(ApiResponse<IEnumerable<OrderResponseDto>>.ErrorResponse(result.Error!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fila de pedidos");
            return StatusCode(500, ApiResponse<IEnumerable<OrderResponseDto>>.ErrorResponse("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Accept an order and add it to the preparation queue
    /// </summary>
    [HttpPost("{id}/accept")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> AcceptOrder(int id, [FromBody] AcceptOrderRequestDto request)
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
    /// Reject an order with a reason
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> RejectOrder(int id, [FromBody] RejectOrderRequestDto request)
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
    /// Update order status in the preparation workflow
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequestDto request)
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

}
