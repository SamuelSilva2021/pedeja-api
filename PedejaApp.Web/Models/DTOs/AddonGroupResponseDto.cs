using PedejaApp.Domain.Enuns;

namespace PedejaApp.Web.Models.DTOs
{
    public class AddonGroupResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public AddonGroupType Type { get; set; }
        public int? MinSelections { get; set; }
        public int? MaxSelections { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public List<AddonResponseDto> Addons { get; set; } = new();
    }
}
