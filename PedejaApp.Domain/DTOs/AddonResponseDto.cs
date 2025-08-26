namespace PedejaApp.Domain.DTOs
{
    /// <summary>
    /// DTO de resposta para adicional individual
    /// </summary>
    public class AddonResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public int AddonGroupId { get; set; }
        public bool IsActive { get; set; }
    }
}
