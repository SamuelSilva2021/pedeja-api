using System.ComponentModel.DataAnnotations;
using PedejaApp.Domain.Entities;

namespace PedejaApp.Application.Models;

public class UpdateOrderStatusRequest
{
    [Required]
    public OrderStatus Status { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
