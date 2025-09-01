using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;

namespace PedejaApp.Application.Services.Interfaces;

/// <summary>
/// Interface para serviços de pagamento seguindo princípios SOLID e Clean Architecture
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Obter todos os pagamentos
    /// </summary>
    /// <returns>Lista de pagamentos</returns>
    Task<IEnumerable<PaymentResponseDto>> GetPaymentsAsync();
    
    /// <summary>
    /// Processar um pagamento
    /// </summary>
    /// <param name="request">Dados do pagamento</param>
    /// <returns>Resultado do processamento</returns>
    Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequest request);
    
    /// <summary>
    /// Gerar pagamento PIX
    /// </summary>
    /// <param name="request">Dados do PIX</param>
    /// <returns>Dados do PIX gerado</returns>
    Task<PixResponseDto> GeneratePixPaymentAsync(PixRequest request);
    
    /// <summary>
    /// Obter status de um pagamento
    /// </summary>
    /// <param name="paymentId">ID do pagamento</param>
    /// <returns>Status do pagamento</returns>
    Task<PaymentStatusDto> GetPaymentStatusAsync(int paymentId);
    
    /// <summary>
    /// Processar estorno de pagamento
    /// </summary>
    /// <param name="request">Dados do estorno</param>
    /// <returns>Resultado do estorno</returns>
    Task<RefundResponseDto> RefundPaymentAsync(RefundRequest request);
}

/// <summary>
/// DTOs para requisições de pagamento
/// </summary>
public class PaymentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? CardToken { get; set; }
    public string? CardHolderName { get; set; }
    public string? Notes { get; set; }
}

public class PixRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class RefundRequest
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RefundedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTOs para respostas de pagamento
/// </summary>
public class PaymentResponseDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? GatewayTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Notes { get; set; }
}

public class PixResponseDto
{
    public string PixId { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class PaymentStatusDto
{
    public int PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public string? GatewayTransactionId { get; set; }
}

public class RefundResponseDto
{
    public int RefundId { get; set; }
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RefundedAt { get; set; }
    public string RefundedBy { get; set; } = string.Empty;
    public string? GatewayRefundId { get; set; }
}