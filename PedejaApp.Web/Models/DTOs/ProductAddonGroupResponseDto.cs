using PedejaApp.Domain.DTOs;

namespace PedejaApp.Web.Models.DTOs
{
    /// <summary>
    /// DTO de resposta para configuração de grupo de adicionais de um produto
    /// </summary>
    public class ProductAddonGroupResponseDto
    {
        /// <summary>
        /// ID da configuração produto-grupo
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID do produto
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// ID do grupo de adicionais
        /// </summary>
        public int AddonGroupId { get; set; }
        
        /// <summary>
        /// Informações do grupo de adicionais
        /// </summary>
        public AddonGroupResponseDto AddonGroup { get; set; } = new();
        
        /// <summary>
        /// Ordem de exibição deste grupo para o produto
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Se este grupo é obrigatório para este produto
        /// </summary>
        public bool IsRequired { get; set; }
        
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
