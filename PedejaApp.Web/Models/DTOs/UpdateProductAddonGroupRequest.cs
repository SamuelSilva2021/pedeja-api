namespace PedejaApp.Web.Models.DTOs
{
    /// <summary>
    /// DTO para atualizar a configuração de um grupo de adicionais em um produto
    /// </summary>
    public class UpdateProductAddonGroupRequest
    {
        /// <summary>
        /// Ordem de exibição deste grupo para o produto
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
        
        /// <summary>
        /// Se este grupo é obrigatório para este produto
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
