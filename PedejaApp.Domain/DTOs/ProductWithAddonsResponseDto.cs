namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO de resposta para produto com adicionais
    /// </summary>
    public class ProductWithAddonsResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public List<AddonGroupResponseDto> AddonGroups { get; set; } = new();
    }
}
