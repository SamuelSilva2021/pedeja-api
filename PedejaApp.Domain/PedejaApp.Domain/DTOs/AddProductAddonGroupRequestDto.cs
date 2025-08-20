namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO para adicionar um grupo de adicionais a um produto
    /// </summary>
    public class AddProductAddonGroupRequestDto
    {
        /// <summary>
        /// ID do grupo de adicionais
        /// </summary>
        public int AddonGroupId { get; set; }
        
        /// <summary>
        /// Ordem de exibição deste grupo para o produto
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
        
        /// <summary>
        /// Se este grupo é obrigatório para este produto (override)
        /// </summary>
        public bool IsRequired { get; set; } = false;
        
        /// <summary>
        /// Override do número mínimo de seleções para este produto
        /// </summary>
        public int? MinSelectionsOverride { get; set; }
        
        /// <summary>
        /// Override do número máximo de seleções para este produto
        /// </summary>
        public int? MaxSelectionsOverride { get; set; }
    }
}
