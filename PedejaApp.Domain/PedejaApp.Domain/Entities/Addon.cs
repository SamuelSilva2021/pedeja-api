using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Domain.Entities
{
    public class Addon : BaseEntity
    {
        /// <summary>
        /// Identificador único do adicional.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Ex: "Banana", "Bacon"
        /// <summary>
        /// Descrição opcional do adicional, como "Banana fatiada", "Bacon crocante".
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }
        /// <summary>
        /// Preço do adicional, que pode ser zero se for gratuito.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        /// <summary>
        /// Identificador do grupo de adicionais ao qual este adicional pertence.
        /// </summary>
        public int AddonGroupId { get; set; }
        /// <summary>
        /// Ordem de exibição do adicional dentro do grupo, permitindo que sejam exibidos em uma ordem específica.
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
        /// <summary>
        /// Indica se o adicional está ativo e disponível para seleção.
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// URL da imagem do adicional, que pode ser usada para exibir uma foto do adicional na interface do usuário.
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        /// <summary>
        /// Grupo de adicionais ao qual este adicional pertence, permitindo que o adicional seja organizado dentro de um grupo específico.
        /// </summary>
        public virtual AddonGroup AddonGroup { get; set; } = null!;
        public virtual ICollection<OrderItemAddon> OrderItemAddons { get; set; } = new List<OrderItemAddon>();
    }
}
