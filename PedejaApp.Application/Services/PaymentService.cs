using Microsoft.Extensions.Logging;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;

namespace PedejaApp.Application.Services;

/// <summary>
/// Serviço para gerenciamento de pagamentos seguindo princípios SOLID e Clean Architecture
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<PaymentRefund> _refundRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IRepository<Payment> paymentRepository,
        IRepository<PaymentRefund> refundRepository,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _refundRepository = refundRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obter todos os pagamentos
    /// </summary>
    public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsAsync()
    {
        try
        {
            var payments = await _paymentRepository.GetAllAsync();
            return payments.Select(p => new PaymentResponseDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount,
                Method = p.Method,
                Status = p.Status,
                GatewayTransactionId = p.GatewayTransactionId,
                CreatedAt = p.CreatedAt,
                ProcessedAt = p.ProcessedAt,
                Notes = p.Notes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pagamentos");
            throw;
        }
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processando pagamento para pedido {OrderId}", request.OrderId);
            
            if (request.Amount <= 0)
                throw new ArgumentException("Valor do pagamento deve ser maior que zero");
            
            var payment = new Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Method = request.Method,
                Status = PaymentStatus.Pending,
                Notes = request.Notes
            };
            
            // Simular processamento baseado no método
            switch (request.Method)
            {
                case PaymentMethod.Cash:
                    payment.Status = PaymentStatus.Approved;
                    payment.ProcessedAt = DateTime.UtcNow;
                    break;
                case PaymentMethod.Pix:
                    payment.Status = PaymentStatus.Processing;
                    break;
                default:
                    payment.Status = PaymentStatus.Processing;
                    break;
            }
            
            var savedPayment = await _paymentRepository.AddAsync(payment);
            
            return new PaymentResponseDto
            {
                Id = savedPayment.Id,
                OrderId = savedPayment.OrderId,
                Amount = savedPayment.Amount,
                Method = savedPayment.Method,
                Status = savedPayment.Status,
                GatewayTransactionId = savedPayment.GatewayTransactionId,
                CreatedAt = savedPayment.CreatedAt,
                ProcessedAt = savedPayment.ProcessedAt,
                Notes = savedPayment.Notes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento");
            throw;
        }
    }

    public async Task<PixResponseDto> GeneratePixPaymentAsync(PixRequest request)
    {
        try
        {
            _logger.LogInformation("Gerando PIX para pedido {OrderId}", request.OrderId);
            
            if (request.Amount <= 0)
                throw new ArgumentException("Valor do PIX deve ser maior que zero");
            
            var pixId = $"pix_{Guid.NewGuid().ToString("N")[..20]}";
            
            var payment = new Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Method = PaymentMethod.Pix,
                Status = PaymentStatus.Pending,
                GatewayTransactionId = pixId,
                Notes = "Pagamento PIX gerado"
            };
            
            await _paymentRepository.AddAsync(payment);
            
            return new PixResponseDto
            {
                PixId = pixId,
                QrCode = $"00020126580014br.gov.bcb.pix0136{pixId}520400005303986540{request.Amount:F2}5802BR6009SAO PAULO62070503***6304",
                QrCodeBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"PIX-{pixId}")),
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                Amount = request.Amount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar PIX");
            throw;
        }
    }

    public async Task<PaymentStatusDto> GetPaymentStatusAsync(int paymentId)
    {
        try
        {
            _logger.LogInformation("Consultando status do pagamento {PaymentId}", paymentId);
            
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            
            if (payment == null)
                throw new ArgumentException("Pagamento não encontrado");
            
            return new PaymentStatusDto
            {
                PaymentId = payment.Id,
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                Method = payment.Method.ToString(),
                ProcessedAt = payment.ProcessedAt,
                GatewayTransactionId = payment.GatewayTransactionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status do pagamento");
            throw;
        }
    }

    public async Task<RefundResponseDto> RefundPaymentAsync(RefundRequest request)
    {
        try
        {
            _logger.LogInformation("Processando estorno do pagamento {PaymentId}", request.PaymentId);
            
            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);
            
            if (payment == null)
                throw new ArgumentException("Pagamento não encontrado");
            
            if (payment.Status != PaymentStatus.Approved)
                throw new ArgumentException("Apenas pagamentos aprovados podem ser estornados");
            
            if (request.Amount <= 0 || request.Amount > payment.Amount)
                throw new ArgumentException("Valor do estorno inválido");
            
            var refundId = $"refund_{Guid.NewGuid().ToString("N")[..20]}";
            
            var refund = new PaymentRefund
            {
                PaymentId = payment.Id,
                Amount = request.Amount,
                Reason = request.Reason ?? "Estorno solicitado",
                RefundedBy = request.RefundedBy ?? "Sistema",
                GatewayRefundId = refundId,
                RefundedAt = DateTime.UtcNow
            };
            
            await _refundRepository.AddAsync(refund);
            
            if (request.Amount >= payment.Amount)
            {
                payment.Status = PaymentStatus.Refunded;
                await _paymentRepository.UpdateAsync(payment);
            }
            
            return new RefundResponseDto
            {
                RefundId = refund.Id,
                PaymentId = payment.Id,
                Amount = refund.Amount,
                Reason = refund.Reason,
                RefundedAt = refund.RefundedAt,
                RefundedBy = refund.RefundedBy,
                GatewayRefundId = refund.GatewayRefundId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar estorno");
            throw;
        }
    }

    /// <summary>
    /// Mapear Payment para PaymentResponseDto
    /// </summary>
    private static PaymentResponseDto MapToPaymentResponseDto(Payment payment)
    {
        return new PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Method = payment.Method,
            Status = payment.Status,
            GatewayTransactionId = payment.GatewayTransactionId,
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt,
            Notes = payment.Notes
        };
    }
}