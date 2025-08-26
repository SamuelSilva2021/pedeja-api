using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PedejaApp.Infrastructure.Services
{
    public interface IPixService
    {
        Task<PixQRCodeResponse> GenerateQRCode(decimal amount, string description, string orderId);
        Task<PixValidationResult> ValidatePixPayment(string transactionId);
        string GeneratePixPayload(decimal amount, string description, string merchantKey, string? transactionId = null);
    }

    public class PixService : IPixService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PixService> _logger;

        public PixService(IConfiguration configuration, ILogger<PixService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PixQRCodeResponse> GenerateQRCode(decimal amount, string description, string orderId)
        {
            try
            {
                var merchantKey = _configuration["PIX:MerchantKey"] ?? "exemplo@banco.com.br";
                var merchantName = _configuration["PIX:MerchantName"] ?? "PEDEJA RESTAURANT LTDA";
                var merchantCity = _configuration["PIX:MerchantCity"] ?? "SAO PAULO";
                
                var transactionId = GenerateTransactionId(orderId);
                var pixPayload = GeneratePixPayload(amount, description, merchantKey, transactionId);
                
                // Simular QR Code image (em produção seria gerado com biblioteca QR Code)
                var qrCodeImage = await GenerateQRCodeImage(pixPayload);

                var response = new PixQRCodeResponse
                {
                    QRCode = pixPayload,
                    QRCodeImage = qrCodeImage,
                    TransactionId = transactionId,
                    Amount = amount,
                    MerchantName = merchantName,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30) // PIX expira em 30 minutos
                };

                _logger.LogInformation("PIX QR Code gerado para pedido {OrderId}, valor {Amount}, transação {TransactionId}",
                    orderId, amount, transactionId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar QR Code PIX para pedido {OrderId}", orderId);
                throw;
            }
        }

        public async Task<PixValidationResult> ValidatePixPayment(string transactionId)
        {
            try
            {
                // TODO: Implementar integração real com API do banco para consultar status do PIX
                await Task.Delay(500); // Simular chamada à API

                // Simular validação (em produção, consultaria API do banco)
                var isValid = !string.IsNullOrEmpty(transactionId) && transactionId.Length >= 10;
                
                return new PixValidationResult
                {
                    IsValid = isValid,
                    Status = isValid ? "PAID" : "PENDING",
                    TransactionId = transactionId,
                    ValidatedAt = DateTime.UtcNow,
                    Amount = 0, // Seria retornado pela API do banco
                    PayerDocument = "***.***.***-**" // Mascarado por segurança
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar pagamento PIX {TransactionId}", transactionId);
                return new PixValidationResult { IsValid = false, Status = "ERROR" };
            }
        }

        public string GeneratePixPayload(decimal amount, string description, string merchantKey, string? transactionId = null)
        {
            try
            {
                transactionId ??= Guid.NewGuid().ToString("N")[..25];
                
                // Formato PIX BR Code (EMVCo)
                var payload = new StringBuilder();
                
                // Payload Format Indicator
                payload.Append("000201");
                
                // Point of Initiation Method
                payload.Append("010212");
                
                // Merchant Account Information (PIX Key)
                var pixKeyData = $"0014BR.GOV.BCB.PIX01{merchantKey.Length:D2}{merchantKey}";
                payload.Append($"26{pixKeyData.Length:D2}{pixKeyData}");
                
                // Merchant Category Code
                payload.Append("52040000");
                
                // Transaction Currency (BRL)
                payload.Append("5303986");
                
                // Transaction Amount
                if (amount > 0)
                {
                    var amountStr = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    payload.Append($"54{amountStr.Length:D2}{amountStr}");
                }
                
                // Country Code
                payload.Append("5802BR");
                
                // Merchant Name
                var merchantName = "PEDEJA RESTAURANT";
                payload.Append($"59{merchantName.Length:D2}{merchantName}");
                
                // Merchant City
                var merchantCity = "SAO PAULO";
                payload.Append($"60{merchantCity.Length:D2}{merchantCity}");
                
                // Additional Data Field Template
                if (!string.IsNullOrEmpty(transactionId))
                {
                    var additionalData = $"05{transactionId.Length:D2}{transactionId}";
                    payload.Append($"62{additionalData.Length:D2}{additionalData}");
                }
                
                // CRC16 (placeholder - em produção seria calculado)
                payload.Append("6304");
                
                var payloadString = payload.ToString();
                var crc = CalculateCRC16(payloadString);
                payloadString += crc.ToString("X4");
                
                return payloadString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar payload PIX");
                throw;
            }
        }

        private string GenerateTransactionId(string orderId)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"PDJ{orderId:D6}{timestamp}".Substring(0, 25);
        }

        private async Task<string> GenerateQRCodeImage(string payload)
        {
            // TODO: Implementar geração real de QR Code usando biblioteca como QRCoder
            await Task.Delay(50); // Simular processamento
            
            // Retornar imagem base64 placeholder (em produção seria QR Code real)
            return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
        }

        private ushort CalculateCRC16(string payload)
        {
            // Implementação simplificada do CRC16 para PIX
            var bytes = Encoding.UTF8.GetBytes(payload);
            ushort crc = 0xFFFF;
            
            foreach (var b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            
            return (ushort)(crc & 0xFFFF);
        }
    }

    #region DTOs

    public class PixQRCodeResponse
    {
        public string QRCode { get; set; } = string.Empty;
        public string QRCodeImage { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class PixValidationResult
    {
        public bool IsValid { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime ValidatedAt { get; set; }
        public decimal Amount { get; set; }
        public string PayerDocument { get; set; } = string.Empty;
    }

    #endregion
}
