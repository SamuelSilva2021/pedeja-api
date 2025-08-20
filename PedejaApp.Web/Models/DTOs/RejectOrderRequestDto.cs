using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Web.Models.DTOs
{
    public class RejectOrderRequestDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

}
