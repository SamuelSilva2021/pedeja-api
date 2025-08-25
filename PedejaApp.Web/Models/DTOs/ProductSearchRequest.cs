using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Web.Models.DTOs
{
    /// <summary>
    /// DTO para requisições de busca de produtos
    /// </summary>
    public class ProductSearchRequest
    {
        /// <summary>
        /// Termo de busca para nome, descrição ou categoria
        /// </summary>
        public string? SearchTerm { get; set; }
        
        /// <summary>
        /// Preço mínimo para filtro
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Preço mínimo deve ser maior ou igual a zero")]
        public decimal? MinPrice { get; set; }
        
        /// <summary>
        /// Preço máximo para filtro
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Preço máximo deve ser maior ou igual a zero")]
        public decimal? MaxPrice { get; set; }
        
        /// <summary>
        /// ID da categoria para filtro
        /// </summary>
        public int? CategoryId { get; set; }
        
        /// <summary>
        /// Filtro por status ativo/inativo
        /// </summary>
        public bool? IsActive { get; set; }
    }
}