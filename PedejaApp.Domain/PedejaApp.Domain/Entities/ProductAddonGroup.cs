using System.ComponentModel.DataAnnotations;


namespace PedejaApp.Domain.Entities
{
    /// <summary>
    /// Representa a associação entre um produto e um grupo de addons.
    /// </summary>
    public class ProductAddonGroup
    {
        /// <summary>
        /// Identificador único da associação entre produto e grupo de adicionais.
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Identificador do produto associado a este grupo de adicionais.
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Identificador do grupo de adicionais associado a este produto.
        /// </summary>
        public int AddonGroupId { get; set; }
        /// <summary>
        /// Ordem de exibição do grupo de adicionais para este produto.
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
        /// <summary>
        /// Indica se o grupo de adicionais é obrigatório para este produto.
        /// </summary>
        public bool IsRequired { get; set; } = false;
        /// <summary>
        /// Número mínimo de seleções permitidas para este grupo de adicionais.
        /// </summary>
        public int? MinSelectionsOverride { get; set; }
        /// <summary>
        /// Número máximo de seleções permitidas para este grupo de adicionais.
        /// </summary>
        public int? MaxSelectionsOverride { get; set; }
        /// <summary>
        /// Representa o produto associado a este grupo de adicionais.
        /// </summary>
        public virtual Product Product { get; set; } = null!;
        /// <summary>
        /// Representa o grupo de adicionais associado a este produto.
        /// </summary>
        public virtual AddonGroup AddonGroup { get; set; } = null!;
    }
}
