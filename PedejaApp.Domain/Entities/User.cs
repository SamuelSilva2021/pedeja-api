using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.Entities;

/// <summary>
/// Representa um usuário do sistema
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// ID do tenant (NULL para administradores da plataforma)
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Email único do usuário
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash da senha do usuário
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Primeiro nome do usuário
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Último nome do usuário
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Telefone do usuário
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// URL do avatar do usuário
    /// </summary>
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Status do usuário (active, inactive, suspended)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// Data de verificação do email
    /// </summary>
    public DateTime? EmailVerifiedAt { get; set; }

    /// <summary>
    /// Data do último login
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Configurações personalizadas do usuário
    /// </summary>
    public string Preferences { get; set; } = "{}";

    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Tenant ao qual o usuário pertence
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Indica se o usuário é administrador da plataforma
    /// </summary>
    public bool IsPlatformAdmin => TenantId == null;
}