using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Domain.Entities
{
    /// <summary>
    /// Representa um item adicional de um pedido.
    /// </summary>
    public class OrderItemAddon
    {
        /// <summary>
        /// Identificador único do item adicional do pedido.
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Identificador do item do pedido ao qual este item adicional está associado.
        /// </summary>
        public int OrderItemId { get; set; }
        /// <summary>
        /// Identificador do addon associado a este item adicional do pedido.
        /// </summary>
        public int AddonId { get; set; }
        /// <summary>
        /// Nome do adcional associado a este item adicional do pedido.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string AddonName { get; set; } = string.Empty; // Snapshot do nome
        /// <summary>
        /// Preço unitário do addon no momento da adição ao pedido.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; } // Snapshot do preço
        /// <summary>
        /// Quantidade do addon adicionada ao item do pedido.
        /// </summary>
        public int Quantity { get; set; } = 1;
        /// <summary>
        /// Subtotal calculado para este item adicional do pedido (UnitPrice * Quantity).
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
        public virtual OrderItem OrderItem { get; set; } = null!;
        public virtual Addon Addon { get; set; } = null!;
    }
}
