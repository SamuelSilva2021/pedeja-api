using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedejaApp.Infrastructure.Data;
using PedejaApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ApplicationDbContext context, ILogger<PaymentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] PaymentRequest request)
        {
            try
            {
                // Validate order exists
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return NotFound($"Order {request.OrderId} not found");
                }

                // Check if order is in a valid state for payment
                if (order.Status != OrderStatus.Confirmed)
                {
                    return BadRequest($"Order {request.OrderId} is not in a valid state for payment (current status: {order.Status})");
                }

                // Create payment record
                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    Method = request.Method,
                    Status = PaymentStatus.Processing,
                    CreatedAt = DateTime.UtcNow,
                    Notes = request.Notes
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // TODO: Process payment with gateway based on method
                var paymentResult = await ProcessPaymentWithGateway(payment, request);

                // Update payment status
                payment.Status = paymentResult.Success ? PaymentStatus.Approved : PaymentStatus.Declined;
                payment.ProcessedAt = DateTime.UtcNow;
                payment.GatewayTransactionId = paymentResult.TransactionId;
                payment.GatewayResponse = paymentResult.GatewayResponse;

                await _context.SaveChangesAsync();

                var response = new PaymentResponse
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Status = payment.Status,
                    TransactionId = payment.GatewayTransactionId,
                    Success = paymentResult.Success,
                    Message = paymentResult.Message,
                    ProcessedAt = payment.ProcessedAt
                };

                _logger.LogInformation("Payment {PaymentId} processed for order {OrderId} with status {Status}",
                    payment.Id, payment.OrderId, payment.Status);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
                return StatusCode(500, "An error occurred while processing the payment");
            }
        }

        [HttpPost("pix/generate")]
        public async Task<ActionResult<PixResponse>> GeneratePixPayment([FromBody] PixRequest request)
        {
            try
            {
                // Validate order exists
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return NotFound($"Order {request.OrderId} not found");
                }

                // Create PIX payment record
                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    Method = PaymentMethod.PIX,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Notes = "PIX payment generated"
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // TODO: Generate PIX QR Code
                var pixResult = await GeneratePixQRCode(payment);

                payment.GatewayTransactionId = pixResult.TransactionId;
                payment.GatewayResponse = pixResult.QRCode;
                await _context.SaveChangesAsync();

                var response = new PixResponse
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    QRCode = pixResult.QRCode,
                    QRCodeImage = pixResult.QRCodeImage,
                    TransactionId = pixResult.TransactionId,
                    Amount = payment.Amount,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30) // PIX expires in 30 minutes
                };

                _logger.LogInformation("PIX payment {PaymentId} generated for order {OrderId}",
                    payment.Id, payment.OrderId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PIX payment for order {OrderId}", request.OrderId);
                return StatusCode(500, "An error occurred while generating PIX payment");
            }
        }

        [HttpGet("{paymentId}/status")]
        public async Task<ActionResult<PaymentStatusResponse>> GetPaymentStatus(int paymentId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    return NotFound($"Payment {paymentId} not found");
                }

                var response = new PaymentStatusResponse
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Status = payment.Status,
                    Amount = payment.Amount,
                    Method = payment.Method,
                    TransactionId = payment.GatewayTransactionId,
                    CreatedAt = payment.CreatedAt,
                    ProcessedAt = payment.ProcessedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for payment {PaymentId}", paymentId);
                return StatusCode(500, "An error occurred while getting payment status");
            }
        }

        [HttpPost("{paymentId}/refund")]
        public async Task<ActionResult<RefundResponse>> RefundPayment(int paymentId, [FromBody] RefundRequest request)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Refunds)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    return NotFound($"Payment {paymentId} not found");
                }

                if (payment.Status != PaymentStatus.Approved)
                {
                    return BadRequest($"Payment {paymentId} cannot be refunded (status: {payment.Status})");
                }

                // Validate refund amount
                var totalRefunded = payment.Refunds.Sum(r => r.Amount);
                var availableForRefund = payment.Amount - totalRefunded;

                if (request.Amount > availableForRefund)
                {
                    return BadRequest($"Refund amount {request.Amount:C} exceeds available amount {availableForRefund:C}");
                }

                // Create refund record
                var refund = new PaymentRefund
                {
                    PaymentId = paymentId,
                    Amount = request.Amount,
                    Reason = request.Reason,
                    RefundedAt = DateTime.UtcNow,
                    RefundedBy = request.RefundedBy
                };

                _context.PaymentRefunds.Add(refund);

                // Update payment status if fully refunded
                if (totalRefunded + request.Amount >= payment.Amount)
                {
                    payment.Status = PaymentStatus.Refunded;
                }

                await _context.SaveChangesAsync();

                // TODO: Process refund with gateway
                var refundResult = await ProcessRefundWithGateway(refund);

                refund.GatewayRefundId = refundResult.RefundId;
                refund.GatewayResponse = refundResult.GatewayResponse;
                await _context.SaveChangesAsync();

                var response = new RefundResponse
                {
                    RefundId = refund.Id,
                    PaymentId = refund.PaymentId,
                    Amount = refund.Amount,
                    Success = refundResult.Success,
                    Message = refundResult.Message,
                    RefundedAt = refund.RefundedAt
                };

                _logger.LogInformation("Refund {RefundId} processed for payment {PaymentId} amount {Amount}",
                    refund.Id, paymentId, request.Amount);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
                return StatusCode(500, "An error occurred while processing the refund");
            }
        }

        // TODO: Implement webhook handlers
        [HttpPost("webhook/stripe")]
        public async Task<IActionResult> StripeWebhook([FromBody] string payload)
        {
            // TODO: Implement Stripe webhook handler
            return Ok();
        }

        [HttpPost("webhook/pagseguro")]
        public async Task<IActionResult> PagSeguroWebhook([FromBody] object notification)
        {
            // TODO: Implement PagSeguro webhook handler
            return Ok();
        }

        #region Private Methods

        private async Task<PaymentGatewayResult> ProcessPaymentWithGateway(Payment payment, PaymentRequest request)
        {
            // TODO: Implement actual gateway integration
            // For now, simulate payment processing
            await Task.Delay(100); // Simulate API call

            return new PaymentGatewayResult
            {
                Success = true, // Simulate success for demo
                TransactionId = Guid.NewGuid().ToString("N")[..20],
                Message = "Payment processed successfully",
                GatewayResponse = "Simulated gateway response"
            };
        }

        private async Task<PixResult> GeneratePixQRCode(Payment payment)
        {
            // TODO: Implement actual PIX integration
            await Task.Delay(100); // Simulate API call

            return new PixResult
            {
                QRCode = $"00020101021226370014BR.GOV.BCB.PIX0114{payment.OrderId:000000000000}5204000053039865802BR5925PEDEJA RESTAURANT LTDA6009SAO PAULO62070503***6304ABCD",
                QRCodeImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==", // Placeholder base64 image
                TransactionId = Guid.NewGuid().ToString("N")[..20]
            };
        }

        private async Task<RefundGatewayResult> ProcessRefundWithGateway(PaymentRefund refund)
        {
            // TODO: Implement actual gateway refund
            await Task.Delay(100); // Simulate API call

            return new RefundGatewayResult
            {
                Success = true,
                RefundId = Guid.NewGuid().ToString("N")[..20],
                Message = "Refund processed successfully",
                GatewayResponse = "Simulated refund response"
            };
        }

        #endregion
    }

    #region DTOs and Models

    public class PaymentRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        public string? Notes { get; set; }

        // Card payment specific fields
        public string? CardToken { get; set; }
        public string? CardHolderName { get; set; }
    }

    public class PaymentResponse
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
    }

    public class PixRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }

    public class PixResponse
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public string QRCodeImage { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class PaymentStatusResponse
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class RefundRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public string RefundedBy { get; set; } = string.Empty;
    }

    public class RefundResponse
    {
        public int RefundId { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime RefundedAt { get; set; }
    }

    // Internal models
    internal class PaymentGatewayResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string GatewayResponse { get; set; } = string.Empty;
    }

    internal class PixResult
    {
        public string QRCode { get; set; } = string.Empty;
        public string QRCodeImage { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
    }

    internal class RefundGatewayResult
    {
        public bool Success { get; set; }
        public string RefundId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string GatewayResponse { get; set; } = string.Empty;
    }

    #endregion
}
