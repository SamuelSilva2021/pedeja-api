using PedejaApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Web.Models.DTOs
{
    public class UpdateOrderStatusRequestDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
