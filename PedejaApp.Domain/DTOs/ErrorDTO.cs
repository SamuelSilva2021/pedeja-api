using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// Detalhes de um erro ocorrido na API
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ErrorDTO
    {
        /// <summary>
        /// Código do erro
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Propriedade relacionada ao erro (se aplicável)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Property { get; set; }

        /// <summary>
        /// Mensagem descritiva do erro
        /// </summary>
        public string Message { get; set; } = string.Empty;

        public List<string>? Details { get; set; }
        public ErrorDTO() { }

        public ErrorDTO(string code, string message)
        {
            Code = code;
            Message = message;
        }
        public ErrorDTO(string code, string message, List<string> details)
        {
            Code = code;
            Message = message;
            Details = details;
        }

        public ErrorDTO(string code, string property, string message)
        {
            Code = code;
            Property = property;
            Message = message;
        }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            (Property != null ? $"{Property} - " : "") + Message;
    }
}
