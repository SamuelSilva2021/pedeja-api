using PedejaApp.Domain.Enuns;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PedejaApp.Domain.Entities
{
    public class AddonGroup : BaseEntity
    {
        /// <summary>
        /// Nome do grupo de adicionais, como "Frutas", "Coberturas", "Extras".
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Ex: "Frutas", "Coberturas", "Extras"
        /// <summary>
        /// Descrição opcional do grupo de adicionais, como "Escolha suas frutas favoritas".
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }
        /// <summary>
        /// Tipo de grupo de adicionais, que pode ser "Single" (única seleção) ou "Multiple" (múltiplas seleções).
        /// </summary>
        public AddonGroupType Type { get; set; } = AddonGroupType.Multiple;
        /// <summary>
        /// Número mínimo de seleções permitidas para este grupo de adicionais.
        /// </summary>
        public int? MinSelections { get; set; }
        /// <summary>
        /// Número máximo de seleções permitidas para este grupo de adicionais.
        /// </summary>
        public int? MaxSelections { get; set; }
        /// <summary>
        /// Indica se este grupo de adicionais é obrigatório para os produtos associados.
        /// </summary>
        public bool IsRequired { get; set; } = false;
        /// <summary>
        /// Ordem de exibição do grupo de adicionais na interface do usuário.
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
        /// <summary>
        /// Indica se o grupo de adicionais está ativo e disponível para seleção.
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Lista de adicionais associados a este grupo, como "Banana", "Bacon", etc.
        /// </summary>
        public virtual ICollection<Addon> Addons { get; set; } = new List<Addon>();
        /// <summary>
        /// Lista de associações entre produtos e este grupo de adicionais, permitindo que o grupo seja usado em diferentes produtos.
        /// </summary>
        public virtual ICollection<ProductAddonGroup> ProductAddonGroups { get; set; } = new List<ProductAddonGroup>();
    }
}
